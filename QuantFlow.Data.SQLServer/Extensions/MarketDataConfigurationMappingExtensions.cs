namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between MarketDataConfigurationModel and MarketDataConfigurationEntity
/// </summary>
public static class MarketDataConfigurationMappingExtensions
{
    /// <summary>
    /// Converts MarketDataConfigurationEntity to MarketDataConfigurationModel
    /// </summary>
    public static MarketDataConfigurationModel ToBusinessModel(this MarketDataConfigurationEntity entity)
    {
        return new MarketDataConfigurationModel
        {
            Id = entity.Id,
            SymbolId = entity.SymbolId,
            SymbolName = entity.Symbol?.Symbol ?? string.Empty,
            Exchange = entity.Exchange,
            Is1mActive = entity.Is1mActive,
            Is5mActive = entity.Is5mActive,
            Is15mActive = entity.Is15mActive,
            Is1hActive = entity.Is1hActive,
            Is4hActive = entity.Is4hActive,
            Is1dActive = entity.Is1dActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts MarketDataConfigurationModel to MarketDataConfigurationEntity
    /// </summary>
    public static MarketDataConfigurationEntity ToEntity(this MarketDataConfigurationModel model)
    {
        return new MarketDataConfigurationEntity
        {
            Id = model.Id,
            SymbolId = model.SymbolId,
            Exchange = model.Exchange,
            Is1mActive = model.Is1mActive,
            Is5mActive = model.Is5mActive,
            Is15mActive = model.Is15mActive,
            Is1hActive = model.Is1hActive,
            Is4hActive = model.Is4hActive,
            Is1dActive = model.Is1dActive,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of MarketDataConfigurationEntities to MarketDataConfigurationModels
    /// </summary>
    public static IEnumerable<MarketDataConfigurationModel> ToBusinessModels(this IEnumerable<MarketDataConfigurationEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}