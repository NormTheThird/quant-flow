namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// InfluxDB measurement for monitoring data quality and completeness
/// </summary>
[Measurement("data_quality")]
public class DataQualityPoint : BaseTimeSeriesPoint
{
    // Tags
    [Column("symbol", IsTag = true)]
    public string Symbol { get; set; } = string.Empty;

    [Column("exchange", IsTag = true)]
    public string Exchange { get; set; } = string.Empty;

    [Column("timeframe", IsTag = true)]
    public string Timeframe { get; set; } = string.Empty;

    [Column("data_source", IsTag = true)]
    public string DataSource { get; set; } = string.Empty;

    [Column("quality_check", IsTag = true)]
    public string QualityCheck { get; set; } = string.Empty; // completeness, accuracy, timeliness

    // Quality Fields
    [Column("quality_score")]
    public decimal QualityScore { get; set; } = 0.0m; // 0.0 to 1.0

    [Column("expected_records")]
    public int ExpectedRecords { get; set; } = 0;

    [Column("actual_records")]
    public int ActualRecords { get; set; } = 0;

    [Column("missing_records")]
    public int MissingRecords { get; set; } = 0;

    [Column("duplicate_records")]
    public int DuplicateRecords { get; set; } = 0;

    [Column("invalid_records")]
    public int InvalidRecords { get; set; } = 0;

    [Column("data_gaps")]
    public int DataGaps { get; set; } = 0;

    [Column("max_gap_minutes")]
    public int MaxGapMinutes { get; set; } = 0;

    [Column("avg_latency_ms")]
    public decimal AvgLatencyMs { get; set; } = 0.0m;

    [Column("max_latency_ms")]
    public decimal MaxLatencyMs { get; set; } = 0.0m;

    [Column("data_freshness_minutes")]
    public int DataFreshnessMinutes { get; set; } = 0;

    [Column("error_count")]
    public int ErrorCount { get; set; } = 0;

    [Column("warning_count")]
    public int WarningCount { get; set; } = 0;
}