namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing market data collection configuration
/// </summary>
public class MarketDataConfigurationModel : BaseModel
{
    public Guid SymbolId { get; set; } = Guid.Empty;
    public string SymbolName { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public bool Is1mActive { get; set; } = false;
    public bool Is5mActive { get; set; } = false;
    public bool Is15mActive { get; set; } = false;
    public bool Is1hActive { get; set; } = false;
    public bool Is4hActive { get; set; } = false;
    public bool Is1dActive { get; set; } = false;
}