namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service for trade operations
/// </summary>
public interface ITradeService
{
    /// <summary>
    /// Gets a trade by ID
    /// </summary>
    Task<TradeModel?> GetTradeByIdAsync(Guid id);

    /// <summary>
    /// Gets all trades for a backtest run
    /// </summary>
    Task<IEnumerable<TradeModel>> GetTradesByBacktestRunIdAsync(Guid backtestRunId);

    /// <summary>
    /// Creates a new trade
    /// </summary>
    Task<TradeModel> CreateTradeAsync(TradeModel trade);

    /// <summary>
    /// Creates multiple trades
    /// </summary>
    Task<IEnumerable<TradeModel>> CreateTradesBatchAsync(IEnumerable<TradeModel> trades);

    /// <summary>
    /// Updates an existing trade
    /// </summary>
    Task<TradeModel> UpdateTradeAsync(TradeModel trade);

    /// <summary>
    /// Deletes a trade
    /// </summary>
    Task<bool> DeleteTradeAsync(Guid id);
}