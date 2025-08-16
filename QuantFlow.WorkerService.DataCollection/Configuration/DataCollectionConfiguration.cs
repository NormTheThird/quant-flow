namespace QuantFlow.WorkerService.DataCollection.Configuration;

/// <summary>
/// Data collection configuration with hardcoded values optimized for production
/// Eventually this will be stored in SQL database for dynamic configuration
/// </summary>
public class DataCollectionConfiguration
{
    public const string SectionName = "DataCollection";

    /// <summary>
    /// Trading symbols to collect data for
    /// </summary>
    public List<string> Symbols { get; set; } = new()
    {
        "BTCUSDT",   // Bitcoin
        "ETHUSDT",   // Ethereum
        "SOLUSDT",   // Solana
        "ADAUSDT",   // Cardano
        "AVAXUSDT",  // Avalanche
        "DOTUSDT",   // Polkadot
        "LINKUSDT",  // Chainlink
        "ATOMUSDT",  // Cosmos
        "ALGOUSDT",  // Algorand
        "XTZUSDT",   // Tezos
    };

    /// <summary>
    /// Exchanges to collect data from
    /// </summary>
    public List<string> Exchanges { get; set; } = new()
    {
        "kraken"
    };

    /// <summary>
    /// All supported timeframes
    /// </summary>
    public List<string> Timeframes { get; set; } = new()
    {
        "1m",
        "5m",
        "15m",
        "1h",
        "4h",
        "1d"
    };

    /// <summary>
    /// Maximum concurrent collections (not used in optimized sequential approach)
    /// </summary>
    public int MaxConcurrentCollections { get; set; } = 1;

    /// <summary>
    /// Number of retry attempts for failed API calls
    /// </summary>
    public int RetryAttempts { get; set; } = 1;

    /// <summary>
    /// Delay between retry attempts in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Batch size for data processing
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// API timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Health check interval in minutes
    /// </summary>
    public int HealthCheckIntervalMinutes { get; set; } = 5;
}