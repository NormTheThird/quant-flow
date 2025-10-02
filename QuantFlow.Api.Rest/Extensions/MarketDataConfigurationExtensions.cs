namespace QuantFlow.Api.Rest.Extensions;

/// <summary>
/// Extension methods for mapping between MarketDataConfiguration requests and models
/// </summary>
public static class MarketDataConfigurationExtensions
{
    /// <summary>
    /// Maps CreateMarketDataConfigurationRequest to MarketDataConfigurationModel
    /// </summary>
    public static MarketDataConfigurationModel ToModel(this CreateMarketDataConfigurationRequest request)
    {
        return new MarketDataConfigurationModel
        {
            SymbolId = request.SymbolId,
            Exchange = request.Exchange,
            Is1mActive = request.Is1mActive,
            Is5mActive = request.Is5mActive,
            Is15mActive = request.Is15mActive,
            Is1hActive = request.Is1hActive,
            Is4hActive = request.Is4hActive,
            Is1dActive = request.Is1dActive
        };
    }

    /// <summary>
    /// Maps UpdateMarketDataConfigurationRequest to MarketDataConfigurationModel
    /// </summary>
    public static MarketDataConfigurationModel ToModel(this UpdateMarketDataConfigurationRequest request, Guid id)
    {
        return new MarketDataConfigurationModel
        {
            Id = id,
            Is1mActive = request.Is1mActive,
            Is5mActive = request.Is5mActive,
            Is15mActive = request.Is15mActive,
            Is1hActive = request.Is1hActive,
            Is4hActive = request.Is4hActive,
            Is1dActive = request.Is1dActive
        };
    }

    /// <summary>
    /// Maps MarketDataConfigurationModel to MarketDataConfigurationResponse
    /// </summary>
    public static MarketDataConfigurationResponse ToResponse(this MarketDataConfigurationModel model)
    {
        return new MarketDataConfigurationResponse
        {
            Id = model.Id,
            SymbolId = model.SymbolId,
            SymbolName = model.SymbolName,
            Exchange = model.Exchange,
            Is1mActive = model.Is1mActive,
            Is5mActive = model.Is5mActive,
            Is15mActive = model.Is15mActive,
            Is1hActive = model.Is1hActive,
            Is4hActive = model.Is4hActive,
            Is1dActive = model.Is1dActive,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Maps collection of MarketDataConfigurationModel to collection of responses
    /// </summary>
    public static IEnumerable<MarketDataConfigurationResponse> ToResponses(
        this IEnumerable<MarketDataConfigurationModel> models)
    {
        return models.Select(m => m.ToResponse());
    }
}