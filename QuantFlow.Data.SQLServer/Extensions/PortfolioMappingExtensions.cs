namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between PortfolioModel and PortfolioEntity
/// </summary>
public static class PortfolioMappingExtensions
{
    /// <summary>
    /// Converts PortfolioModel to PortfolioEntity
    /// </summary>
    public static PortfolioEntity ToEntity(this PortfolioModel model)
    {
        return new PortfolioEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            Description = model.Description,
            InitialBalance = model.InitialBalance,
            CurrentBalance = model.CurrentBalance,
            Status = model.Status.ToString(),
            Mode = model.Mode.ToString(),
            Exchange = model.Exchange.ToString(),
            BaseCurrency = model.BaseCurrency.ToString(),
            UserExchangeDetailsId = model.UserExchangeDetailsId,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts PortfolioEntity to PortfolioModel
    /// </summary>
    public static PortfolioModel ToBusinessModel(this PortfolioEntity entity)
    {
        return new PortfolioModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            Description = entity.Description,
            InitialBalance = entity.InitialBalance,
            CurrentBalance = entity.CurrentBalance,
            Status = Enum.Parse<Status>(entity.Status),
            Mode = Enum.Parse<PortfolioMode>(entity.Mode),
            Exchange = Enum.Parse<Exchange>(entity.Exchange),
            BaseCurrency = Enum.Parse<BaseCurrency>(entity.BaseCurrency),
            UserExchangeDetailsId = entity.UserExchangeDetailsId,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts collection of PortfolioEntity to collection of PortfolioModel
    /// </summary>
    public static IEnumerable<PortfolioModel> ToBusinessModels(this IEnumerable<PortfolioEntity> entities)
    {
        return entities.Select(entity => entity.ToBusinessModel());
    }
}