namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// InfluxDB measurement for price data
/// </summary>
[Measurement("prices")]
public class PricePoint : BaseTimeSeriesPoint
{
    // Tags (indexed fields)
    [Column("symbol", IsTag = true)]
    public string Symbol { get; set; } = string.Empty;

    [Column("exchange", IsTag = true)]
    public string Exchange { get; set; } = string.Empty;

    [Column("timeframe", IsTag = true)]
    public string Timeframe { get; set; } = string.Empty; // 1m, 5m, 1h, 1d

    [Column("data_source", IsTag = true)]
    public string DataSource { get; set; } = string.Empty;

    // Fields (actual data)
    [Column("open")]
    public decimal Open { get; set; } = 0.0m;

    [Column("high")]
    public decimal High { get; set; } = 0.0m;

    [Column("low")]
    public decimal Low { get; set; } = 0.0m;

    [Column("close")]
    public decimal Close { get; set; } = 0.0m;

    [Column("volume")]
    public decimal Volume { get; set; } = 0.0m;

    [Column("vwap")]
    public decimal? VWAP { get; set; } = null;

    [Column("trade_count")]
    public int? TradeCount { get; set; } = null;

    [Column("bid")]
    public decimal? Bid { get; set; } = null;

    [Column("ask")]
    public decimal? Ask { get; set; } = null;

    [Column("spread")]
    public decimal? Spread { get; set; } = null;
}