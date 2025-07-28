namespace QuantFlow.Common.Models;

/// <summary>
/// Information about data availability for a symbol
/// </summary>
public class DataAvailabilityInfo
{
    public string Symbol { get; set; } = string.Empty;
    public string? Exchange { get; set; } = null;
    public DateTime? EarliestDataPoint { get; set; } = null;
    public DateTime? LatestDataPoint { get; set; } = null;
    public TimeSpan? TotalDataSpan { get; set; } = null;
    public Dictionary<string, TimeframeAvailability> TimeframeAvailability { get; set; } = [];
    public long TotalDataPoints { get; set; } = 0;
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}