namespace QuantFlow.Data.InfluxDB.Extensions;

/// <summary>
/// Extension methods for configuring InfluxDB services
/// </summary>
public static class InfluxServiceCollectionExtensions2
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
        services.AddScoped<IMarketDataRepository, InfluxMarketDataRepository>();
        services.AddScoped<IAlgorithmPerformanceRepository, InfluxAlgorithmPerformanceRepository>();
        services.AddScoped<ISystemMetricsRepository, InfluxSystemMetricsRepository>();
        services.AddScoped<IDataQualityRepository, InfluxDataQualityRepository>();

        return services;
    }

    public static IServiceCollection AddCompleteInfluxDb(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddInfluxDb(configuration)
            .AddInfluxDbRepositories();
    }

    public static IServiceCollection AddInfluxDb(
        this IServiceCollection services,
        string url,
        string token,
        string bucket,
        string organization)
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