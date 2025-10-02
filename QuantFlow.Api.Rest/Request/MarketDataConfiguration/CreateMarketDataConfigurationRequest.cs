namespace QuantFlow.Api.Rest.Request.MarketDataConfiguration;

/// <summary>
/// Request model for creating a new market data configuration
/// </summary>
public class CreateMarketDataConfigurationRequest
{
    [Required]
    public Guid SymbolId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Exchange { get; set; } = string.Empty;

    public bool Is1mActive { get; set; } = false;
    public bool Is5mActive { get; set; } = false;
    public bool Is15mActive { get; set; } = false;
    public bool Is1hActive { get; set; } = false;
    public bool Is4hActive { get; set; } = false;
    public bool Is1dActive { get; set; } = false;
}