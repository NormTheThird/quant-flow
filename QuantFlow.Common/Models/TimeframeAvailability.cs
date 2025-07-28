namespace QuantFlow.Common.Models;

/// <summary>
/// Availability information for a specific timeframe
/// </summary>
public class TimeframeAvailability
{
    public string Timeframe { get; set; } = string.Empty;
    public DateTime? FirstAvailable { get; set; } = null;
    public DateTime? LastAvailable { get; set; } = null;
    public long DataPointCount { get; set; } = 0;
    public decimal CompletenessPercentage { get; set; } = 0.0m;
    public int GapCount { get; set; } = 0;
}