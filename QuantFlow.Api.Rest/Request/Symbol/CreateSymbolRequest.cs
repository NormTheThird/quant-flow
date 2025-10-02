namespace QuantFlow.Api.Rest.Request.Symbol;

/// <summary>
/// Request model for creating a new symbol
/// </summary>
public class CreateSymbolRequest
{
    [Required]
    [MaxLength(20)]
    public required string Symbol { get; set; }

    [Required]
    [MaxLength(10)]
    public required string BaseAsset { get; set; }

    [Required]
    [MaxLength(10)]
    public required string QuoteAsset { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, double.MaxValue)]
    public decimal MinTradeAmount { get; set; } = 0.0m;

    [Range(0, 18)]
    public int PricePrecision { get; set; } = 8;

    [Range(0, 18)]
    public int QuantityPrecision { get; set; } = 8;
}