namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for updating exchange fees
/// </summary>
public class UpdateExchangeFeesRequest
{
    [Required]
    [Range(0, 1)]
    public decimal MakerFee { get; set; } = 0.0m;

    [Required]
    [Range(0, 1)]
    public decimal TakerFee { get; set; } = 0.0m;
}