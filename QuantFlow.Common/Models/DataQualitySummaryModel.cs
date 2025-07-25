namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing aggregated data quality summary
/// </summary>
public class DataQualitySummaryModel
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public decimal AvgQualityScore { get; set; } = 0.0m;
    public decimal MinQualityScore { get; set; } = 1.0m;
    public int TotalMissingRecords { get; set; } = 0;
    public int TotalDuplicateRecords { get; set; } = 0;
    public int TotalInvalidRecords { get; set; } = 0;
    public int TotalDataGaps { get; set; } = 0;
    public decimal AvgLatencyMs { get; set; } = 0.0m;
    public decimal MaxLatencyMs { get; set; } = 0.0m;
    public int TotalErrorCount { get; set; } = 0;
    public int TotalWarningCount { get; set; } = 0;
    public decimal CompletenessPercentage { get; set; } = 100.0m;
    public decimal AccuracyPercentage { get; set; } = 100.0m;
}