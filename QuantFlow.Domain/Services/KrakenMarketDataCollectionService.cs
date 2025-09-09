using QuantFlow.Common.Interfaces.Infrastructure;

namespace QuantFlow.Domain.Services;

public class KrakenMarketDataCollectionService : IKrakenMarketDataCollectionService
{
    private readonly ILogger<KrakenMarketDataCollectionService> _logger;
    private readonly IKrakenApiService _krakenApiService;
    private readonly KrakenCredentials _credentials;
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly IApiRateLimitHandler _apiRateLimitHandler;

    public KrakenMarketDataCollectionService(ILogger<KrakenMarketDataCollectionService> logger, IKrakenApiService krakenApiService, KrakenCredentials credentials,
                                             IMarketDataRepository marketDataRepository, IApiRateLimitHandler apiRateLimitHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _krakenApiService = krakenApiService ?? throw new ArgumentNullException(nameof(krakenApiService));
        _marketDataRepository = marketDataRepository ?? throw new ArgumentNullException(nameof(marketDataRepository));
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _apiRateLimitHandler = apiRateLimitHandler ?? throw new ArgumentNullException(nameof(apiRateLimitHandler));
    }

    /// <summary>
    /// Gets market data for the specified parameters - used by DataCollectionOrchestrator
    /// </summary>
    public async Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(string symbol, string timeframe, DateTime startTime, DateTime endTime)
    {
        // Convert string timeframe to enum
        var timeframeEnum = ConvertStringToTimeframe(timeframe);

        // Get kline data from Kraken API using date range
        var klineData = await _krakenApiService.GetKlinesAsync(symbol, timeframeEnum, startTime, endTime);

        // Transform using the flexible method
        return TransformKlineDataToMarketData(klineData, symbol, timeframeEnum);
    }

    /// <summary>
    /// Gets market data for the specified parameters - used by DataCollectionOrchestrator (overload with Timeframe enum)
    /// </summary>
    public async Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(string symbol, Timeframe timeframe, DateTime startTime, DateTime endTime)
    {
        // Get kline data from Kraken API using date range
        var klineData = await _krakenApiService.GetKlinesAsync(symbol, timeframe, startTime, endTime);

        // Transform using existing method
        return TransformKlineDataToMarketData(klineData, symbol, timeframe);
    }

    public async Task CollectDataAsync(string symbol, Timeframe timeframe, int intervalsBack = 100)
    {
        _logger.LogInformation("Collecting {IntervalsBack} intervals of {Timeframe} data for {Symbol}",
            intervalsBack, timeframe, symbol);

        try
        {
            // Get data from Kraken using your existing service
            var klineData = await GetKlineDataFromKrakenAsync(symbol, timeframe, intervalsBack);

            if (klineData?.Any() == true)
            {
                // Transform to your domain models
                var marketDataList = TransformKlineDataToMarketData(klineData, symbol, timeframe).ToList();

                // Filter out duplicates
                var filteredData = await FilterDuplicatesAsync(marketDataList, symbol, timeframe);

                if (filteredData.Any())
                {
                    // Store in InfluxDB
                    await _marketDataRepository.WritePriceDataBatchAsync(filteredData);

                    _logger.LogInformation("Successfully stored {Count} new data points for {Symbol} {Timeframe}",
                        filteredData.Count(), symbol, timeframe);
                }
                else
                {
                    _logger.LogInformation("No new data to store for {Symbol} {Timeframe} (all were duplicates)",
                        symbol, timeframe);
                }
            }
            else
            {
                _logger.LogWarning("No data received from Kraken for {Symbol} {Timeframe}", symbol, timeframe);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting data for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    public async Task CollectDataAsync(List<SymbolConfig> symbolConfigs)
    {
        _logger.LogInformation("Collecting data for {SymbolCount} symbols", symbolConfigs.Count);

        foreach (var symbolConfig in symbolConfigs)
        {
            foreach (var timeframe in symbolConfig.Timeframes)
            {
                try
                {
                    await CollectDataAsync(symbolConfig.Symbol, timeframe);

                    // Small delay to be respectful to Kraken API
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to collect data for {Symbol} {Timeframe}, continuing with next",
                        symbolConfig.Symbol, timeframe);
                    // Continue with other symbols/timeframes
                }
            }
        }
    }

    public async Task CollectRecentDataAsync(string symbol, Timeframe timeframe)
    {
        _logger.LogInformation("Collecting recent data for {Symbol} {Timeframe}", symbol, timeframe);

        try
        {
            // Calculate how much data we need based on what we already have
            var intervalsNeeded = await CalculateMissingIntervalsAsync(symbol, timeframe);

            if (intervalsNeeded > 0)
            {
                await CollectDataAsync(symbol, timeframe, intervalsNeeded);
            }
            else
            {
                _logger.LogInformation("Data for {Symbol} {Timeframe} is already up to date", symbol, timeframe);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting recent data for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    /// <summary>
    /// Populates missing data ranges specifically from Kraken API
    /// This belongs in the Kraken service because it's Kraken-specific logic
    /// </summary>
    public async Task<DataPopulationResult> PopulateMissingDataAsync(string symbol, Timeframe timeframe, IEnumerable<MissingDataRange> missingRanges)
    {
        var result = new DataPopulationResult
        {
            ProcessingStartTime = DateTime.UtcNow,
            TotalRangesProcessed = missingRanges.Count()
        };

        _logger.LogInformation("🔄 Starting Kraken data population for {Symbol} ({Timeframe}) - {Count} ranges",
            symbol, timeframe, result.TotalRangesProcessed);

        try
        {
            foreach (var range in missingRanges)
            {
                try
                {
                    _logger.LogDebug("🔍 Populating Kraken data for range {Start} to {End}",
                        range.StartTime, range.EndTime);

                    // Use rate limit handler for Kraken API calls
                    var newData = await _apiRateLimitHandler.ExecuteWithRateLimitHandlingAsync(async () =>
                    {
                        var klineData = await _krakenApiService.GetKlinesAsync(symbol, timeframe, range.StartTime, range.EndTime);
                        return TransformKlineDataToMarketData(klineData, symbol, timeframe);
                    }, $"PopulateGap_{symbol}_{timeframe}_{range.StartTime:yyyyMMdd}");

                    if (newData?.Any() == true)
                    {
                        // Store the new data
                        await _marketDataRepository.WritePriceDataBatchAsync(newData);
                        result.NewDataPointsAdded += newData.Count(); // Use the count of data we tried to store
                        result.SuccessfulRanges++;

                        _logger.LogDebug("✅ Successfully populated {Count} Kraken data points for range {Start} to {End}",
                            newData.Count(), range.StartTime, range.EndTime);
                    }
                    else
                    {
                        result.FailedRanges++;
                        result.RemainingGaps.Add(range);

                        _logger.LogWarning("⚠️ No data returned from Kraken for {Symbol} range {Start} to {End}",
                            symbol, range.StartTime, range.EndTime);
                    }
                }
                catch (RateLimitExceededException ex)
                {
                    result.FailedRanges++;
                    result.RemainingGaps.Add(range);
                    result.Errors.Add($"Kraken rate limit exceeded for range {range.StartTime} to {range.EndTime}: {ex.Message}");

                    _logger.LogError(ex, "🚫 Kraken rate limit exceeded for range {Start} to {End}",
                        range.StartTime, range.EndTime);

                    // Stop processing on persistent rate limits
                    _logger.LogWarning("🛑 Stopping Kraken data population due to rate limiting");
                    break;
                }
                catch (Exception ex)
                {
                    result.FailedRanges++;
                    result.RemainingGaps.Add(range);
                    result.Errors.Add($"Failed to populate Kraken data for range {range.StartTime} to {range.EndTime}: {ex.Message}");

                    _logger.LogError(ex, "❌ Failed to populate Kraken data for range {Start} to {End}",
                        range.StartTime, range.EndTime);
                }
            }

            result.ProcessingEndTime = DateTime.UtcNow;
            result.TotalProcessingTime = result.ProcessingEndTime - result.ProcessingStartTime;

            _logger.LogInformation("📊 Kraken data population completed: {Success}/{Total} ranges successful, {NewPoints} new points",
                result.SuccessfulRanges, result.TotalRangesProcessed, result.NewDataPointsAdded);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Critical error during Kraken data population for {Symbol}", symbol);
            result.ProcessingEndTime = DateTime.UtcNow;
            result.TotalProcessingTime = result.ProcessingEndTime - result.ProcessingStartTime;
            result.Errors.Add($"Critical error: {ex.Message}");
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<List<KlineData>> GetKlineDataFromKrakenAsync(string symbol, Timeframe timeframe, int intervalsBack)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => await _krakenApiService.Get1MinuteKlinesAsync(symbol, intervalsBack),
            Timeframe.FiveMinutes => await _krakenApiService.Get5MinuteKlinesAsync(symbol, intervalsBack),
            Timeframe.FifteenMinutes => await _krakenApiService.Get15MinuteKlinesAsync(symbol, intervalsBack),
            Timeframe.OneHour => await _krakenApiService.GetHourlyKlinesAsync(symbol, intervalsBack),
            Timeframe.OneDay => await _krakenApiService.GetDailyKlinesAsync(symbol, intervalsBack),
            _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
        };
    }

    private IEnumerable<MarketDataModel> TransformKlineDataToMarketData(List<KlineData> klineData, string symbol, Timeframe timeframe)
    {
        return klineData.Select(kd => new MarketDataModel
        {
            Symbol = symbol,
            Exchange = Exchange.Kraken,
            Timeframe = timeframe,
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(kd.ChartTimeEpoch).DateTime,
            Open = kd.OpeningPrice,
            High = kd.HighestPrice,
            Low = kd.LowestPrice,
            Close = kd.ClosingPrice,
            Volume = kd.Volume,
            TradeCount = kd.NumberOfTrades,
            VWAP = kd.VolumeWeightedAveragePrice
        });
    }

    private static Timeframe ConvertStringToTimeframe(string timeframe)
    {
        return timeframe.ToLowerInvariant() switch
        {
            "1m" => Timeframe.OneMinute,
            "5m" => Timeframe.FiveMinutes,
            "15m" => Timeframe.FifteenMinutes,
            "30m" => Timeframe.ThirtyMinutes,
            "1h" => Timeframe.OneHour,
            "4h" => Timeframe.FourHours,
            "1d" => Timeframe.OneDay,
            "1w" => Timeframe.OneWeek,
            _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
        };
    }

    private async Task<IEnumerable<MarketDataModel>> FilterDuplicatesAsync(List<MarketDataModel> newData, string symbol, Timeframe timeframe)
    {
        if (!newData.Any())
            return Enumerable.Empty<MarketDataModel>();

        try
        {
            var startDate = newData.Min(d => d.Timestamp);
            var endDate = newData.Max(d => d.Timestamp);

            var existingData = await _marketDataRepository.GetPriceDataAsync(Exchange.Kraken, symbol, timeframe, startDate, endDate);

            var existingTimestamps = new HashSet<DateTime>(
                existingData.Select(d => d.Timestamp));

            return newData.Where(d => !existingTimestamps.Contains(d.Timestamp));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking for duplicates, returning all data");
            return newData;
        }
    }

    private async Task<int> CalculateMissingIntervalsAsync(string symbol, Timeframe timeframe)
    {
        try
        {
            var latestData = await _marketDataRepository.GetLatestPriceAsync(Exchange.Kraken, symbol, timeframe);
            if (latestData == null)
                return GetInitialHistoryAmount(timeframe);

            var now = DateTime.UtcNow;
            var timeSinceLastData = now - latestData.Timestamp;
            var intervalMinutes = (int)timeframe;
            var missedIntervals = (int)(timeSinceLastData.TotalMinutes / intervalMinutes);

            // Add buffer for safety
            return Math.Max(0, missedIntervals + 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not calculate missing intervals, using default");
            return GetInitialHistoryAmount(timeframe);
        }
    }

    private int GetInitialHistoryAmount(Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => 1440,     // 1 day
            Timeframe.FiveMinutes => 288,    // 1 day  
            Timeframe.FifteenMinutes => 96,  // 1 day
            Timeframe.OneHour => 168,        // 1 week
            Timeframe.OneDay => 30,          // 1 month - for cold start
            _ => 100
        };
    }

    #endregion
}