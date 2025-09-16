namespace QuantFlow.WorkerService.DataCollection.Services;

/// <summary>
/// Configuration service for setting up dependency injection and configuration
/// </summary>
public static class ConfigurationService
{
    /// <summary>
    /// Configures the host builder with all necessary services and configuration
    /// </summary>
    public static IHostBuilder ConfigureApplication(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration(args)
            .ConfigureServices();
    }

    /// <summary>
    /// Configures application configuration sources
    /// </summary>
    public static IHostBuilder ConfigureAppConfiguration(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            // Add User Secrets in Development
            if (context.HostingEnvironment.IsDevelopment())
            {
                config.AddUserSecrets<Program>();

                // Add Vault for local development
                config.AddVault("http://vault.local:30420", "quant-flow", "root");
            }
            else
            {
                // Add Vault for production
                config.AddVault("http://vault.vault.svc.cluster.local:8200", "quant-flow", "root");
            }
        });
    }

    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSerilog(context, "DataCollection");

            // Register configuration as singleton
            services.AddSingleton(context.Configuration);

            // Add the missing service registration
            services.AddScoped<IApiRateLimitHandler, ApiRateLimitHandler>();

            // Data stores
            services.AddSqlServerDataStore(context.Configuration);
            services.AddInfluxDataStore(context.Configuration);

            // Register hardcoded configuration classes as singletons
            // These will eventually be loaded from SQL database
            services.AddSingleton<DataCollectionConfiguration>();
            services.AddSingleton<CollectionScheduleConfiguration>();
            services.AddSingleton<ExchangeConfiguration>();

            // Register configurations for IOptions pattern compatibility
            services.AddSingleton<IOptionsMonitor<DataCollectionConfiguration>>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<DataCollectionConfiguration>();
                return new HardcodedOptionsMonitor<DataCollectionConfiguration>(config);
            });

            services.AddSingleton<IOptionsMonitor<CollectionScheduleConfiguration>>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<CollectionScheduleConfiguration>();
                return new HardcodedOptionsMonitor<CollectionScheduleConfiguration>(config);
            });

            services.AddSingleton<IOptionsMonitor<ExchangeConfiguration>>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<ExchangeConfiguration>();
                return new HardcodedOptionsMonitor<ExchangeConfiguration>(config);
            });

            // Domain services (includes IKrakenApiService and IKrakenMarketDataCollectionService)
            services.AddDomainServices();
            services.AddHttpClient();

            // Hosted services
            services.AddHostedService<MarketDataCollectionService>();

            // Application services that are NOT in AddDomainServices
            services.AddScoped<IDataCollectionOrchestrator, DataCollectionOrchestrator>();

            // Kraken credentials configuration
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var credentials = new KrakenCredentials();
                configuration.GetSection("Kraken").Bind(credentials);
                return credentials;
            });
        });
    }
}