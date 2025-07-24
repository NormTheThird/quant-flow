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
        // Add Entity Framework DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repository implementations
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IBacktestRunRepository, BacktestRunRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<ISymbolRepository, SymbolRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }

    /// <summary>
    /// Adds SQL Server data store services with custom connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The SQL Server connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSqlServerDataStore(this IServiceCollection services, string connectionString)
    {
        // Add Entity Framework DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register repository implementations
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IBacktestRunRepository, BacktestRunRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<ISymbolRepository, SymbolRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }
}