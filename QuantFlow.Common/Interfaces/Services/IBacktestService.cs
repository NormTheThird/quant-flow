namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for backtest operations
/// </summary>
public interface IBacktestService
{
    /// <summary>
    /// Gets a backtest run by its unique identifier
    /// </summary>
    /// <param name="id">The backtest run's unique identifier</param>
    /// <returns>Backtest run if found, null otherwise</returns>
    Task<BacktestRunModel?> GetBacktestRunByIdAsync(Guid id);

    /// <summary>
    /// Gets all backtest runs for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of backtest runs</returns>
    Task<IEnumerable<BacktestRunModel>> GetBacktestRunsByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new backtest run
    /// </summary>
    /// <param name="backtestRun">Backtest run to create</param>
    /// <returns>Created backtest run</returns>
    Task<BacktestRunModel> CreateBacktestRunAsync(BacktestRunModel backtestRun);

    /// <summary>
    /// Updates an existing backtest run
    /// </summary>
    /// <param name="backtestRun">Backtest run with updates</param>
    /// <returns>Updated backtest run</returns>
    Task<BacktestRunModel> UpdateBacktestRunAsync(BacktestRunModel backtestRun);

    /// <summary>
    /// Deletes a backtest run
    /// </summary>
    /// <param name="id">The backtest run's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteBacktestRunAsync(Guid id);

    /// <summary>
    /// Gets backtest runs by status
    /// </summary>
    /// <param name="status">The backtest status to filter by</param>
    /// <returns>Collection of backtest runs</returns>
    Task<IEnumerable<BacktestRunModel>> GetBacktestRunsByStatusAsync(BacktestStatus status);
}