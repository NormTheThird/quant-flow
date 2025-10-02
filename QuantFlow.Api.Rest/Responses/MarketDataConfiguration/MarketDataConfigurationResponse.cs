namespace QuantFlow.Api.Rest.Responses.MarketDataConfiguration;

/// <summary>
/// Response model for market data configuration
/// </summary>
public class MarketDataConfigurationResponse
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid SymbolId { get; set; } = Guid.Empty;
    public string SymbolName { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public bool Is1mActive { get; set; } = false;
    public bool Is5mActive { get; set; } = false;
    public bool Is15mActive { get; set; } = false;
    public bool Is1hActive { get; set; } = false;
    public bool Is4hActive { get; set; } = false;
    public bool Is1dActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}