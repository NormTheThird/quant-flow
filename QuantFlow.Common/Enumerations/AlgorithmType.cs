namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Represents the type/category of trading algorithm
/// </summary>
public enum AlgorithmType
{
    /// <summary>
    /// Unknown or unspecified algorithm type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Trend following strategies (moving averages, MACD, etc.)
    /// </summary>
    TrendFollowing = 1,

    /// <summary>
    /// Mean reversion strategies (RSI, Bollinger Bands, etc.)
    /// </summary>
    MeanReversion = 2,

    /// <summary>
    /// Breakout strategies (range breakouts, flat top, etc.)
    /// </summary>
    Breakout = 3,

    /// <summary>
    /// Momentum-based strategies
    /// </summary>
    Momentum = 4,

    /// <summary>
    /// Volatility-based strategies
    /// </summary>
    Volatility = 5,

    /// <summary>
    /// Strategies using multiple signals/indicators
    /// </summary>
    MultiSignal = 6
}