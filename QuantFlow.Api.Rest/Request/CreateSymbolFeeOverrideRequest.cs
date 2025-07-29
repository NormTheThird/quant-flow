namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for creating symbol fee override
/// </summary>
public class CreateSymbolFeeOverrideRequest
{
    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Range(0, 1)]
    public decimal? MakerFeeOverride { get; set; } = null;

    [Range(0, 1)]
    public decimal? TakerFeeOverride { get; set; } = null;

    public string Reason { get; set; } = string.Empty;
}