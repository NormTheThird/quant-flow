namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a range of missing data intervals that need to be populated
/// </summary>
public class MissingDataRange
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int ExpectedDataPoints { get; set; }
    public string Timeframe { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}