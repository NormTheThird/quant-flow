//namespace QuantFlow.Data.SQLServer.Extensions;

///// <summary>
///// Extension methods for mapping between SymbolFeeOverride entities and business models
///// </summary>
//public static class SymbolFeeOverrideMappingExtensions
//{
//    /// <summary>
//    /// Converts SymbolFeeOverrideEntity to SymbolFeeOverrideModel
//    /// </summary>
//    public static SymbolFeeOverrideModel ToBusinessModel(this SymbolFeeOverrideEntity entity)
//    {
//        return new SymbolFeeOverrideModel
//        {
//            Id = entity.Id,
//            ExchangeConfigurationId = entity.ExchangeConfigurationId,
//            Exchange = (Exchange)entity.Exchange,
//            Symbol = entity.Symbol,
//            MakerFeePercent = entity.MakerFeePercent,
//            TakerFeePercent = entity.TakerFeePercent,
//            IsActive = entity.IsActive,
//            Reason = entity.Reason,
//            CreatedAt = entity.CreatedAt,
//            UpdatedAt = entity.UpdatedAt,
//            IsDeleted = entity.IsDeleted,
//            CreatedBy = entity.CreatedBy,
//            UpdatedBy = entity.UpdatedBy
//        };
//    }

//    /// <summary>
//    /// Converts SymbolFeeOverrideModel to SymbolFeeOverrideEntity
//    /// </summary>
//    public static SymbolFeeOverrideEntity ToEntity(this SymbolFeeOverrideModel model)
//    {
//        return new SymbolFeeOverrideEntity
//        {
//            Id = model.Id,
//            ExchangeConfigurationId = model.ExchangeConfigurationId,
//            Exchange = (int)model.Exchange,
//            Symbol = model.Symbol,
//            MakerFeePercent = model.MakerFeePercent,
//            TakerFeePercent = model.TakerFeePercent,
//            IsActive = model.IsActive,
//            Reason = model.Reason,
//            CreatedAt = model.CreatedAt,
//            UpdatedAt = model.UpdatedAt,
//            IsDeleted = model.IsDeleted,
//            CreatedBy = model.CreatedBy,
//            UpdatedBy = model.UpdatedBy ?? string.Empty
//        };
//    }

//    /// <summary>
//    /// Converts collection of SymbolFeeOverrideEntities to business models
//    /// </summary>
//    public static IEnumerable<SymbolFeeOverrideModel> ToBusinessModels(this IEnumerable<SymbolFeeOverrideEntity> entities)
//    {
//        return entities.Select(e => e.ToBusinessModel());
//    }
//}