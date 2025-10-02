namespace QuantFlow.Api.Rest.Request.MarketDataConfiguration;

/// <summary>
/// Request model for toggling an interval
/// </summary>
public class ToggleIntervalRequest
{
    [Required]
    public bool IsActive { get; set; } = false;
}