namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the status of a portfolio in the QuantFlow system
/// </summary>
public enum Status
{
    /// <summary>
    /// Represents an unknown or unspecified value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Portfolio is active and can be used for trading
    /// </summary>
    Active = 1,

    /// <summary>
    /// Portfolio is inactive and not currently trading
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Portfolio is temporarily paused
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Portfolio is archived and no longer in use
    /// </summary>
    Archived = 4
}