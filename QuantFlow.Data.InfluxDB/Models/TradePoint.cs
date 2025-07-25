namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// InfluxDB measurement for individual trade data
/// </summary>
[Measurement("trades")]
public class TradePoint : BaseTimeSeriesPoint
{
    // Tags
    [Column("symbol", IsTag = true)]
    public string Symbol { get; set; } = string.Empty;

    [Column("exchange", IsTag = true)]
    public string Exchange { get; set; } = string.Empty;

    [Column("side", IsTag = true)]
    public string Side { get; set; } = string.Empty; // buy, sell

    [Column("trade_type", IsTag = true)]
    public string TradeType { get; set; } = string.Empty; // market, limit, stop

    // Fields
    [Column("trade_id")]
    public string TradeId { get; set; } = string.Empty;

    [Column("price")]
    public decimal Price { get; set; } = 0.0m;

    [Column("quantity")]
    public decimal Quantity { get; set; } = 0.0m;

    [Column("value")]
    public decimal Value { get; set; } = 0.0m;

    [Column("fees")]
    public decimal Fees { get; set; } = 0.0m;

    [Column("execution_time_ms")]
    public int? ExecutionTimeMs { get; set; } = null;
}