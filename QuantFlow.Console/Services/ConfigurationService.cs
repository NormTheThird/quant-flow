namespace QuantFlow.Console.Services;

/// <summary>
/// Configuration service for setting up logging and dependency injection
/// </summary>
public static class ConfigurationService
{
    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton(hostContext.Configuration);

            // SQL Server configuration
            services.AddSqlDataStoreIfAvailable(hostContext.Configuration);

            // InfluxDB configuration
            services.AddInfluxDataStoreIfAvailable(hostContext.Configuration);

            // Core domain services
            services.AddScoped<IMarketDataService, MarketDataService>();
            services.AddScoped<IExchangeConfigurationService, ExchangeConfigurationService>();

            // CLI command dispatcher
            services.AddScoped<ICommandHandler, CommandHandler>();
        });
    }

    /// <summary>
    /// Configures Serilog logging for the application
    /// </summary>
    public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("QuantFlow", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .Filter.ByExcluding(logEvent =>
                    logEvent.Level == LogEventLevel.Information &&
                    logEvent.RenderMessage().Contains("Content root path"))
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);
        });
    }



    /// <summary>
    /// Adds SQL Server data store if configuration is present
    /// </summary>
    private static IServiceCollection AddSqlDataStoreIfAvailable(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnectionString = configuration.GetConnectionString("DefaultConnection");

        services = !string.IsNullOrEmpty(sqlConnectionString)
            ? services.AddSqlServerDataStore(configuration)
            : services.AddScoped<IExchangeConfigurationRepository, MockExchangeConfigurationRepository>();

        return services;
    }

    /// <summary>
    /// Adds InfluxDB data store if configuration is present
    /// </summary>
    private static IServiceCollection AddInfluxDataStoreIfAvailable(this IServiceCollection services, IConfiguration configuration)
    {
        var influxUrl = configuration.GetSection("InfluxDb:Url").Value;

        services = !string.IsNullOrEmpty(influxUrl)
            ? services.AddCompleteInfluxDb(configuration)
            : services.AddScoped<IMarketDataRepository, MockMarketDataRepository>();

        return services;
    }
}