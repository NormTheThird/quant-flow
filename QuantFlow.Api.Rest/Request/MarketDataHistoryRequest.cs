namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for getting market data history
/// </summary>
public class MarketDataHistoryRequest
{
    [Required]
    public string Symbol { get; set; } = string.Empty;

    public string? Exchange { get; set; } = null;

    [Required]
    public string Timeframe { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; } = new();

    [Required]
    public DateTime EndDate { get; set; } = new();

    public int? Limit { get; set; } = null;
}