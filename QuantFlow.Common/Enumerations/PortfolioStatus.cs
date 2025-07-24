namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the status of a portfolio in the QuantFlow system
/// </summary>
public enum PortfolioStatus
{
    /// <summary>
    /// Portfolio is active and can be used for trading
    /// </summary>
    Active = 1,

    /// <summary>
    /// Portfolio is temporarily paused
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Portfolio is archived and no longer in use
    /// </summary>
    Archived = 3,

    /// <summary>
    /// Portfolio is suspended due to issues
    /// </summary>
    Suspended = 4
}