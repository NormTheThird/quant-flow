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
            services.AddSqlServerDataStore(hostContext.Configuration);

            // InfluxDB configuration
            services.AddInfluxDataStore(hostContext.Configuration);

            // Core domain services
            services.AddScoped<IMarketDataService, MarketDataService>();
            services.AddScoped<IExchangeConfigurationService, ExchangeConfigurationService>();

            // Kraken API Service (your existing service)
            services.AddScoped<IKrakenApiService, KrakenApiService>();
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var credentials = new KrakenCredentials();
                configuration.GetSection("Kraken").Bind(credentials);
                return credentials;
            });

            // Market Data Collection Services
            services.AddScoped<IKrakenMarketDataCollectionService, KrakenMarketDataCollectionService>();
            services.AddScoped<GetMarketDataService>();

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
}