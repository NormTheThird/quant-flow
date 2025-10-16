namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for configuring SQL Server data services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server data store services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSqlServerDataStore(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the interceptor
        services.AddSingleton<ChangeTrackerClearingInterceptor>();

        // Add Entity Framework DbContext with interceptor
        services.AddDbContext<QuantFlowDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<ChangeTrackerClearingInterceptor>();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });

        // Register repository implementations
        services.AddScoped<IAlgorithmPositionRepository, AlgorithmPositionRepository>();
        services.AddScoped<IBacktestRunRepository, BacktestRunRepository>();
        services.AddScoped<IMarketDataConfigurationRepository, MarketDataConfigurationRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ISymbolRepository, SymbolRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IUserExchangeDetailsRepository, UserExchangeDetailsRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}