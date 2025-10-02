namespace QuantFlow.Api.Rest.Request.Symbol;

/// <summary>
/// Request model for updating an existing symbol
/// </summary>
public class UpdateSymbolRequest
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

    public bool IsActive { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinTradeAmount { get; set; }

    [Range(0, 18)]
    public int PricePrecision { get; set; }

    [Range(0, 18)]
    public int QuantityPrecision { get; set; }
}