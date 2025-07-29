namespace QuantFlow.Data.InfluxDB.Extensions;

/// <summary>
/// Extension methods for configuring InfluxDB services
/// </summary>
public static class InfluxServiceCollectionExtensions
{
    public static IServiceCollection AddInfluxDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<InfluxDBClient>(serviceProvider =>
        {
            var url = configuration.GetSection("InfluxDb:Url").Value
                ?? throw new InvalidOperationException("InfluxDb:Url configuration is required");
            var token = configuration.GetSection("InfluxDb:Token").Value
                ?? throw new InvalidOperationException("InfluxDb:Token configuration is required");

            var logger = serviceProvider.GetRequiredService<ILogger<InfluxDBClient>>();
            logger.LogInformation("Configuring InfluxDB client for URL: {Url}", url);

            return new InfluxDBClient(url, token);
        });

        services.AddScoped<InfluxDbContext>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<InfluxDBClient>();
            var bucket = configuration.GetSection("InfluxDb:Bucket").Value
                ?? throw new InvalidOperationException("InfluxDb:Bucket configuration is required");
            var organization = configuration.GetSection("InfluxDb:Organization").Value
                ?? throw new InvalidOperationException("InfluxDb:Organization configuration is required");
            var logger = serviceProvider.GetRequiredService<ILogger<InfluxDbContext>>();

            return new InfluxDbContext(client, bucket, organization, logger);
        });

        return services;
    }

    public static IServiceCollection AddInfluxDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<IAlgorithmPerformanceRepository, AlgorithmPerformanceRepository>();
        services.AddScoped<ISystemMetricsRepository, SystemMetricsRepository>();
        services.AddScoped<IDataQualityRepository, DataQualityRepository>();

        return services;
    }

    public static IServiceCollection AddInfluxDataStore(this IServiceCollection services, IConfiguration configuration)
    {
        // Validate configuration first
        var influxUrl = configuration.GetSection("InfluxDb:Url").Value;
        var influxToken = configuration.GetSection("InfluxDb:Token").Value;
        var influxBucket = configuration.GetSection("InfluxDb:Bucket").Value;
        var influxOrg = configuration.GetSection("InfluxDb:Organization").Value;

        if (string.IsNullOrEmpty(influxUrl) || string.IsNullOrEmpty(influxToken) ||
            string.IsNullOrEmpty(influxBucket) || string.IsNullOrEmpty(influxOrg))
        {
            throw new InvalidOperationException("InfluxDB configuration is incomplete");
        }

        // Register InfluxDB client as singleton with proper disposal
        services.AddSingleton<InfluxDBClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InfluxDBClient>>();
            logger.LogInformation("Creating InfluxDB client for {Url}", influxUrl);

            return new InfluxDBClient(influxUrl, influxToken);
        });

        // Register InfluxDB context as scoped to match request lifetime
        services.AddScoped<InfluxDbContext>(provider =>
        {
            var client = provider.GetRequiredService<InfluxDBClient>();
            var logger = provider.GetRequiredService<ILogger<InfluxDbContext>>();

            return new InfluxDbContext(client, influxBucket, influxOrg, logger);
        });

        // Register repositories as scoped
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();

        return services;
    }

    public static IServiceCollection AddInfluxDb(this IServiceCollection services, string url, string token, string bucket, string organization)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(organization);

        services.AddSingleton<InfluxDBClient>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<InfluxDBClient>>();
            logger.LogInformation("Configuring InfluxDB client for URL: {Url}", url);

            return new InfluxDBClient(url, token);
        });

        services.AddScoped<InfluxDbContext>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<InfluxDBClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<InfluxDbContext>>();

            return new InfluxDbContext(client, bucket, organization, logger);
        });

        return services;
    }
}