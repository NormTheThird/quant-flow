namespace QuantFlow.WorkerService.DataCollection.Configuration;

/// <summary>
/// Exchange configuration with hardcoded values
/// Eventually this will be stored in SQL database for dynamic configuration
/// </summary>
public class ExchangeConfiguration
{
    public const string SectionName = "Exchanges";

    /// <summary>
    /// All configured exchanges with their settings
    /// </summary>
    public Dictionary<string, ExchangeSettings> Exchanges { get; set; } = new()
    {
        ["Kraken"] = new ExchangeSettings
        {
            BaseUrl = "https://api.kraken.com",
            RateLimitPerMinute = 180,
            Enabled = true
        },
        ["Kucoin"] = new ExchangeSettings
        {
            BaseUrl = "https://api.kucoin.com",
            RateLimitPerMinute = 300,
            Enabled = true
        }
    };
}

/// <summary>
/// Individual exchange settings
/// </summary>
public class ExchangeSettings
{
    /// <summary>
    /// Base API URL for the exchange
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Maximum API calls allowed per minute
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Whether this exchange is currently enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}