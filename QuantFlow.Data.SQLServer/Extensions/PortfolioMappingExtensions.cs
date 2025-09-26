//namespace QuantFlow.Data.SQLServer.Extensions;

///// <summary>
///// Extension methods for mapping between PortfolioModel and PortfolioEntity
///// </summary>
//public static class PortfolioMappingExtensions
//{
//    /// <summary>
//    /// Converts PortfolioEntity to PortfolioModel
//    /// </summary>
//    /// <param name="entity">The entity to convert</param>
//    /// <returns>PortfolioModel business object</returns>
//    public static PortfolioModel ToBusinessModel(this PortfolioEntity entity)
//    {
//        return new PortfolioModel
//        {
//            Id = entity.Id,
//            Name = entity.Name,
//            Description = entity.Description,
//            InitialBalance = entity.InitialBalance,
//            CurrentBalance = entity.CurrentBalance,
//            Status = (PortfolioStatus)entity.Status,
//            UserId = entity.UserId,
//            MaxPositionSizePercent = entity.MaxPositionSizePercent,
//            CommissionRate = entity.CommissionRate,
//            AllowShortSelling = entity.AllowShortSelling,
//            CreatedAt = entity.CreatedAt,
//            UpdatedAt = entity.UpdatedAt,
//            IsDeleted = entity.IsDeleted,
//            CreatedBy = entity.CreatedBy,
//            UpdatedBy = entity.UpdatedBy
//        };
//    }

//    /// <summary>
//    /// Converts PortfolioModel to PortfolioEntity
//    /// </summary>
//    /// <param name="model">The business model to convert</param>
//    /// <returns>PortfolioEntity for database operations</returns>
//    public static PortfolioEntity ToEntity(this PortfolioModel model)
//    {
//        return new PortfolioEntity
//        {
//            Id = model.Id,
//            Name = model.Name,
//            Description = model.Description,
//            InitialBalance = model.InitialBalance,
//            CurrentBalance = model.CurrentBalance,
//            Status = (int)model.Status,
//            UserId = model.UserId,
//            MaxPositionSizePercent = model.MaxPositionSizePercent,
//            CommissionRate = model.CommissionRate,
//            AllowShortSelling = model.AllowShortSelling,
//            CreatedAt = model.CreatedAt,
//            UpdatedAt = model.UpdatedAt,
//            IsDeleted = model.IsDeleted,
//            CreatedBy = model.CreatedBy,
//            UpdatedBy = model.UpdatedBy
//        };
//    }

//    /// <summary>
//    /// Converts a collection of PortfolioEntities to PortfolioModels
//    /// </summary>
//    /// <param name="entities">Collection of entities</param>
//    /// <returns>Collection of business models</returns>
//    public static IEnumerable<PortfolioModel> ToBusinessModels(this IEnumerable<PortfolioEntity> entities)
//    {
//        return entities.Select(e => e.ToBusinessModel());
//    }
//}