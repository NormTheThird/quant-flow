namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between AlgorithmPositionEntity and AlgorithmPositionModel
/// </summary>
public static class AlgorithmPositionMappingExtensions
{
    /// <summary>
    /// Converts AlgorithmPositionEntity to AlgorithmPositionModel
    /// </summary>
    public static AlgorithmPositionModel ToBusinessModel(this AlgorithmPositionEntity entity)
    {
        return new AlgorithmPositionModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PortfolioId = entity.PortfolioId,
            AlgorithmId = entity.AlgorithmId,
            PositionName = entity.PositionName,
            Symbol = entity.Symbol,
            AllocatedPercent = entity.AllocatedPercent,
            Status = Enum.Parse<Status>(entity.Status),
            MaxPositionSizePercent = entity.MaxPositionSizePercent,
            ExchangeFees = entity.ExchangeFees,
            AllowShortSelling = entity.AllowShortSelling,
            CurrentValue = entity.CurrentValue,
            ActivatedAt = entity.ActivatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts AlgorithmPositionModel to AlgorithmPositionEntity
    /// </summary>
    public static AlgorithmPositionEntity ToEntity(this AlgorithmPositionModel model)
    {
        return new AlgorithmPositionEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            PortfolioId = model.PortfolioId,
            AlgorithmId = model.AlgorithmId,
            PositionName = model.PositionName,
            Symbol = model.Symbol,
            AllocatedPercent = model.AllocatedPercent,
            Status = model.Status.ToString(),
            MaxPositionSizePercent = model.MaxPositionSizePercent,
            ExchangeFees = model.ExchangeFees,
            AllowShortSelling = model.AllowShortSelling,
            CurrentValue = model.CurrentValue,
            ActivatedAt = model.ActivatedAt,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts collection of AlgorithmPositionEntity to collection of AlgorithmPositionModel
    /// </summary>
    public static IEnumerable<AlgorithmPositionModel> ToBusinessModels(this IEnumerable<AlgorithmPositionEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }

    /// <summary>
    /// Converts collection of AlgorithmPositionModel to collection of AlgorithmPositionEntity
    /// </summary>
    public static IEnumerable<AlgorithmPositionEntity> ToEntities(this IEnumerable<AlgorithmPositionModel> models)
    {
        return models.Select(m => m.ToEntity());
    }
}