using Kraken.Net.Clients;
using Kraken.Net.Enums;

namespace QuantFlow.Domain.Services.ApiServices;

/// <summary>
/// Kraken API service for retrieving market data with proper dependency injection
/// Github: https://github.com/JKorf/Kraken.Net
/// </summary>
public class KrakenApiService : IKrakenApiService
{
    private readonly ILogger<KrakenApiService> _logger;
    private readonly KrakenRestClient _krakenClient;
    private ApiCredentials? _currentCredentials;

    /// <summary>
    /// Initializes a new instance of KrakenApiService with proper dependency injection
    /// </summary>
    /// <param name="configuration">Application configuration for request settings</param>
    /// <param name="logger">Logger for error handling and debugging</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
    public KrakenApiService(IConfiguration configuration, ILogger<KrakenApiService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var requestTimeoutSeconds = configuration.GetValue<int>("Kraken:RequestTimeoutSeconds", 60);

        _krakenClient = new KrakenRestClient(options => { options.RequestTimeout = TimeSpan.FromSeconds(requestTimeoutSeconds); });

        _logger.LogInformation("KrakenApiService initialized with {TimeoutSeconds}s timeout", requestTimeoutSeconds);
    }

    /// <summary>
    /// Sets the API credentials to use for subsequent API calls
    /// </summary>
    /// <param name="apiKey">Kraken API key</param>
    /// <param name="apiSecret">Kraken API secret</param>
    public void SetCredentials(string apiKey, string apiSecret)
    {
        _logger.LogDebug("Setting Kraken API credentials");
        _currentCredentials = new ApiCredentials(apiKey, apiSecret);
        _krakenClient.SetApiCredentials(_currentCredentials);
    }

    /// <summary>
    /// Clears the currently set API credentials, reverting to default credentials from config
    /// </summary>
    public void ClearCredentials()
    {
        _logger.LogDebug("Clearing Kraken API credentials");
        _currentCredentials = null;
        // Revert to original credentials from constructor if they exist
    }

    #region Klines (OHLCV) Data

    /// <summary>
    /// Gets 1-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> Get1MinuteKlinesAsync(string symbol, int intervalsBack)
    {
        intervalsBack++;
        var minutesPerInterval = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-intervalsBack * minutesPerInterval);
        var klineInterval = GetKlineInterval(Timeframe.OneMinute);
        return await GetKlinesAsync(klineInterval, symbol, startTime);
    }

    /// <summary>
    /// Gets 5-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> Get5MinuteKlinesAsync(string symbol, int intervalsBack)
    {
        intervalsBack++;
        var minutesPerInterval = 5;
        var startTime = DateTime.UtcNow.AddMinutes(-intervalsBack * minutesPerInterval);
        var klineInterval = GetKlineInterval(Timeframe.FiveMinutes);
        return await GetKlinesAsync(klineInterval, symbol, startTime);
    }

    /// <summary>
    /// Gets 15-minute kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> Get15MinuteKlinesAsync(string symbol, int intervalsBack)
    {
        intervalsBack++;
        var minutesPerInterval = 15;
        var startTime = DateTime.UtcNow.AddMinutes(-intervalsBack * minutesPerInterval);
        var klineInterval = GetKlineInterval(Timeframe.FifteenMinutes);
        return await GetKlinesAsync(klineInterval, symbol, startTime);
    }

    /// <summary>
    /// Gets hourly kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> GetHourlyKlinesAsync(string symbol, int intervalsBack)
    {
        intervalsBack++;
        var startTime = DateTime.UtcNow.AddHours(-intervalsBack);
        var klineInterval = GetKlineInterval(Timeframe.OneHour);
        return await GetKlinesAsync(klineInterval, symbol, startTime);
    }

    /// <summary>
    /// Gets daily kline data for the specified symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT")</param>
    /// <param name="intervalsBack">Number of intervals to retrieve</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> GetDailyKlinesAsync(string symbol, int intervalsBack)
    {
        intervalsBack++;
        var startTime = DateTime.UtcNow.AddDays(-intervalsBack);
        var klineInterval = GetKlineInterval(Timeframe.OneDay);
        return await GetKlinesAsync(klineInterval, symbol, startTime);
    }

    /// <summary>
    /// Gets kline data with custom parameters
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="timeframe">Timeframe for the klines</param>
    /// <param name="startTime">Start time for data retrieval</param>
    /// <param name="endTime">Optional end time for data retrieval</param>
    /// <returns>List of kline data</returns>
    public async Task<List<KlineData>> GetKlinesAsync(string symbol, Timeframe timeframe, DateTime startTime, DateTime? endTime = null)
    {
        var klineInterval = GetKlineInterval(timeframe);
        return await GetKlinesAsync(klineInterval, symbol, startTime, endTime);
    }

    #endregion

    /// <summary>
    /// Validates API credentials by attempting to query account balance
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string apiKey, string apiSecret)
    {
        try
        {
            _logger.LogInformation("Validating Kraken API credentials");

            // Create a temporary client with the provided credentials
            var tempClient = new KrakenRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(apiKey, apiSecret);
            });

            // Try to get account balance - this requires valid credentials
            var result = await tempClient.SpotApi.Account.GetBalancesAsync();

            if (result.Success)
            {
                _logger.LogInformation("Kraken API credentials are valid");
                return true;
            }
            else
            {
                _logger.LogWarning("Kraken API credentials validation failed: {Error}", result.Error?.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Kraken API credentials");
            return false;
        }
    }

    /// <summary>
    /// Gets the balance for a specific currency from Kraken account
    /// </summary>
    public async Task<decimal> GetCurrencyBalanceAsync(string apiKey, string apiSecret, string currency)
    {
        try
        {
            _logger.LogInformation("Getting {Currency} balance from Kraken", currency);

            // Create a temporary client with the provided credentials
            var tempClient = new KrakenRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(apiKey, apiSecret);
            });

            var result = await tempClient.SpotApi.Account.GetBalancesAsync();

            if (!result.Success)
            {
                _logger.LogError("Failed to get Kraken balances: {Error}", result.Error?.Message);
                throw new InvalidOperationException($"Failed to get Kraken balances: {result.Error?.Message}");
            }

            // The result.Data is a Dictionary<string, decimal>
            // Find the balance for the specified currency
            if (result.Data.TryGetValue(currency, out var balance))
            {
                _logger.LogInformation("Kraken {Currency} balance: {Balance}", currency, balance);
                return balance;
            }

            _logger.LogWarning("Currency {Currency} not found in Kraken account", currency);
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {Currency} balance from Kraken", currency);
            throw;
        }
    }

    /// <summary>
    /// Gets the account balance for all assets
    /// </summary>
    /// <returns>Dictionary of asset names and their balances</returns>
    public async Task<Dictionary<string, decimal>> GetAccountBalanceAsync()
    {
        _logger.LogInformation("Fetching account balance from Kraken");

        try
        {
            var result = await _krakenClient.SpotApi.Account.GetBalancesAsync();

            if (!result.Success)
            {
                _logger.LogError("Failed to get Kraken balances: {Error}", result.Error?.Message);
                throw new InvalidOperationException($"Failed to get Kraken balances: {result.Error?.Message}");
            }

            if (result.Data == null || result.Data.Count == 0)
            {
                _logger.LogWarning("No balances returned from Kraken");
                return new Dictionary<string, decimal>();
            }

            _logger.LogInformation("Retrieved {Count} asset balances", result.Data.Count);
            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching account balance from Kraken");
            throw;
        }
    }

    /// <summary>
    /// Gets ticker information for a specific trading pair
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSD", "ETHUSD")</param>
    /// <returns>Ticker data with last price</returns>
    public async Task<TickerData?> GetTickerAsync(string symbol)
    {
        try
        {
            _logger.LogDebug("Getting ticker for symbol: {Symbol}", symbol);

            var result = await _krakenClient.SpotApi.ExchangeData.GetTickerAsync(symbol);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to get ticker for {Symbol}: {Error}", symbol, result.Error?.Message);
                return null;
            }

            var ticker = result.Data.FirstOrDefault().Value;
            if (ticker == null)
            {
                _logger.LogWarning("No ticker data returned for symbol: {Symbol}", symbol);
                return null;
            }

            return new TickerData
            {
                Symbol = symbol,
                LastPrice = ticker.LastTrade.Price,
                BidPrice = ticker.BestBids.Price,
                AskPrice = ticker.BestAsks.Price,
                Volume24h = ticker.Volume.Value24H,
                High24h = ticker.High.Value24H,
                Low24h = ticker.Low.Value24H
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticker for symbol: {Symbol}", symbol);
            return null;
        }
    }

    #region Private Methods

    /// <summary>
    /// Internal method to retrieve kline data from Kraken API
    /// </summary>
    private async Task<List<KlineData>> GetKlinesAsync(KlineInterval klineInterval, string symbol, DateTime since, DateTime? until = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var klinesDataList = new List<KlineData>();

        try
        {
            _logger.LogDebug("Requesting klines for {Symbol} with interval {Interval} since {Since}",
                symbol, klineInterval, since);

            var response = await _krakenClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, since);

            if (!response.Success)
            {
                var errorMessage = response.Error?.Message ?? "Unknown error occurred";
                _logger.LogError("Kraken API error for symbol {Symbol}: {Error}", symbol, errorMessage);
                throw new KrakenApiException($"Failed to retrieve klines for {symbol}: {errorMessage}");
            }

            var result = response.Data;
            if (result?.Data == null)
            {
                _logger.LogWarning("No kline data returned for symbol {Symbol}", symbol);
                return klinesDataList;
            }

            foreach (var record in result.Data)
            {
                // Filter by end time if specified
                if (until.HasValue && record.OpenTime > until.Value)
                    continue;

                klinesDataList.Add(new KlineData
                {
                    OpeningPrice = record.OpenPrice,
                    HighestPrice = record.HighPrice,
                    LowestPrice = record.LowPrice,
                    ClosingPrice = record.ClosePrice,
                    ChartTimeEpoch = record.OpenTime.ToEpochMilliseconds(),
                    Volume = record.Volume,
                    VolumeWeightedAveragePrice = record.VolumeWeightedAveragePrice,
                    NumberOfTrades = record.TradeCount
                });
            }

            _logger.LogDebug("Retrieved {Count} klines for symbol {Symbol}", klinesDataList.Count, symbol);
        }
        catch (KrakenApiException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting klines for symbol {Symbol}", symbol);
            throw new KrakenApiException($"Unexpected error retrieving klines for {symbol}: {ex.Message}", ex);
        }

        return klinesDataList;
    }

    /// <summary>
    /// Maps timeframe enum to Kraken.Net KlineInterval
    /// </summary>
    private static KlineInterval GetKlineInterval(Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => KlineInterval.OneMinute,
            Timeframe.FiveMinutes => KlineInterval.FiveMinutes,
            Timeframe.FifteenMinutes => KlineInterval.FifteenMinutes,
            Timeframe.ThirtyMinutes => KlineInterval.ThirtyMinutes,
            Timeframe.OneHour => KlineInterval.OneHour,
            Timeframe.FourHours => KlineInterval.FourHour,
            Timeframe.OneDay => KlineInterval.OneDay,
            Timeframe.OneWeek => KlineInterval.OneWeek,
            _ => throw new NotSupportedException($"Timeframe {timeframe} is not supported by Kraken API")
        };
    }

    #endregion
}