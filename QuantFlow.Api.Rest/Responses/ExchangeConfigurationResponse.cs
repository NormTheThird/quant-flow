namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Exchange configuration response model
/// </summary>
public class ExchangeConfigurationResponse
{
    public string Name { get; set; } = string.Empty;
    public Exchange Exchange { get; set; } = Exchange.Kraken;
    public decimal MakerFee { get; set; } = 0.0m;
    public decimal TakerFee { get; set; } = 0.0m;
    public bool IsSupported { get; set; } = false;
    public List<FeeTierResponse> FeeTiers { get; set; } = [];
    public List<SymbolFeeOverrideResponse> SymbolOverrides { get; set; } = [];
}