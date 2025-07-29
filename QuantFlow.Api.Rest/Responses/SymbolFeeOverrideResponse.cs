namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Symbol fee override response model
/// </summary>
public class SymbolFeeOverrideResponse
{
    public string Symbol { get; set; } = string.Empty;
    public decimal? MakerFeeOverride { get; set; } = null;
    public decimal? TakerFeeOverride { get; set; } = null;
    public string Reason { get; set; } = string.Empty;
}