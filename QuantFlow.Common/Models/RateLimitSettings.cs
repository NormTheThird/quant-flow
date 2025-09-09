namespace QuantFlow.Common.Models;

/// <summary>
/// Simple configuration settings for rate limiting
/// No interface needed - this is just data
/// </summary>
public class RateLimitSettings
{
    public const string SectionName = "RateLimit";

    /// <summary>
    /// Maximum number of retry attempts (default: 5)
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Base delay for general retries in milliseconds (default: 1 second)
    /// </summary>
    public int BaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Initial delay for rate limit errors in milliseconds (default: 10 seconds)
    /// </summary>
    public int RateLimitDelayMs { get; set; } = 10000;

    /// <summary>
    /// Maximum delay cap in milliseconds (default: 5 minutes)
    /// </summary>
    public int MaxDelayMs { get; set; } = 300000;

    /// <summary>
    /// Minimum interval between API calls in milliseconds (default: 1.2 seconds)
    /// </summary>
    public int MinIntervalBetweenCallsMs { get; set; } = 1200;
}