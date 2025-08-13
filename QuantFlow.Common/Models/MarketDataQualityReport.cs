namespace QuantFlow.Common.Models;

/// <summary>
/// Report containing market data quality validation results
/// </summary>
public class MarketDataQualityReport
{
    public string Symbol { get; set; } = string.Empty;
    public Timeframe Timeframe { get; set; } = Timeframe.Unknown;
    public Exchange Exchange { get; set; } = Exchange.Unknown;
    public DateTime StartDate { get; set; } = new();
    public DateTime EndDate { get; set; } = new();
    public int TotalDataPoints { get; set; } = 0;
    public int ExpectedDataPoints { get; set; } = 0;
    public decimal DataCompleteness { get; set; } = 0.0m;
    public int InvalidPriceRelationships { get; set; } = 0;
    public int ZeroVolumeCandles { get; set; } = 0;
    public int DuplicateTimestamps { get; set; } = 0;
    public List<DataGap> Gaps { get; set; } = [];
    public List<string> ValidationErrors { get; set; } = [];
    public bool IsValid { get; set; } = true;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}