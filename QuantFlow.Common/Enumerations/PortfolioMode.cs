namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the mode of a portfolio (Test or Exchange Connected)
/// </summary>
public enum PortfolioMode
{
    /// <summary>
    /// Represents an unknown or unspecified value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Portfolio operates in test mode with simulated trading
    /// </summary>
    TestMode = 1,

    /// <summary>
    /// Portfolio is connected to a real exchange for live trading
    /// </summary>
    ExchangeConnected = 2
}