namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing data quality metrics
/// </summary>
public class DataQualityModel
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string QualityCheck { get; set; } = string.Empty;
    public decimal QualityScore { get; set; } = 0.0m;
    public int ExpectedRecords { get; set; } = 0;
    public int ActualRecords { get; set; } = 0;
    public int MissingRecords { get; set; } = 0;
    public int DuplicateRecords { get; set; } = 0;
    public int InvalidRecords { get; set; } = 0;
    public int DataGaps { get; set; } = 0;
    public int MaxGapMinutes { get; set; } = 0;
    public decimal AvgLatencyMs { get; set; } = 0.0m;
    public decimal MaxLatencyMs { get; set; } = 0.0m;
    public int DataFreshnessMinutes { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    public int WarningCount { get; set; } = 0;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}