namespace QuantFlow.Common.Models;

/// <summary>
/// Represents ticker data for a trading pair
/// </summary>
public class TickerData
{
    public string Symbol { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public decimal Volume24h { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
}