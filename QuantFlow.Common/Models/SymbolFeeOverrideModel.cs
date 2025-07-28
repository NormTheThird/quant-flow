namespace QuantFlow.Common.Models;

/// <summary>
/// Represents symbol-specific fee overrides for an exchange
/// </summary>
public class SymbolFeeOverrideModel : BaseModel
{
    public Guid ExchangeConfigurationId { get; set; }
    public Exchange Exchange { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal MakerFeePercent { get; set; }
    public decimal TakerFeePercent { get; set; }
    public bool IsActive { get; set; } = true;
    public string Reason { get; set; } = string.Empty;

    // Navigation property
    public ExchangeConfigurationModel ExchangeConfiguration { get; set; } = null!;
}