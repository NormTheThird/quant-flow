namespace QuantFlow.Domain.Services.ApiServices;

/// <summary>
/// Interface for Kraken API service operations
/// </summary>
public interface IKrakenApiService
{
    /// <summary>
    /// Sets the API credentials to use for subsequent API calls
    /// </summary>
    /// <param name="apiKey">Kraken API key</param>
    /// <param name="apiSecret">Kraken API secret</param>
    void SetCredentials(string apiKey, string apiSecret);

    /// <summary>
    /// Clears the currently set API credentials
    /// </summary>
    void ClearCredentials();

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

    /// <summary>
    /// Validates API credentials by attempting to query account balance
    /// </summary>
    /// <param name="apiKey">Kraken API key</param>
    /// <param name="apiSecret">Kraken API secret</param>
    /// <returns>True if credentials are valid, false otherwise</returns>
    Task<bool> ValidateCredentialsAsync(string apiKey, string apiSecret);

    /// <summary>
    /// Gets the balance for a specific currency from Kraken account
    /// </summary>
    /// <param name="apiKey">Kraken API key</param>
    /// <param name="apiSecret">Kraken API secret</param>
    /// <param name="currency">Currency symbol (e.g., "USDT", "USDC")</param>
    /// <returns>Balance amount for the specified currency</returns>
    Task<decimal> GetCurrencyBalanceAsync(string apiKey, string apiSecret, string currency);

    /// <summary>
    /// Gets the account balance for all assets
    /// </summary>
    /// <returns>Dictionary of asset names and their balances</returns>
    Task<Dictionary<string, decimal>> GetAccountBalanceAsync();

    /// <summary>
    /// Gets ticker information for a specific trading pair
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSD", "ETHUSD")</param>
    /// <returns>Ticker data with last price</returns>
    Task<TickerData?> GetTickerAsync(string symbol);
}