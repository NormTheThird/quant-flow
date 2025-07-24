namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the types of trades in the QuantFlow system
/// </summary>
public enum TradeType
{
    /// <summary>
    /// Buy order - purchasing an asset
    /// </summary>
    Buy = 1,

    /// <summary>
    /// Sell order - selling an asset
    /// </summary>
    Sell = 2,

    /// <summary>
    /// Short sell order - selling borrowed assets
    /// </summary>
    ShortSell = 3,

    /// <summary>
    /// Cover order - buying to close a short position
    /// </summary>
    Cover = 4
}