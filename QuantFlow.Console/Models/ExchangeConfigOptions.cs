namespace QuantFlow.Console.Models;

/// <summary>
/// Options for exchange config commands
/// </summary>
public class ExchangeConfigOptions
{
    public string? Action { get; set; }
    public string? Exchange { get; set; }
    public string? Symbol { get; set; }
    public decimal? MakerFee { get; set; }
    public decimal? TakerFee { get; set; }
    public int? TierLevel { get; set; }
    public decimal? Volume { get; set; }
    public string? Reason { get; set; }
}