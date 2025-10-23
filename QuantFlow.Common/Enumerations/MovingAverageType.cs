namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Type of moving average calculation
/// </summary>
public enum MovingAverageType
{
    /// <summary>
    /// Unknown or unspecified type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Simple Moving Average
    /// </summary>
    SimpleMovingAverage = 1,

    /// <summary>
    /// Exponential Moving Average
    /// </summary>
    ExponentialMovingAverage = 2
}