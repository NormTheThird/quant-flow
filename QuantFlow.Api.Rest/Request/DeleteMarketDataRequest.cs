using static QuantFlow.Api.Rest.Controllers.MarketDataController;

namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for deleting market data by criteria
/// </summary>
public class DeleteMarketDataRequest
{
    public List<DeleteCriteria> DeleteCriteria { get; set; } = [];
}