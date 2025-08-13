namespace QuantFlow.Common.Models;

/// <summary>
/// Represents symbol availability on exchanges in the QuantFlow system
/// </summary>
public class ExchangeSymbolModel : BaseModel
{
    public required Guid SymbolId { get; set; } = Guid.Empty;
    public required Exchange Exchange { get; set; } = Exchange.Unknown;
    public required string ExchangeSymbolName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime LastDataUpdate { get; set; } = new();
    public string ApiEndpoint { get; set; } = string.Empty;

    // Navigation property
    public SymbolModel Symbol { get; set; } = null!;
}