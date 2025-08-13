namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Defines the timeframes for trading data in the QuantFlow system
/// </summary>
public enum Timeframe
{
    /// <summary>
    /// Unknown or unspecified timeframe
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 1 minute timeframe
    /// </summary>
    OneMinute = 1,

    /// <summary>
    /// 5 minute timeframe
    /// </summary>
    FiveMinutes = 5,

    /// <summary>
    /// 15 minute timeframe
    /// </summary>
    FifteenMinutes = 15,

    /// <summary>
    /// 30 minute timeframe
    /// </summary>
    ThirtyMinutes = 30,

    /// <summary>
    /// 1 hour timeframe
    /// </summary>
    OneHour = 60,

    /// <summary>
    /// 4 hour timeframe
    /// </summary>
    FourHours = 240,

    /// <summary>
    /// 1 day timeframe
    /// </summary>
    OneDay = 1440,

    /// <summary>
    /// 1 week timeframe
    /// </summary>
    OneWeek = 10080,

    /// <summary>
    /// 1 month timeframe
    /// </summary>
    OneMonth = 43200
}