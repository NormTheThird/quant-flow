namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for market data operations and gap detection
/// This service focuses on data retrieval and gap detection - it does NOT populate gaps from APIs
/// Gap population should be handled by exchange-specific services like KrakenMarketDataCollectionService
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly ILogger<MarketDataService> _logger;

    public MarketDataService(IMarketDataRepository marketDataRepository, ILogger<MarketDataService> logger)
    {
        _marketDataRepository = marketDataRepository ?? throw new ArgumentNullException(nameof(marketDataRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets market data for a symbol within a specified time range
    /// </summary>
    public async Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate, int? limit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        _logger.LogInformation("Retrieving market data for {Symbol} ({Timeframe}) from {Start} to {End} from {Exchange}",
            symbol, timeframe, startDate, endDate, exchange);

        try
        {
            var marketData = await _marketDataRepository.GetPriceDataAsync(exchange, symbol, timeframe, startDate, endDate);
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
    public async Task<MarketDataModel?> GetLatestMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _logger.LogDebug("Retrieving latest market data for {Symbol} from {Exchange}", symbol, exchange);

        try
        {
            return await _marketDataRepository.GetLatestPriceAsync(exchange, symbol, timeframe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve latest market data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Detects gaps in market data for a given time range (legacy method - finds gaps between existing data)
    /// </summary>
    public async Task<IEnumerable<DataGap>> GetDataGapsAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate)
    {
        var marketData = await GetMarketDataAsync(exchange, symbol, timeframe, startDate, endDate);
        var dataList = marketData.OrderBy(x => x.Timestamp).ToList();

        var intervalMinutes = GetTimeframeMinutes(timeframe);
        var gaps = new List<DataGap>();
        var expectedInterval = TimeSpan.FromMinutes(intervalMinutes);

        // Check for gap from baseline (2020-01-01) to first data point
        if (dataList.Count > 0)
        {
            var baselineDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDataPoint = dataList[0].Timestamp;
            var initialGap = firstDataPoint - baselineDate;

            if (initialGap > expectedInterval)
            {
                var missingPoints = (int)(initialGap.TotalMinutes / intervalMinutes) - 1;
                if (missingPoints > 0)
                {
                    gaps.Add(new DataGap
                    {
                        StartTime = baselineDate,
                        EndTime = firstDataPoint.Subtract(expectedInterval),
                        Duration = initialGap.Subtract(expectedInterval),
                        MissingDataPoints = missingPoints,
                        TimeframeInterval = timeframe.ToString()
                    });
                }
            }
        }
        else
        {
            // No data at all - entire range is a gap
            var totalGap = endDate - startDate;
            var missingPoints = (int)(totalGap.TotalMinutes / intervalMinutes);
            gaps.Add(new DataGap
            {
                StartTime = startDate,
                EndTime = endDate,
                Duration = totalGap,
                MissingDataPoints = missingPoints,
                TimeframeInterval = timeframe.ToString()
            });

            _logger.LogWarning("No data found for {Symbol} - entire range is a gap", symbol);
            return gaps;
        }

        // Check for gaps between existing data points
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
                    TimeframeInterval = timeframe.ToString()
                });
            }
        }

        _logger.LogInformation("Detected {GapCount} gaps in {Symbol} data", gaps.Count, symbol);
        return gaps;
    }

    /// <summary>
    /// Enhanced gap detection that finds ALL missing intervals from start date to end date
    /// This method identifies complete missing ranges regardless of existing data
    /// NOTE: This method only DETECTS gaps, it does NOT populate them
    /// </summary>
    public async Task<IEnumerable<MissingDataRange>> GetMissingIntervalsAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime? endDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var end = endDate ?? DateTime.UtcNow;
        if (startDate >= end)
            throw new ArgumentException("Start date must be before end date");

        _logger.LogInformation("Detecting missing intervals for {Symbol} ({Timeframe}) from {Start} to {End} on {Exchange}",
            symbol, timeframe, startDate, end, exchange);

        try
        {
            // Generate all expected timestamps for the date range
            var expectedTimestamps = GenerateExpectedTimestamps(startDate, end, timeframe);

            // Get existing data from database (using existing method)
            var existingData = await GetMarketDataAsync(exchange, symbol, timeframe, startDate, end);
            var existingTimestamps = new HashSet<DateTime>(existingData.Select(d => d.Timestamp));

            var missingRanges = new List<MissingDataRange>();
            var currentRangeStart = (DateTime?)null;

            foreach (var expectedTimestamp in expectedTimestamps)
            {
                bool isDataMissing = !existingTimestamps.Contains(expectedTimestamp);

                if (isDataMissing)
                {
                    // Start a new missing range if not already in one
                    if (currentRangeStart == null)
                    {
                        currentRangeStart = expectedTimestamp;
                    }
                }
                else
                {
                    // Data exists - close any current missing range
                    if (currentRangeStart.HasValue)
                    {
                        var missingRange = CreateMissingDataRange(currentRangeStart.Value, expectedTimestamp, timeframe);
                        missingRanges.Add(missingRange);
                        currentRangeStart = null;
                    }
                }
            }

            // Handle case where missing range extends to the end
            if (currentRangeStart.HasValue)
            {
                var missingRange = CreateMissingDataRange(currentRangeStart.Value, end, timeframe);
                missingRanges.Add(missingRange);
            }

            _logger.LogInformation("Found {Count} missing data ranges for {Symbol} ({Timeframe}) - ready for population by exchange service",
                missingRanges.Count, symbol, timeframe);

            return missingRanges;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect missing intervals for {Symbol} on {Exchange}", symbol, exchange);
            throw;
        }
    }

    public async Task<IEnumerable<MarketDataSummary>> GetDataAvailabilitySummaryAsync()
    {
        _logger.LogInformation("Getting market data availability summary");
        return await _marketDataRepository.GetDataAvailabilitySummaryAsync();
    }

    /// <summary>
    /// Stores market data points to the database
    /// </summary>
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


    #region Private Helper Methods

    /// <summary>
    /// Generates expected timestamps for gap detection
    /// </summary>
    private List<DateTime> GenerateExpectedTimestamps(DateTime start, DateTime end, Timeframe timeframe)
    {
        var timestamps = new List<DateTime>();
        var intervalMinutes = GetTimeframeMinutes(timeframe);
        var current = start;

        while (current <= end)
        {
            timestamps.Add(current);
            current = current.AddMinutes(intervalMinutes);
        }

        _logger.LogDebug("Generated {Count} expected timestamps for {Timeframe} from {Start} to {End}",
            timestamps.Count, timeframe, start, end);

        return timestamps;
    }

    /// <summary>
    /// Creates a missing data range object for gap reporting
    /// </summary>
    private MissingDataRange CreateMissingDataRange(DateTime startTime, DateTime endTime, Timeframe timeframe)
    {
        var intervalMinutes = GetTimeframeMinutes(timeframe);
        var duration = endTime - startTime;
        var expectedPoints = (int)(duration.TotalMinutes / intervalMinutes);

        return new MissingDataRange
        {
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            ExpectedDataPoints = expectedPoints,
            Timeframe = timeframe.ToString(),
            Description = $"Missing data for {duration.TotalHours:F1} hours ({expectedPoints} points)"
        };
    }

    /// <summary>
    /// Gets timeframe minutes from enum
    /// </summary>
    private static int GetTimeframeMinutes(Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => 1,
            Timeframe.FiveMinutes => 5,
            Timeframe.FifteenMinutes => 15,
            Timeframe.ThirtyMinutes => 30,
            Timeframe.OneHour => 60,
            Timeframe.FourHours => 240,
            Timeframe.OneDay => 1440,
            Timeframe.OneWeek => 10080,
            _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
        };
    }

    #endregion
}