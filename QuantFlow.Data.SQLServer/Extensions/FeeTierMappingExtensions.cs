namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between FeeTier entities and business models
/// </summary>
public static class FeeTierMappingExtensions
{
    /// <summary>
    /// Converts FeeTierEntity to FeeTierModel
    /// </summary>
    public static FeeTierModel ToBusinessModel(this FeeTierEntity entity)
    {
        return new FeeTierModel
        {
            Id = entity.Id,
            ExchangeConfigurationId = entity.ExchangeConfigurationId,
            Exchange = (Exchange)entity.Exchange,
            TierLevel = entity.TierLevel,
            MinimumVolumeThreshold = entity.MinimumVolumeThreshold,
            MakerFeePercent = entity.MakerFeePercent,
            TakerFeePercent = entity.TakerFeePercent,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts FeeTierModel to FeeTierEntity
    /// </summary>
    public static FeeTierEntity ToEntity(this FeeTierModel model)
    {
        return new FeeTierEntity
        {
            Id = model.Id,
            ExchangeConfigurationId = model.ExchangeConfigurationId,
            Exchange = (int)model.Exchange,
            TierLevel = model.TierLevel,
            MinimumVolumeThreshold = model.MinimumVolumeThreshold,
            MakerFeePercent = model.MakerFeePercent,
            TakerFeePercent = model.TakerFeePercent,
            IsActive = model.IsActive,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy ?? string.Empty
        };
    }

    /// <summary>
    /// Converts collection of FeeTierEntities to business models
    /// </summary>
    public static IEnumerable<FeeTierModel> ToBusinessModels(this IEnumerable<FeeTierEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}