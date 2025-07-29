namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Market data response model
/// </summary>
public class MarketDataResponse
{
    public string Symbol { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public decimal Open { get; set; } = 0.0m;
    public decimal High { get; set; } = 0.0m;
    public decimal Low { get; set; } = 0.0m;
    public decimal Close { get; set; } = 0.0m;
    public decimal Volume { get; set; } = 0.0m;
    public decimal? VWAP { get; set; } = null;
    public int? TradeCount { get; set; } = null;
    public DateTime Timestamp { get; set; } = new();
}