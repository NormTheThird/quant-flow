namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for bulk importing market data
/// </summary>
public class BulkImportMarketDataRequest
{
    [Required]
    public List<MarketDataImportItem> Data { get; set; } = [];
}