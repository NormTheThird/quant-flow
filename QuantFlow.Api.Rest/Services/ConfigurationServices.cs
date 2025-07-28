namespace QuantFlow.Api.Rest.Services;

public static class ConfigurationServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        //builder.Services.AddAuthorization(options =>
        //    options.AddPolicy("ApiKeyPolicy", policy => policy.RequireClaim("ApiKey")));

        //builder.Services.AddAuthentication("ApiKey")
        //    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(options =>
        //    {
        //        options.SaveToken = true;
        //        options.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuer = false, // TODO: TREY: Figure out how to set this to true
        //            ValidateAudience = false, // TODO: TREY: Figure out how to set this to true
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //            ValidAudience = builder.Configuration["Jwt:Audience"],
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        //        };
        //    });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "CORSPolicy",
                policy => { policy.AllowAnyOrigin(); policy.AllowAnyMethod(); policy.AllowAnyHeader(); });
        });

        builder.Services.AddOpenApi();
        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
}