namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between PortfolioModel and PortfolioEntity
/// </summary>
public static class PortfolioMappingExtensions
{
    /// <summary>
    /// Converts PortfolioEntity to PortfolioModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>PortfolioModel business object</returns>
    public static PortfolioModel ToBusinessModel(this PortfolioEntity entity)
    {
        return new PortfolioModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            InitialBalance = entity.InitialBalance,
            CurrentBalance = entity.CurrentBalance,
            Status = Enum.Parse<PortfolioStatus>(entity.Status),
            Mode = Enum.Parse<PortfolioMode>(entity.Mode),
            Exchange = string.IsNullOrEmpty(entity.Exchange) ? null : Enum.Parse<Exchange>(entity.Exchange),
            UserExchangeDetailsId = entity.UserExchangeDetailsId,
            UserId = entity.UserId,
            MaxPositionSizePercent = entity.MaxPositionSizePercent,
            CommissionRate = entity.CommissionRate,
            AllowShortSelling = entity.AllowShortSelling,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts PortfolioModel to PortfolioEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>PortfolioEntity for database operations</returns>
    public static PortfolioEntity ToEntity(this PortfolioModel model)
    {
        return new PortfolioEntity
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            InitialBalance = model.InitialBalance,
            CurrentBalance = model.CurrentBalance,
            Status = model.Status.ToString(),
            Mode = model.Mode.ToString(),
            Exchange = model.Exchange?.ToString(),
            UserExchangeDetailsId = model.UserExchangeDetailsId,
            UserId = model.UserId,
            MaxPositionSizePercent = model.MaxPositionSizePercent,
            CommissionRate = model.CommissionRate,
            AllowShortSelling = model.AllowShortSelling,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of PortfolioEntities to PortfolioModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<PortfolioModel> ToBusinessModels(this IEnumerable<PortfolioEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}