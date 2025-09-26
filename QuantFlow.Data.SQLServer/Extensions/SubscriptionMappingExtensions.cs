//namespace QuantFlow.Data.SQLServer.Extensions;

///// <summary>
///// Extension methods for mapping between SubscriptionModel and SubscriptionEntity
///// </summary>
//public static class SubscriptionMappingExtensions
//{
//    /// <summary>
//    /// Converts SubscriptionEntity to SubscriptionModel
//    /// </summary>
//    /// <param name="entity">The entity to convert</param>
//    /// <returns>SubscriptionModel business object</returns>
//    public static SubscriptionModel ToBusinessModel(this SubscriptionEntity entity)
//    {
//        return new SubscriptionModel
//        {
//            Id = entity.Id,
//            UserId = entity.UserId,
//            Type = (SubscriptionType)entity.Type,
//            StartDate = entity.StartDate,
//            EndDate = entity.EndDate,
//            IsActive = entity.IsActive,
//            MaxPortfolios = entity.MaxPortfolios,
//            MaxAlgorithms = entity.MaxAlgorithms,
//            MaxBacktestRuns = entity.MaxBacktestRuns,
//            CreatedAt = entity.CreatedAt,
//            UpdatedAt = entity.UpdatedAt,
//            IsDeleted = entity.IsDeleted,
//            CreatedBy = entity.CreatedBy,
//            UpdatedBy = entity.UpdatedBy
//        };
//    }

//    /// <summary>
//    /// Converts SubscriptionModel to SubscriptionEntity
//    /// </summary>
//    /// <param name="model">The business model to convert</param>
//    /// <returns>SubscriptionEntity for database operations</returns>
//    public static SubscriptionEntity ToEntity(this SubscriptionModel model)
//    {
//        return new SubscriptionEntity
//        {
//            Id = model.Id,
//            UserId = model.UserId,
//            Type = (int)model.Type,
//            StartDate = model.StartDate,
//            EndDate = model.EndDate,
//            IsActive = model.IsActive,
//            MaxPortfolios = model.MaxPortfolios,
//            MaxAlgorithms = model.MaxAlgorithms,
//            MaxBacktestRuns = model.MaxBacktestRuns,
//            CreatedAt = model.CreatedAt,
//            UpdatedAt = model.UpdatedAt,
//            IsDeleted = model.IsDeleted,
//            CreatedBy = model.CreatedBy,
//            UpdatedBy = model.UpdatedBy
//        };
//    }

//    /// <summary>
//    /// Converts a collection of SubscriptionEntities to SubscriptionModels
//    /// </summary>
//    /// <param name="entities">Collection of entities</param>
//    /// <returns>Collection of business models</returns>
//    public static IEnumerable<SubscriptionModel> ToBusinessModels(this IEnumerable<SubscriptionEntity> entities)
//    {
//        return entities.Select(e => e.ToBusinessModel());
//    }
//}