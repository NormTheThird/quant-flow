namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Types of stop loss strategies available for risk management
/// </summary>
public enum StopLossType
{
    /// <summary>
    /// Fixed percentage below entry price (most common)
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Dynamic stop that trails price movements to lock in profits
    /// </summary>
    Trailing = 2,

    /// <summary>
    /// Stop based on Average True Range for volatility adjustment
    /// </summary>
    ATRBased = 3,

    /// <summary>
    /// Time-based exit after specified duration
    /// </summary>
    TimeBased = 4
}