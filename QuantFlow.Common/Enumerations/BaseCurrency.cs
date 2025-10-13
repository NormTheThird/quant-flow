namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the base currencies supported for portfolio trading
/// </summary>
public enum BaseCurrency
{
    /// <summary>
    /// Unknown or unspecified base currency
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Tether USD stablecoin
    /// </summary>
    USDT = 1,

    /// <summary>
    /// USD Coin stablecoin
    /// </summary>
    USDC = 2
}