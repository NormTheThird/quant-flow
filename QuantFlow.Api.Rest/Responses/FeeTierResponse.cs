namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Fee tier response model
/// </summary>
public class FeeTierResponse
{
    public int TierLevel { get; set; } = 0;
    public decimal MinimumVolume { get; set; } = 0.0m;
    public decimal MakerFeePercentage { get; set; } = 0.0m;
    public decimal TakerFeePercentage { get; set; } = 0.0m;
}