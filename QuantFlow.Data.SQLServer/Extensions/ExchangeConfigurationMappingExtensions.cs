namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between ExchangeConfiguration entities and business models
/// </summary>
public static class ExchangeConfigurationMappingExtensions
{
    /// <summary>
    /// Converts ExchangeConfigurationEntity to ExchangeConfigurationModel
    /// </summary>
    public static ExchangeConfigurationModel ToBusinessModel(this ExchangeConfigurationEntity entity)
    {
        return new ExchangeConfigurationModel
        {
            Id = entity.Id,
            Exchange = (Exchange)entity.Exchange,
            Name = entity.Name,
            IsActive = entity.IsActive,
            IsSupported = entity.IsSupported,
            BaseMakerFeePercent = entity.BaseMakerFeePercent,
            BaseTakerFeePercent = entity.BaseTakerFeePercent,
            ApiEndpoint = entity.ApiEndpoint,
            MaxRequestsPerMinute = entity.MaxRequestsPerMinute,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            FeeTiers = entity.FeeTiers.Select(ft => ft.ToBusinessModel()).ToList(),
            SymbolOverrides = entity.SymbolOverrides.Select(so => so.ToBusinessModel()).ToList()
        };
    }

    /// <summary>
    /// Converts ExchangeConfigurationModel to ExchangeConfigurationEntity
    /// </summary>
    public static ExchangeConfigurationEntity ToEntity(this ExchangeConfigurationModel model)
    {
        return new ExchangeConfigurationEntity
        {
            Id = model.Id,
            Exchange = (int)model.Exchange,
            Name = model.Name,
            IsActive = model.IsActive,
            IsSupported = model.IsSupported,
            BaseMakerFeePercent = model.BaseMakerFeePercent,
            BaseTakerFeePercent = model.BaseTakerFeePercent,
            ApiEndpoint = model.ApiEndpoint,
            MaxRequestsPerMinute = model.MaxRequestsPerMinute,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy ?? string.Empty
        };
    }

    /// <summary>
    /// Converts collection of ExchangeConfigurationEntities to business models
    /// </summary>
    public static IEnumerable<ExchangeConfigurationModel> ToBusinessModels(this IEnumerable<ExchangeConfigurationEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}