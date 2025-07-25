namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// Base class for all InfluxDB time-series measurements
/// </summary>
public abstract class BaseTimeSeriesPoint
{
    /// <summary>
    /// Timestamp of the measurement
    /// </summary>
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}