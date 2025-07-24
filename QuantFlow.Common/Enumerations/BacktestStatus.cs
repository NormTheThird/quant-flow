namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the status of a backtest run in the QuantFlow system
/// </summary>
public enum BacktestStatus
{
    /// <summary>
    /// Backtest is pending execution
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Backtest is currently running
    /// </summary>
    Running = 2,

    /// <summary>
    /// Backtest completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Backtest failed with an error
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Backtest was cancelled by the user
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Backtest timed out during execution
    /// </summary>
    TimedOut = 6
}