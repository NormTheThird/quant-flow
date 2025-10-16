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
        services.AddScoped<IAlgorithmPositionService, AlgorithmPositionService>();
        services.AddScoped<IAlgorithmService, AlgorithmService>();
        services.AddScoped<IAlgorithmExecutionService, AlgorithmExecutionService>();
        services.AddScoped<IBacktestService, BacktestService>();
        services.AddScoped<IBacktestExecutionService, BacktestExecutionService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IHoldingsService, HoldingsService>();
        services.AddScoped<IKrakenMarketDataCollectionService, KrakenMarketDataCollectionService>();
        services.AddScoped<IMarketDataConfigurationService, MarketDataConfigurationService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<ISymbolService, SymbolService>();
        services.AddScoped<IUserExchangeDetailsService, UserExchangeDetailsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITradeService, TradeService>();

        return services;
    }
}