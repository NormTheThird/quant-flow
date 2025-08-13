namespace QuantFlow.WorkerService.DataCollection.Configuration;

public class DataCollectionConfiguration
{
    public const string SectionName = "DataCollection";

    public List<string> Symbols { get; set; } = new() { "BTCUSDT", "ETHUSDT", "ADAUSDT" };
    public List<string> Exchanges { get; set; } = new() { "kraken", "kucoin" };
    public List<string> Timeframes { get; set; } = new() { "1m", "5m", "15m", "1h", "4h", "1d" };
    public int MaxConcurrentCollections { get; set; } = 3;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public bool EnableHealthChecks { get; set; } = true;
    public int HealthCheckIntervalMinutes { get; set; } = 5;
}