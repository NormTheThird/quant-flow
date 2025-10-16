namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for executing backtests
/// </summary>
public interface IBacktestExecutionService
{
    /// <summary>
    /// Executes a backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <returns>Completed backtest run with results</returns>
    Task<BacktestRunModel> ExecuteBacktestAsync(Guid backtestRunId);
}