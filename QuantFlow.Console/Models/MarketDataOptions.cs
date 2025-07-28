namespace QuantFlow.Console.Models;

/// <summary>
/// Options for market data commands
/// </summary>
public class MarketDataOptions
{
    public string? Action { get; set; }
    public string? Symbol { get; set; }
    public string? Timeframe { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Exchange { get; set; }
    public int? Limit { get; set; }
}