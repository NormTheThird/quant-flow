namespace QuantFlow.Api.Rest.Models;

/// <summary>
/// Individual market data item for bulk import
/// </summary>
public class MarketDataImportItem
{
    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    public string Timeframe { get; set; } = string.Empty;

    [Required]
    public string DataSource { get; set; } = string.Empty;

    [Required]
    public decimal Open { get; set; } = 0.0m;

    [Required]
    public decimal High { get; set; } = 0.0m;

    [Required]
    public decimal Low { get; set; } = 0.0m;

    [Required]
    public decimal Close { get; set; } = 0.0m;

    [Required]
    public decimal Volume { get; set; } = 0.0m;

    public decimal? VWAP { get; set; } = null;

    public int? TradeCount { get; set; } = null;

    [Required]
    public DateTime Timestamp { get; set; } = new();
}