namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a gap in market data
/// </summary>
public class DataGap
{
    public DateTime StartTime { get; set; } = new();
    public DateTime EndTime { get; set; } = new();
    public TimeSpan Duration { get; set; } = new();
    public int MissingDataPoints { get; set; } = 0;
    public string TimeframeInterval { get; set; } = string.Empty;
}