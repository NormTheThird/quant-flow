namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between SymbolModel and SymbolEntity
/// </summary>
public static class SymbolMappingExtensions
{
    /// <summary>
    /// Converts SymbolEntity to SymbolModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>SymbolModel business object</returns>
    public static SymbolModel ToBusinessModel(this SymbolEntity entity)
    {
        return new SymbolModel
        {
            Id = entity.Id,
            Symbol = entity.Symbol,
            BaseAsset = entity.BaseAsset,
            QuoteAsset = entity.QuoteAsset,
            IsActive = entity.IsActive,
            MinTradeAmount = entity.MinTradeAmount,
            PricePrecision = entity.PricePrecision,
            QuantityPrecision = entity.QuantityPrecision,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts SymbolModel to SymbolEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>SymbolEntity for database operations</returns>
    public static SymbolEntity ToEntity(this SymbolModel model)
    {
        return new SymbolEntity
        {
            Id = model.Id,
            Symbol = model.Symbol,
            BaseAsset = model.BaseAsset,
            QuoteAsset = model.QuoteAsset,
            IsActive = model.IsActive,
            MinTradeAmount = model.MinTradeAmount,
            PricePrecision = model.PricePrecision,
            QuantityPrecision = model.QuantityPrecision,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy ?? "System",
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of SymbolEntities to SymbolModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<SymbolModel> ToBusinessModels(this IEnumerable<SymbolEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}