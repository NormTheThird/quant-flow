namespace QuantFlow.Domain.Extensions;

/// <summary>
/// Extension methods for registering domain services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds domain services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register API services
        services.AddScoped<IKrakenApiService, KrakenApiService>();

        // Register Domain services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        //services.AddScoped<IExchangeConfigurationService, ExchangeConfigurationService>();
        services.AddScoped<IKrakenMarketDataCollectionService, KrakenMarketDataCollectionService>();

        return services;
    }
}