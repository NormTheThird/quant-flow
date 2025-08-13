namespace QuantFlow.Data.InfluxDB.Extensions;

/// <summary>
/// Extension methods for configuring InfluxDB services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds InfluxDB data store with all repositories
    /// </summary>
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

        // Register InfluxDB client as singleton
        services.AddSingleton<InfluxDBClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InfluxDBClient>>();
            logger.LogInformation("Creating InfluxDB client for {Url}", influxUrl);
            return new InfluxDBClient(influxUrl, influxToken);
        });

        // Register InfluxDB context as scoped
        services.AddScoped<InfluxDbContext>(provider =>
        {
            var client = provider.GetRequiredService<InfluxDBClient>();
            var logger = provider.GetRequiredService<ILogger<InfluxDbContext>>();
            return new InfluxDbContext(client, influxBucket, influxOrg, logger);
        });

        // Register ALL repositories
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<IAlgorithmPerformanceRepository, AlgorithmPerformanceRepository>();
        services.AddScoped<ISystemMetricsRepository, SystemMetricsRepository>();
        services.AddScoped<IDataQualityRepository, DataQualityRepository>();

        return services;
    }
}