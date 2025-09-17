namespace QuantFlow.Common.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the rate limit handler with configuration.
    /// </summary>
    public static IServiceCollection AddRateLimitHandling(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind the configuration to the settings object
        var rateLimitSettings = new RateLimitSettings();
        configuration.GetSection(RateLimitSettings.SectionName).Bind(rateLimitSettings);

        // Register as singleton instance
        services.AddSingleton(Options.Create(rateLimitSettings));

        // Register the rate limit handler as singleton
        services.AddSingleton<IApiRateLimitHandler, ApiRateLimitHandler>();

        return services;
    }

    /// <summary>
    /// Configures complete application configuration including JSON files, environment variables, user secrets, and Vault.
    /// </summary>
    /// <param name="config">Configuration builder.</param>
    /// <param name="environment">Host environment.</param>
    /// <param name="args">Command line arguments.</param>
    /// <typeparam name="TProgram">Program type for user secrets.</typeparam>
    public static IConfigurationBuilder AddQuantFlowConfiguration<TProgram>(this IConfigurationBuilder config, IHostEnvironment environment, string[] args)
        where TProgram : class
    {
        config.AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        if (environment.IsDevelopment())
            config.AddUserSecrets<TProgram>();

        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? 
            throw new InvalidOperationException("VAULT_TOKEN environment variable must be set");

        if (environment.IsDevelopment())
            config.AddVault("http://vault.local:30420", "quant-flow", vaultToken);
        else
            config.AddVault("http://vault.vault.svc.cluster.local:8200", "quant-flow", vaultToken);

        return config;
    }

    /// <summary>
    /// Configures Serilog logging with console and optional Grafana Loki output.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="context">Host builder context (for config and environment).</param>
    /// <param name="applicationName">Application name to enrich logs.</param>
    public static IServiceCollection AddSerilog(this IServiceCollection services, HostBuilderContext context, string applicationName)
    {
        // Default Loki app label to a lowercase version of the application name if not provided
        var lokiAppLabel = $"quantflow-{applicationName.ToLowerInvariant()}";

        // Build logger
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .ApplyLokiSink(context, lokiAppLabel)
            .CreateLogger();

        // Add Serilog to the logging pipeline
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });

        return services;
    }

    /// <summary>
    /// Internal helper to add Grafana Loki sink if configured.
    /// </summary>
    private static LoggerConfiguration ApplyLokiSink(this LoggerConfiguration loggerConfiguration, HostBuilderContext context, string lokiAppLabel)
    {
        var lokiUrl = context.Configuration["Loki:Url"];
        if (!string.IsNullOrEmpty(lokiUrl))
        {
            loggerConfiguration.WriteTo.GrafanaLoki(
                uri: lokiUrl,
                labels: new[]
                {
                    new LokiLabel { Key = "app", Value = lokiAppLabel },
                    new LokiLabel { Key = "env", Value = context.HostingEnvironment.EnvironmentName.ToLowerInvariant() }
                });
        }

        return loggerConfiguration;
    }
}