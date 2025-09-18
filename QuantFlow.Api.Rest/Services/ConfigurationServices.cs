namespace QuantFlow.Api.Rest.Services;

/// <summary>
/// Configuration services for setting up the API application
/// </summary>
public static class ConfigurationServices
{
    /// <summary>
    /// Configures the host builder with all necessary services and configuration
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured web application builder for method chaining</returns>
    public static WebApplicationBuilder ConfigureApplication(this WebApplicationBuilder builder, string[] args)
    {
        builder.Configuration.AddQuantFlowConfiguration<Program>(builder.Environment, args);
        builder.ConfigureServices();

        return builder;
    }

    /// <summary>
    /// Configures all application services
    /// </summary>
    private static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Host.ConfigureServices((context, services) =>
        {
            services.AddSerilog(context, "Api");
            services.AddSqlServerDataStore(builder.Configuration);
            services.AddInfluxDataStore(builder.Configuration);

            services.AddScoped<IApiRateLimitHandler, ApiRateLimitHandler>();

            services.AddDomainServices();
        });

        builder.AddApiVersioning();
        builder.AddAuthentication();
        builder.AddApiServices();
        builder.AddSwagger();
    }

    /// <summary>
    /// Adds API versioning configuration
    /// </summary>
    private static void AddApiVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader()
            );
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    /// <summary>
    /// Configures authentication and authorization
    /// </summary>
    private static void AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("ApiKey")
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiKeyPolicy", policy => policy.RequireClaim("ApiKey"));
            options.AddPolicy("JwtPolicy", policy => policy.RequireAuthenticatedUser());
        });
    }

    /// <summary>
    /// Adds API-specific services
    /// </summary>
    private static void AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CORSPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddHealthChecks();
    }

    /// <summary>
    /// Configures Swagger/OpenAPI
    /// </summary>
    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "QuantFlow Public API",
                Version = "v1",
                Description = "Public API endpoints for automated cryptocurrency trading platform"
            });

            // Only include public APIs in Swagger
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                var routeTemplate = apiDesc.RelativePath ?? string.Empty;
                return routeTemplate.Contains("/pub/");
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "API Key authentication"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Bearer token authentication"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    /// <summary>
    /// Configures the application pipeline
    /// </summary>
    public static void ConfigurePipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuantFlow Public API v1");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
        });

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors("CORSPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
    }
}