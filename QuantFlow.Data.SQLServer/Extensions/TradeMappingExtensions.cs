namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between TradeModel and TradeEntity
/// </summary>
public static class TradeMappingExtensions
{
    /// <summary>
    /// Converts TradeEntity to TradeModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>TradeModel business object</returns>
    public static TradeModel ToBusinessModel(this TradeEntity entity)
    {
        return new TradeModel
        {
            Id = entity.Id,
            BacktestRunId = entity.BacktestRunId,
            Exchange = Exchange.Kraken,
            Symbol = entity.Symbol,
            Type = (TradeType)entity.Type,
            ExecutionTimestamp = entity.ExecutionTimestamp,
            Quantity = entity.Quantity,
            Price = entity.Price,
            Value = entity.Value,
            Commission = entity.Commission,
            NetValue = entity.NetValue,
            PortfolioBalanceBefore = entity.PortfolioBalanceBefore,
            PortfolioBalanceAfter = entity.PortfolioBalanceAfter,
            AlgorithmReason = entity.AlgorithmReason,
            AlgorithmConfidence = entity.AlgorithmConfidence,
            RealizedProfitLoss = entity.RealizedProfitLoss,
            RealizedProfitLossPercent = entity.RealizedProfitLossPercent,
            EntryTradeId = entity.EntryTradeId,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts TradeModel to TradeEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>TradeEntity for database operations</returns>
    public static TradeEntity ToEntity(this TradeModel model)
    {
        return new TradeEntity
        {
            Id = model.Id,
            BacktestRunId = model.BacktestRunId,
            Symbol = model.Symbol,
            Type = (int)model.Type,
            ExecutionTimestamp = model.ExecutionTimestamp,
            Quantity = model.Quantity,
            Price = model.Price,
            Value = model.Value,
            Commission = model.Commission,
            NetValue = model.NetValue,
            PortfolioBalanceBefore = model.PortfolioBalanceBefore,
            PortfolioBalanceAfter = model.PortfolioBalanceAfter,
            AlgorithmReason = model.AlgorithmReason,
            AlgorithmConfidence = model.AlgorithmConfidence,
            RealizedProfitLoss = model.RealizedProfitLoss,
            RealizedProfitLossPercent = model.RealizedProfitLossPercent,
            EntryTradeId = model.EntryTradeId,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy ?? "System",
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of TradeEntities to TradeModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<TradeModel> ToBusinessModels(this IEnumerable<TradeEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}