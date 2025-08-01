﻿namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Reason why a trade was executed in the backtesting system
/// </summary>
public enum TradeExecutionReason
{
    /// <summary>
    /// Trade was generated by the Python algorithm logic
    /// </summary>
    Algorithm = 1,

    /// <summary>
    /// Trade was automatically executed due to stop loss trigger
    /// </summary>
    StopLoss = 2,

    /// <summary>
    /// Trade was executed to take profits at target price
    /// </summary>
    TakeProfit = 3,

    /// <summary>
    /// Trade was executed due to time-based exit rules
    /// </summary>
    Timeout = 4,

    /// <summary>
    /// Trade was executed to comply with risk management limits
    /// </summary>
    RiskLimit = 5
}