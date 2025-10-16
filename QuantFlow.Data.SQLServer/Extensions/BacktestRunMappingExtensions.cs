namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between BacktestRunModel and BacktestRunEntity
/// </summary>
public static class BacktestRunMappingExtensions
{
    /// <summary>
    /// Converts BacktestRunEntity to BacktestRunModel
    /// </summary>
    public static BacktestRunModel ToBusinessModel(this BacktestRunEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new BacktestRunModel
        {
            Id = entity.Id,
            Name = entity.Name,
            AlgorithmId = entity.AlgorithmId,
            UserId = entity.UserId,
            Symbol = entity.Symbol,
            Exchange = Enum.Parse<Exchange>(entity.Exchange),
            Timeframe = (Timeframe)entity.Timeframe,
            BacktestStartDate = entity.BacktestStartDate,
            BacktestEndDate = entity.BacktestEndDate,
            Status = Enum.Parse<BacktestStatus>(entity.Status),
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
            ExecutionDuration = TimeSpan.FromTicks(entity.ExecutionDurationTicks),
            ErrorMessage = entity.ErrorMessage,
            CompletedAt = entity.CompletedAt,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts BacktestRunModel to BacktestRunEntity
    /// </summary>
    public static BacktestRunEntity ToEntity(this BacktestRunModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new BacktestRunEntity
        {
            Id = model.Id,
            Name = model.Name,
            AlgorithmId = model.AlgorithmId,
            UserId = model.UserId,
            Symbol = model.Symbol,
            Exchange = model.Exchange.ToString(),
            Timeframe = (int)model.Timeframe,
            BacktestStartDate = model.BacktestStartDate,
            BacktestEndDate = model.BacktestEndDate,
            Status = model.Status.ToString(),
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
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts collection of entities to business models
    /// </summary>
    public static IEnumerable<BacktestRunModel> ToBusinessModels(this IEnumerable<BacktestRunEntity> entities)
    {
        return entities?.Select(_ => _.ToBusinessModel()) ?? Enumerable.Empty<BacktestRunModel>();
    }
}