namespace QuantFlow.Console.Models;

/// <summary>
/// Options for market data commands
/// </summary>
public class MarketDataOptions
{
    public string? Action { get; set; } = null;
    public string? Symbol { get; set; } = null;
    public string? Timeframe { get; set; } = null;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public string? DataSource { get; set; } = null; // Changed from Exchange
    public int? Limit { get; set; } = null;
}