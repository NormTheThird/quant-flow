namespace QuantFlow.Common.Models;

/// <summary>
/// Represents volume-based fee tiers for an exchange
/// </summary>
public class FeeTierModel : BaseModel
{
    public Guid ExchangeConfigurationId { get; set; }
    public Exchange Exchange { get; set; }
    public int TierLevel { get; set; }
    public decimal MinimumVolumeThreshold { get; set; }
    public decimal MakerFeePercent { get; set; }
    public decimal TakerFeePercent { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ExchangeConfigurationModel ExchangeConfiguration { get; set; } = null!;
}