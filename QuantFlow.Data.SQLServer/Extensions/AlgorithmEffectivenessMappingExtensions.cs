namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between AlgorithmEffectivenessEntity and AlgorithmEffectivenessModel
/// </summary>
public static class AlgorithmEffectivenessMappingExtensions
{
    /// <summary>
    /// Converts AlgorithmEffectivenessEntity to business model
    /// </summary>
    public static AlgorithmEffectivenessModel ToBusinessModel(this AlgorithmEffectivenessEntity entity)
    {
        return new AlgorithmEffectivenessModel
        {
            Id = entity.Id,
            AlgorithmId = entity.AlgorithmId,
            Timeframe = entity.Timeframe,
            ReliabilityStars = entity.ReliabilityStars,
            OpportunityStars = entity.OpportunityStars,
            RecommendedStars = entity.RecommendedStars,
            ReliabilityReason = entity.ReliabilityReason,
            OpportunityReason = entity.OpportunityReason,
            AverageWinRate = entity.AverageWinRate,
            AverageReturnPerTrade = entity.AverageReturnPerTrade,
            AverageTradesPerMonth = entity.AverageTradesPerMonth,
            AverageSharpeRatio = entity.AverageSharpeRatio,
            AverageMaxDrawdown = entity.AverageMaxDrawdown,
            AverageStopLossPercent = entity.AverageStopLossPercent,
            BestFor = entity.BestFor,
            AvoidWhen = entity.AvoidWhen,
            TotalBacktestsRun = entity.TotalBacktestsRun,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts business model to AlgorithmEffectivenessEntity
    /// </summary>
    public static AlgorithmEffectivenessEntity ToEntity(this AlgorithmEffectivenessModel model)
    {
        return new AlgorithmEffectivenessEntity
        {
            Id = model.Id,
            AlgorithmId = model.AlgorithmId,
            Timeframe = model.Timeframe,
            ReliabilityStars = model.ReliabilityStars,
            OpportunityStars = model.OpportunityStars,
            RecommendedStars = model.RecommendedStars,
            ReliabilityReason = model.ReliabilityReason,
            OpportunityReason = model.OpportunityReason,
            AverageWinRate = model.AverageWinRate,
            AverageReturnPerTrade = model.AverageReturnPerTrade,
            AverageTradesPerMonth = model.AverageTradesPerMonth,
            AverageSharpeRatio = model.AverageSharpeRatio,
            AverageMaxDrawdown = model.AverageMaxDrawdown,
            AverageStopLossPercent = model.AverageStopLossPercent,
            BestFor = model.BestFor,
            AvoidWhen = model.AvoidWhen,
            TotalBacktestsRun = model.TotalBacktestsRun,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts collection of entities to business models
    /// </summary>
    public static IEnumerable<AlgorithmEffectivenessModel> ToBusinessModels(this IEnumerable<AlgorithmEffectivenessEntity> entities)
    {
        return entities.Select(_ => _.ToBusinessModel());
    }
}