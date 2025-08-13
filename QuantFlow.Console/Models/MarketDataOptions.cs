namespace QuantFlow.Console.Models;

/// <summary>
/// Options for market data commands
/// </summary>
public class MarketDataOptions
{
    public string Action { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public Timeframe Timeframe { get; set; } = Timeframe.Unknown;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public Exchange Exchange { get; set; } = Exchange.Unknown;
    public int? Limit { get; set; } = null;
}