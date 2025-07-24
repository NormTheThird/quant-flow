namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between BacktestRunModel and BacktestRunEntity
/// </summary>
public static class BacktestRunMappingExtensions
{
    /// <summary>
    /// Converts BacktestRunEntity to BacktestRunModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>BacktestRunModel business object</returns>
    public static BacktestRunModel ToBusinessModel(this BacktestRunEntity entity)
    {
        return new BacktestRunModel
        {
            Id = entity.Id,
            Name = entity.Name,
            AlgorithmId = entity.AlgorithmId,
            PortfolioId = entity.PortfolioId,
            UserId = entity.UserId,
            Symbol = entity.Symbol,
            Exchange = (Exchange)entity.Exchange,
            Timeframe = (Timeframe)entity.Timeframe,
            BacktestStartDate = entity.BacktestStartDate,
            BacktestEndDate = entity.BacktestEndDate,
            Status = (BacktestStatus)entity.Status,
            InitialBalance = entity.InitialBalance,
            AlgorithmParameters = entity.AlgorithmParameters,
            CommissionRate = entity.CommissionRate,
            FinalBalance = entity.FinalBalance,
            TotalReturnPercent = entity.TotalReturnPercent,
            MaxDrawdownPercent = entity.MaxDrawdownPercent,
            SharpeRatio = entity.SharpeRatio,
            TotalTrades = entity.TotalTrades,
            WinningTrades = entity.WinningTrades,
            LosingTrades = entity.LosingTrades,
            WinRatePercent = entity.WinRatePercent,
            AverageTradeReturnPercent = entity.AverageTradeReturnPercent,
            ExecutionDuration = new TimeSpan(entity.ExecutionDurationTicks),
            ErrorMessage = entity.ErrorMessage,
            CompletedAt = entity.CompletedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts BacktestRunModel to BacktestRunEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>BacktestRunEntity for database operations</returns>
    public static BacktestRunEntity ToEntity(this BacktestRunModel model)
    {
        return new BacktestRunEntity
        {
            Id = model.Id,
            Name = model.Name,
            AlgorithmId = model.AlgorithmId,
            PortfolioId = model.PortfolioId,
            UserId = model.UserId,
            Symbol = model.Symbol,
            Exchange = (int)model.Exchange,
            Timeframe = (int)model.Timeframe,
            BacktestStartDate = model.BacktestStartDate,
            BacktestEndDate = model.BacktestEndDate,
            Status = (int)model.Status,
            InitialBalance = model.InitialBalance,
            AlgorithmParameters = model.AlgorithmParameters,
            CommissionRate = model.CommissionRate,
            FinalBalance = model.FinalBalance,
            TotalReturnPercent = model.TotalReturnPercent,
            MaxDrawdownPercent = model.MaxDrawdownPercent,
            SharpeRatio = model.SharpeRatio,
            TotalTrades = model.TotalTrades,
            WinningTrades = model.WinningTrades,
            LosingTrades = model.LosingTrades,
            WinRatePercent = model.WinRatePercent,
            AverageTradeReturnPercent = model.AverageTradeReturnPercent,
            ExecutionDurationTicks = model.ExecutionDuration.Ticks,
            ErrorMessage = model.ErrorMessage,
            CompletedAt = model.CompletedAt,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of BacktestRunEntities to BacktestRunModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<BacktestRunModel> ToBusinessModels(this IEnumerable<BacktestRunEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}