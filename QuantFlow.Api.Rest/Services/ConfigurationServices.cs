namespace QuantFlow.Api.Rest.Services;

/// <summary>
/// Configuration services for setting up the API application
/// </summary>
public static class ConfigurationServices
{
    /// <summary>
    /// Configures all application services
    /// </summary>
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Host.ConfigureServices((context, services) =>
        {
            services.AddSerilog(context, "Api");
        });

        builder.AddApiVersioning();
        builder.AddDataStores();
        builder.AddDomainServices();
        builder.AddAuthentication();
        builder.AddApiServices();
        builder.AddSwagger();
    }

    /// <summary>
    /// Configures application configuration sources for WebApplicationBuilder
    /// </summary>
    public static void ConfigureAppConfiguration(this WebApplicationBuilder builder, string[] args)
    {
        // Clear existing sources to have full control
        builder.Configuration.Sources.Clear();

        // Add configuration sources in order
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        // Add User Secrets in Development
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
            // Add Vault for local development
            builder.Configuration.AddVault("http://vault.local:30420", "quant-flow", "root");
        }
        else
        {
            // Add Vault for production
            builder.Configuration.AddVault("http://vault.vault.svc.cluster.local:8200", "quant-flow", "root");
        }
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
    /// Adds data store services
    /// </summary>
    private static void AddDataStores(this WebApplicationBuilder builder)
    {
        var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var influxUrl = builder.Configuration.GetSection("InfluxDb:Url").Value;

        if (!string.IsNullOrEmpty(sqlConnectionString))
        {
            builder.Services.AddSqlServerDataStore(builder.Configuration);
        }

        if (!string.IsNullOrEmpty(influxUrl))
        {
            builder.Services.AddInfluxDataStore(builder.Configuration);
        }
    }

    /// <summary>
    /// Adds domain services
    /// </summary>
    private static void AddDomainServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMarketDataService, MarketDataService>();
        builder.Services.AddScoped<IExchangeConfigurationService, ExchangeConfigurationService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
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
        if (app.Environment.IsDevelopment())
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
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors("CORSPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
    }
}