namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the side of a trade (buy/sell)
/// </summary>
public enum TradeSignal
{
    /// <summary>
    /// Hold current position, no action
    /// </summary>
    Hold = 0,

    /// <summary>
    /// Buy side - purchasing an asset
    /// </summary>
    Buy = 1,

    /// <summary>
    /// Sell side - selling an asset
    /// </summary>
    Sell = 2
}