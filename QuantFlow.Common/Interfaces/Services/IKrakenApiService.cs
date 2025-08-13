namespace QuantFlow.Domain.Services.ApiServices;

/// <summary>
/// Interface for Kraken API service operations
/// </summary>
public interface IKrakenApiService
{
    /// <summary>
    /// Gets 1-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> Get1MinuteKlinesAsync(string symbol, int intervalsBack);

    /// <summary>
    /// Gets 5-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> Get5MinuteKlinesAsync(string symbol, int intervalsBack);

    /// <summary>
    /// Gets 15-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> Get15MinuteKlinesAsync(string symbol, int intervalsBack);

    /// <summary>
    /// Gets hourly kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> GetHourlyKlinesAsync(string symbol, int intervalsBack);

    /// <summary>
    /// Gets daily kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> GetDailyKlinesAsync(string symbol, int intervalsBack);

    /// <summary>
    /// Gets kline data with custom parameters
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="timeframe">Timeframe for the klines</param>
    /// <param name="startTime">Start time for data retrieval</param>
    /// <param name="endTime">Optional end time for data retrieval</param>
    /// <returns>List of kline data</returns>
    Task<List<KlineData>> GetKlinesAsync(string symbol, Timeframe timeframe, DateTime startTime, DateTime? endTime = null);
}