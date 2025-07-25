namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Enumeration representing algorithm status in the trading system
/// </summary>
public enum AlgorithmStatus
{
    /// <summary>
    /// Algorithm is in draft state and not yet ready for testing
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Algorithm is being tested with simulated or paper trading
    /// </summary>
    Testing = 2,

    /// <summary>
    /// Algorithm is actively running and executing trades
    /// </summary>
    Active = 3,

    /// <summary>
    /// Algorithm is temporarily paused but can be resumed
    /// </summary>
    Paused = 4,

    /// <summary>
    /// Algorithm has been manually stopped and is not executing
    /// </summary>
    Stopped = 5,

    /// <summary>
    /// Algorithm has encountered an error and requires attention
    /// </summary>
    Error = 6,

    /// <summary>
    /// Algorithm has been archived and is no longer in use
    /// </summary>
    Archived = 7
}