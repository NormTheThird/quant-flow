namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for trade operations using business models
/// </summary>
public interface ITradeRepository
{
    /// <summary>
    /// Gets a trade by its unique identifier
    /// </summary>
    /// <param name="id">The trade's unique identifier</param>
    /// <returns>Trade business model if found, null otherwise</returns>
    Task<TradeModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all trades for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <returns>Collection of trade business models</returns>
    Task<IEnumerable<TradeModel>> GetByBacktestRunIdAsync(Guid backtestRunId);

    /// <summary>
    /// Gets all trades
    /// </summary>
    /// <returns>Collection of trade business models</returns>
    Task<IEnumerable<TradeModel>> GetAllAsync();

    /// <summary>
    /// Creates a new trade
    /// </summary>
    /// <param name="trade">Trade business model to create</param>
    /// <returns>Created trade business model</returns>
    Task<TradeModel> CreateAsync(TradeModel trade);

    /// <summary>
    /// Creates multiple trades in a batch operation
    /// </summary>
    /// <param name="trades">Collection of trade business models to create</param>
    /// <returns>Collection of created trade business models</returns>
    Task<IEnumerable<TradeModel>> CreateBatchAsync(IEnumerable<TradeModel> trades);

    /// <summary>
    /// Updates an existing trade
    /// </summary>
    /// <param name="trade">Trade business model with updates</param>
    /// <returns>Updated trade business model</returns>
    Task<TradeModel> UpdateAsync(TradeModel trade);

    /// <summary>
    /// Soft deletes a trade
    /// </summary>
    /// <param name="id">The trade's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets trades by symbol for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <param name="symbol">The trading symbol</param>
    /// <returns>Collection of trade business models</returns>
    Task<IEnumerable<TradeModel>> GetBySymbolAsync(Guid backtestRunId, string symbol);

    /// <summary>
    /// Gets trades by type for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <param name="tradeType">The trade type</param>
    /// <returns>Collection of trade business models</returns>
    Task<IEnumerable<TradeModel>> GetByTypeAsync(Guid backtestRunId, TradeType tradeType);
}