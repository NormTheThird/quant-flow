namespace QuantFlow.Common.Models;

/// <summary>
/// Result model for fee calculations
/// </summary>
public class FeeCalculationResult
{
    public Exchange Exchange { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal TradeValue { get; set; }
    public decimal MakerFeePercent { get; set; }
    public decimal TakerFeePercent { get; set; }
    public decimal MakerFeeAmount { get; set; }
    public decimal TakerFeeAmount { get; set; }
    public string FeeSource { get; set; } = string.Empty; // "Base", "Tier", "Override"
    public int? AppliedTierLevel { get; set; }
    public decimal? VolumeUsed { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}