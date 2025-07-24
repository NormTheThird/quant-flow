namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for backtest run operations using business models
/// </summary>
public interface IBacktestRunRepository
{
    /// <summary>
    /// Gets a backtest run by its unique identifier
    /// </summary>
    /// <param name="id">The backtest run's unique identifier</param>
    /// <returns>BacktestRun business model if found, null otherwise</returns>
    Task<BacktestRunModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all backtest runs for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of backtest run business models</returns>
    Task<IEnumerable<BacktestRunModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all backtest runs for a specific portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio's unique identifier</param>
    /// <returns>Collection of backtest run business models</returns>
    Task<IEnumerable<BacktestRunModel>> GetByPortfolioIdAsync(Guid portfolioId);

    /// <summary>
    /// Gets all backtest runs
    /// </summary>
    /// <returns>Collection of backtest run business models</returns>
    Task<IEnumerable<BacktestRunModel>> GetAllAsync();

    /// <summary>
    /// Creates a new backtest run
    /// </summary>
    /// <param name="backtestRun">BacktestRun business model to create</param>
    /// <returns>Created backtest run business model</returns>
    Task<BacktestRunModel> CreateAsync(BacktestRunModel backtestRun);

    /// <summary>
    /// Updates an existing backtest run
    /// </summary>
    /// <param name="backtestRun">BacktestRun business model with updates</param>
    /// <returns>Updated backtest run business model</returns>
    Task<BacktestRunModel> UpdateAsync(BacktestRunModel backtestRun);

    /// <summary>
    /// Soft deletes a backtest run
    /// </summary>
    /// <param name="id">The backtest run's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Counts the number of backtest runs for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Number of backtest runs</returns>
    Task<int> CountByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets backtest runs by status
    /// </summary>
    /// <param name="status">The backtest status to filter by</param>
    /// <returns>Collection of backtest run business models</returns>
    Task<IEnumerable<BacktestRunModel>> GetByStatusAsync(BacktestStatus status);
}