namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for market data operations, validation, and quality analysis
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly ILogger<MarketDataService> _logger;

    /// <summary>
    /// Gets timeframe minutes from string representation
    /// </summary>
    private static int GetTimeframeMinutes(string timeframe)
    {
        return timeframe.ToLowerInvariant() switch
        {
            "1m" => (int)Timeframe.OneMinute,
            "5m" => (int)Timeframe.FiveMinutes,
            "15m" => (int)Timeframe.FifteenMinutes,
            "30m" => (int)Timeframe.ThirtyMinutes,
            "1h" => (int)Timeframe.OneHour,
            "4h" => (int)Timeframe.FourHours,
            "1d" => (int)Timeframe.OneDay,
            "1w" => (int)Timeframe.OneWeek,
            _ => throw new ArgumentException($"Unknown timeframe: {timeframe}")
        };
    }

    /// <summary>
    /// Tries to get timeframe minutes from string representation
    /// </summary>
    private static bool TryGetTimeframeMinutes(string timeframe, out int minutes)
    {
        try
        {
            minutes = GetTimeframeMinutes(timeframe);
            return true;
        }
        catch
        {
            minutes = 0;
            return false;
        }
    }

    /// <summary>
    /// Gets all supported timeframes
    /// </summary>
    private static readonly string[] SupportedTimeframes = ["1m", "5m", "15m", "30m", "1h", "4h", "1d", "1w"];

    public MarketDataService(IMarketDataRepository marketDataRepository, ILogger<MarketDataService> logger)
    {
        _marketDataRepository = marketDataRepository ?? throw new ArgumentNullException(nameof(marketDataRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets market data for a symbol within a specified time range
    /// </summary>
    /// <param name="symbol">Trading symbol (e.g., BTCUSDT)</param>
    /// <param name="dataSource">Optional data source filter (e.g., kraken, kucoin)</param>
    /// <param name="timeframe">Timeframe interval (e.g., 1m, 5m, 1h, 1d)</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <param name="limit">Optional limit on number of records</param>
    /// <returns>Collection of market data models ordered by timestamp</returns>
    public async Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(string symbol, string? dataSource, string timeframe, DateTime startDate, DateTime endDate, int? limit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        _logger.LogInformation("Retrieving market data for {Symbol} ({Timeframe}) from {Start} to {End} from {DataSource}",
            symbol, timeframe, startDate, endDate, dataSource ?? "any");

        try
        {
            var marketData = await _marketDataRepository.GetPriceDataAsync(symbol, timeframe, startDate, endDate, dataSource);

            var dataList = marketData.OrderBy(x => x.Timestamp).ToList();

            // Apply limit if specified
            if (limit.HasValue && limit.Value > 0)
            {
                dataList = dataList.Take(limit.Value).ToList();
            }

            _logger.LogInformation("Retrieved {Count} data points for {Symbol}", dataList.Count, symbol);

            return dataList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve market data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets the latest market data point for a symbol
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <param name="timeframe">Timeframe interval</param>
    /// <returns>Latest market data model or null if not found</returns>
    public async Task<MarketDataModel?> GetLatestMarketDataAsync(string symbol, string? dataSource = null, string timeframe = "1h")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _logger.LogDebug("Retrieving latest market data for {Symbol} from {DataSource}", symbol, dataSource ?? "any");

        try
        {
            return await _marketDataRepository.GetLatestPriceAsync(symbol, dataSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve latest market data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Validates market data quality and detects gaps
    /// </summary>
    /// <param name="marketData">Collection of market data to validate</param>
    /// <param name="expectedTimeframe">Expected timeframe interval</param>
    /// <returns>Data quality report with validation results</returns>
    public async Task<MarketDataQualityReport> ValidateDataQualityAsync(IEnumerable<MarketDataModel> marketData, string expectedTimeframe)
    {
        ArgumentNullException.ThrowIfNull(marketData);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedTimeframe);

        var dataList = marketData.OrderBy(x => x.Timestamp).ToList();
        var report = new MarketDataQualityReport
        {
            Timeframe = expectedTimeframe,
            TotalDataPoints = dataList.Count,
            GeneratedAt = DateTime.UtcNow
        };

        if (dataList.Count == 0)
        {
            report.IsValid = false;
            report.ValidationErrors.Add("No data points provided for validation");
            return report;
        }

        // Set basic info from first data point
        var firstPoint = dataList.First();
        report.Symbol = firstPoint.Symbol;
        report.StartDate = firstPoint.Timestamp;
        report.EndDate = dataList.Last().Timestamp;

        await Task.Run(() =>
        {
            ValidatePriceRelationships(dataList, report);
            DetectDuplicateTimestamps(dataList, report);
            CountZeroVolumeCandles(dataList, report);
            CalculateDataCompleteness(dataList, expectedTimeframe, report);
        });

        report.IsValid = report.ValidationErrors.Count == 0;

        _logger.LogInformation("Data quality validation completed for {Symbol}: {Valid}, {Completeness:P2} complete",
            report.Symbol, report.IsValid ? "Valid" : "Invalid", report.DataCompleteness);

        return report;
    }

    /// <summary>
    /// Detects gaps in market data for a given time range
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <param name="timeframe">Timeframe interval</param>
    /// <param name="startDate">Start date for gap detection</param>
    /// <param name="endDate">End date for gap detection</param>
    /// <returns>Collection of detected data gaps</returns>
    public async Task<IEnumerable<DataGap>> GetDataGapsAsync(string symbol, string? dataSource, string timeframe, DateTime startDate, DateTime endDate)
    {
        var marketData = await GetMarketDataAsync(symbol, dataSource, timeframe, startDate, endDate);
        var dataList = marketData.OrderBy(x => x.Timestamp).ToList();

        if (dataList.Count == 0)
            return [];

        if (!TryGetTimeframeMinutes(timeframe, out var intervalMinutes))
        {
            _logger.LogWarning("Unknown timeframe {Timeframe}, cannot detect gaps", timeframe);
            return [];
        }

        var gaps = new List<DataGap>();
        var expectedInterval = TimeSpan.FromMinutes(intervalMinutes);

        for (int i = 1; i < dataList.Count; i++)
        {
            var previousTime = dataList[i - 1].Timestamp;
            var currentTime = dataList[i].Timestamp;
            var actualGap = currentTime - previousTime;

            if (actualGap > expectedInterval.Add(TimeSpan.FromMinutes(1))) // Allow 1 minute tolerance
            {
                var missingPoints = (int)(actualGap.TotalMinutes / intervalMinutes) - 1;
                gaps.Add(new DataGap
                {
                    StartTime = previousTime.Add(expectedInterval),
                    EndTime = currentTime.Subtract(expectedInterval),
                    Duration = actualGap.Subtract(expectedInterval),
                    MissingDataPoints = missingPoints,
                    TimeframeInterval = timeframe
                });
            }
        }

        _logger.LogInformation("Detected {GapCount} gaps in {Symbol} data", gaps.Count, symbol);
        return gaps;
    }

    /// <summary>
    /// Gets data availability summary for a symbol
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <returns>Data availability information</returns>
    public async Task<DataAvailabilityInfo> GetDataAvailabilityAsync(string symbol, string? dataSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _logger.LogDebug("Checking data availability for {Symbol} from {DataSource}",
            symbol, dataSource ?? "any");

        var availability = new DataAvailabilityInfo
        {
            Symbol = symbol,
            DataSource = dataSource,
            CheckedAt = DateTime.UtcNow
        };

        // Check each timeframe
        foreach (var timeframe in SupportedTimeframes)
        {
            try
            {
                // Get a sample of recent data to determine availability
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30); // Check last 30 days

                var sampleData = await GetMarketDataAsync(symbol, dataSource, timeframe, startDate, endDate);
                var dataList = sampleData.OrderBy(x => x.Timestamp).ToList();

                if (dataList.Count > 0)
                {
                    var timeframeInfo = new TimeframeAvailability
                    {
                        Timeframe = timeframe,
                        FirstAvailable = dataList.First().Timestamp,
                        LastAvailable = dataList.Last().Timestamp,
                        DataPointCount = dataList.Count
                    };

                    // Calculate expected data points for the sample period
                    var intervalMinutes = GetTimeframeMinutes(timeframe);
                    var expectedPoints = (int)((endDate - startDate).TotalMinutes / intervalMinutes);
                    timeframeInfo.CompletenessPercentage = expectedPoints > 0
                        ? (decimal)dataList.Count / expectedPoints
                        : 0;

                    // Count gaps
                    var gaps = await GetDataGapsAsync(symbol, dataSource, timeframe, startDate, endDate);
                    timeframeInfo.GapCount = gaps.Count();

                    availability.TimeframeAvailability[timeframe] = timeframeInfo;

                    // Update overall availability info
                    if (availability.EarliestDataPoint == null || timeframeInfo.FirstAvailable < availability.EarliestDataPoint)
                        availability.EarliestDataPoint = timeframeInfo.FirstAvailable;

                    if (availability.LatestDataPoint == null || timeframeInfo.LastAvailable > availability.LatestDataPoint)
                        availability.LatestDataPoint = timeframeInfo.LastAvailable;

                    availability.TotalDataPoints += timeframeInfo.DataPointCount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check availability for {Symbol} timeframe {Timeframe}",
                    symbol, timeframe);
            }
        }

        if (availability.EarliestDataPoint.HasValue && availability.LatestDataPoint.HasValue)
        {
            availability.TotalDataSpan = availability.LatestDataPoint.Value - availability.EarliestDataPoint.Value;
        }

        return availability;
    }

    /// <summary>
    /// Stores market data points to the database
    /// </summary>
    /// <param name="marketData">Market data to store</param>
    /// <returns>Number of records successfully stored</returns>
    public async Task<int> StoreMarketDataAsync(IEnumerable<MarketDataModel> marketData)
    {
        ArgumentNullException.ThrowIfNull(marketData);

        var dataList = marketData.ToList();
        if (dataList.Count == 0)
            return 0;

        _logger.LogInformation("Storing {Count} market data points", dataList.Count);

        try
        {
            await _marketDataRepository.WritePriceDataBatchAsync(dataList);
            _logger.LogInformation("Successfully stored {Count} market data points", dataList.Count);
            return dataList.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store market data");
            throw;
        }
    }

    /// <summary>
    /// Normalizes market data timestamps to ensure consistent intervals
    /// </summary>
    /// <param name="marketData">Market data to normalize</param>
    /// <param name="timeframe">Target timeframe</param>
    /// <returns>Normalized market data collection</returns>
    public async Task<IEnumerable<MarketDataModel>> NormalizeTimestampsAsync(IEnumerable<MarketDataModel> marketData, string timeframe)
    {
        ArgumentNullException.ThrowIfNull(marketData);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);

        var dataList = marketData.OrderBy(x => x.Timestamp).ToList();
        if (dataList.Count == 0)
            return dataList;

        if (!TryGetTimeframeMinutes(timeframe, out var intervalMinutes))
        {
            _logger.LogWarning("Unknown timeframe {Timeframe}, returning original data", timeframe);
            return dataList;
        }

        var normalizedData = await Task.Run(() =>
        {
            var interval = TimeSpan.FromMinutes(intervalMinutes);
            var result = new List<MarketDataModel>();

            foreach (var data in dataList)
            {
                // Round timestamp to nearest interval
                var ticks = data.Timestamp.Ticks;
                var intervalTicks = interval.Ticks;
                var normalizedTicks = (ticks / intervalTicks) * intervalTicks;
                var normalizedTimestamp = new DateTime(normalizedTicks);

                var normalizedData = new MarketDataModel
                {
                    Symbol = data.Symbol,
                    Timeframe = data.Timeframe,
                    DataSource = data.DataSource,
                    Open = data.Open,
                    High = data.High,
                    Low = data.Low,
                    Close = data.Close,
                    Volume = data.Volume,
                    VWAP = data.VWAP,
                    TradeCount = data.TradeCount,
                    Bid = data.Bid,
                    Ask = data.Ask,
                    QuoteVolume = data.QuoteVolume,
                    Timestamp = normalizedTimestamp
                };

                result.Add(normalizedData);
            }

            return result;
        });

        _logger.LogDebug("Normalized {Count} timestamps for timeframe {Timeframe}",
            normalizedData.Count(), timeframe);

        return normalizedData;
    }

    /// <summary>
    /// Deletes market data for testing purposes
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="timeframe">Timeframe interval</param>
    /// <param name="startDate">Start date for deletion</param>
    /// <param name="endDate">End date for deletion</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <returns>Task representing the deletion operation</returns>
    public async Task DeleteMarketDataAsync(string symbol, string timeframe, DateTime startDate, DateTime endDate, string? dataSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        _logger.LogWarning("Deleting market data for {Symbol} ({Timeframe}) from {Start} to {End} for {DataSource}",
            symbol, timeframe, startDate, endDate, dataSource ?? "all sources");

        try
        {
            await _marketDataRepository.DeleteMarketDataAsync(symbol, timeframe, startDate, endDate, dataSource);

            _logger.LogInformation("Successfully deleted market data for {Symbol}", symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete market data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Deletes all market data for a symbol (use with extreme caution - for testing only)
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <returns>Task representing the deletion operation</returns>
    public async Task DeleteAllMarketDataAsync(string symbol, string? dataSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _logger.LogWarning("⚠️  DELETING ALL market data for {Symbol} from {DataSource} - THIS IS IRREVERSIBLE",
            symbol, dataSource ?? "all sources");

        try
        {
            await _marketDataRepository.DeleteAllMarketDataAsync(symbol, dataSource);

            _logger.LogWarning("✅ Successfully deleted ALL market data for {Symbol}", symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete all market data for {Symbol}", symbol);
            throw;
        }
    }



    #region Private Validation Methods

    /// <summary>
    /// Validates price relationships (High >= Open, Close >= Low, etc.)
    /// </summary>
    private static void ValidatePriceRelationships(List<MarketDataModel> dataList, MarketDataQualityReport report)
    {
        foreach (var data in dataList)
        {
            if (data.High < data.Open || data.High < data.Close || data.High < data.Low ||
                data.Low > data.Open || data.Low > data.Close || data.Low > data.High)
            {
                report.InvalidPriceRelationships++;
                report.ValidationErrors.Add($"Invalid price relationship at {data.Timestamp}: O:{data.Open} H:{data.High} L:{data.Low} C:{data.Close}");
            }
        }
    }

    /// <summary>
    /// Detects duplicate timestamps in the data
    /// </summary>
    private static void DetectDuplicateTimestamps(List<MarketDataModel> dataList, MarketDataQualityReport report)
    {
        var duplicates = dataList.GroupBy(x => x.Timestamp)
                                .Where(g => g.Count() > 1)
                                .ToList();

        report.DuplicateTimestamps = duplicates.Count;
        foreach (var duplicate in duplicates)
        {
            report.ValidationErrors.Add($"Duplicate timestamp: {duplicate.Key}");
        }
    }

    /// <summary>
    /// Counts candles with zero volume
    /// </summary>
    private static void CountZeroVolumeCandles(List<MarketDataModel> dataList, MarketDataQualityReport report)
    {
        report.ZeroVolumeCandles = dataList.Count(x => x.Volume == 0);
        if (report.ZeroVolumeCandles > 0)
        {
            report.ValidationErrors.Add($"Found {report.ZeroVolumeCandles} candles with zero volume");
        }
    }

    /// <summary>
    /// Calculates data completeness based on expected timeframe intervals
    /// </summary>
    private static void CalculateDataCompleteness(List<MarketDataModel> dataList, string timeframe, MarketDataQualityReport report)
    {
        if (dataList.Count < 2)
        {
            report.DataCompleteness = dataList.Count > 0 ? 1.0m : 0.0m;
            return;
        }

        if (!TryGetTimeframeMinutes(timeframe, out var intervalMinutes))
        {
            report.DataCompleteness = 1.0m; // Can't validate unknown timeframe
            return;
        }

        var totalSpan = dataList.Last().Timestamp - dataList.First().Timestamp;
        var expectedPoints = (int)(totalSpan.TotalMinutes / intervalMinutes) + 1;

        report.ExpectedDataPoints = expectedPoints;
        report.DataCompleteness = expectedPoints > 0 ? (decimal)dataList.Count / expectedPoints : 0.0m;
    }

    #endregion
}