namespace QuantFlow.Common.Models;

/// <summary>
/// Base class for all algorithm parameters
/// </summary>
public abstract class BaseParameters
{
    /// <summary>
    /// Stop loss percentage (e.g., 5.0 = 5% below entry)
    /// </summary>
    public decimal StopLossPercent { get; set; } = 5.0m;

    /// <summary>
    /// Take profit percentage (e.g., 10.0 = 10% above entry)
    /// </summary>
    public decimal TakeProfitPercent { get; set; } = 10.0m;

    /// <summary>
    /// Percentage of allocated capital to use per trade (0-100)
    /// </summary>
    public decimal PositionSizePercent { get; set; } = 100m;

    /// <summary>
    /// Use ATR (Average True Range) for dynamic stop loss calculation
    /// </summary>
    public bool UseATRForStops { get; set; } = false;

    /// <summary>
    /// ATR multiplier for stop loss calculation (e.g., 2.0 = 2x ATR)
    /// </summary>
    public decimal ATRMultiplier { get; set; } = 2.0m;

    /// <summary>
    /// Converts parameters to dictionary for storage/serialization
    /// </summary>
    public abstract Dictionary<string, object> ToDictionary();

    /// <summary>
    /// Populates parameters from dictionary
    /// </summary>
    public abstract void FromDictionary(Dictionary<string, object> dict);
}