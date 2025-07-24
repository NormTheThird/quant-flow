namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a cryptocurrency trading pair in the QuantFlow system
/// </summary>
public class SymbolModel : BaseModel
{
    public required string Symbol { get; set; } = string.Empty;
    public required string BaseAsset { get; set; } = string.Empty;
    public required string QuoteAsset { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public decimal MinTradeAmount { get; set; } = 0.0m;
    public int PricePrecision { get; set; } = 8;
    public int QuantityPrecision { get; set; } = 8;

    // Navigation properties
    public List<ExchangeSymbolModel> ExchangeSymbols { get; set; } = [];
}