namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between ExchangeSymbolModel and ExchangeSymbolEntity
/// </summary>
public static class ExchangeSymbolMappingExtensions
{
    /// <summary>
    /// Converts ExchangeSymbolEntity to ExchangeSymbolModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>ExchangeSymbolModel business object</returns>
    public static ExchangeSymbolModel ToBusinessModel(this ExchangeSymbolEntity entity)
    {
        return new ExchangeSymbolModel
        {
            Id = entity.Id,
            SymbolId = entity.SymbolId,
            Exchange = (Exchange)entity.Exchange,
            ExchangeSymbolName = entity.ExchangeSymbolName,
            IsActive = entity.IsActive,
            LastDataUpdate = entity.LastDataUpdate,
            ApiEndpoint = entity.ApiEndpoint,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts ExchangeSymbolModel to ExchangeSymbolEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>ExchangeSymbolEntity for database operations</returns>
    public static ExchangeSymbolEntity ToEntity(this ExchangeSymbolModel model)
    {
        return new ExchangeSymbolEntity
        {
            Id = model.Id,
            SymbolId = model.SymbolId,
            Exchange = (int)model.Exchange,
            ExchangeSymbolName = model.ExchangeSymbolName,
            IsActive = model.IsActive,
            LastDataUpdate = model.LastDataUpdate,
            ApiEndpoint = model.ApiEndpoint,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of ExchangeSymbolEntities to ExchangeSymbolModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<ExchangeSymbolModel> ToBusinessModels(this IEnumerable<ExchangeSymbolEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}