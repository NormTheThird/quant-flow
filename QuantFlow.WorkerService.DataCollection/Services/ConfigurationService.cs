using QuantFlow.Common.Infrastructure;
using QuantFlow.Common.Infrastructure.Vault;
using QuantFlow.Common.Interfaces.Infrastructure;

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
            .UseSerilog(ConfigureSerilog)
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
                config.AddVault("http://vault.local:30420", "quant-flow", "root");
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

    /// <summary>
    /// Configures Serilog with console and Grafana Loki sinks
    /// </summary>
    private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("QuantFlow", LogEventLevel.Debug)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "QuantFlow.DataCollection")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Filter.ByExcluding(logEvent =>
                logEvent.Level == LogEventLevel.Information &&
                logEvent.RenderMessage().Contains("Content root path"))
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information);

        // Add Grafana Loki sink if configured
        var lokiUrl = context.Configuration.GetSection("Logging:Loki:Url").Value;
        if (!string.IsNullOrEmpty(lokiUrl))
        {
            loggerConfiguration.WriteTo.GrafanaLoki(
                uri: lokiUrl,
                labels: new[]
                {
                    new LokiLabel { Key = "app", Value = "quantflow-datacollection" },
                    new LokiLabel { Key = "env", Value = context.HostingEnvironment.EnvironmentName.ToLowerInvariant() }
                },
                restrictedToMinimumLevel: LogEventLevel.Information);
        }
    }
}