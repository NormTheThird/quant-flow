namespace QuantFlow.Common.Models;

/// <summary>
/// Represents exchange configuration with base fee structures
/// </summary>
public class ExchangeConfigurationModel : BaseModel
{
    public Exchange Exchange { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsSupported { get; set; } = true;
    public decimal BaseMakerFeePercent { get; set; } = 0.0m;
    public decimal BaseTakerFeePercent { get; set; } = 0.0m;
    public string ApiEndpoint { get; set; } = string.Empty;
    public int MaxRequestsPerMinute { get; set; } = 60;

    // Navigation properties
    public List<FeeTierModel> FeeTiers { get; set; } = [];
    public List<SymbolFeeOverrideModel> SymbolOverrides { get; set; } = [];
}