namespace QuantFlow.Api.Rest.Request.MarketDataConfiguration;

/// <summary>
/// Request model for updating a configuration
/// </summary>
public class UpdateMarketDataConfigurationRequest
{
    public bool Is1mActive { get; set; } = false;
    public bool Is5mActive { get; set; } = false;
    public bool Is15mActive { get; set; } = false;
    public bool Is1hActive { get; set; } = false;
    public bool Is4hActive { get; set; } = false;
    public bool Is1dActive { get; set; } = false;
}