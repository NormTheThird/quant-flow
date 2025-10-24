using System.ComponentModel;

namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Enumeration of hard-coded algorithm names that map to database records.
/// When adding a new hard-coded algorithm, add a new enum value here
/// and create a corresponding SQL seed script with the same name.
/// </summary>
public enum HardCodedAlgorithmName
{
    /// <summary>
    /// Unknown or unspecified algorithm
    /// </summary>
    [Description("Unknown")]
    Unknown = 0,

    /// <summary>
    /// Moving Average Crossover strategy
    /// </summary>
    [Description("Moving Average Crossover")]
    MovingAverageCrossover = 1,

    /// <summary>
    /// RSI Mean Reversion strategy
    /// </summary>
    [Description("RSI Mean Reversion")]
    RsiMeanReversion = 2,

    /// <summary>
    /// Bollinger Bands Breakout strategy
    /// </summary>
    [Description("Bollinger Bands Breakout")]
    BollingerBandsBreakout = 3,

    /// <summary>
    /// MACD Crossover strategy
    /// </summary>
    [Description("MACD Crossover")]
    MacdCrossover = 4,

    /// <summary>
    /// Volume Weighted Average Price strategy
    /// </summary>
    [Description("Volume Weighted Average Price")]
    VolumeWeightedAveragePrice = 5
}