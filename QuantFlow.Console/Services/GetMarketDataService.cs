namespace QuantFlow.Console.Services;

/// <summary>
/// Console service for orchestrating market data collection from Kraken
/// </summary>
public class GetMarketDataService
{
    private readonly ILogger<GetMarketDataService> _logger;
    private readonly IKrakenMarketDataCollectionService _krakenCollectionService;
    private readonly IMarketDataService _marketDataService;

    public GetMarketDataService(ILogger<GetMarketDataService> logger, IKrakenMarketDataCollectionService krakenCollectionService, IMarketDataService marketDataService)
    {
        _krakenCollectionService = krakenCollectionService ?? throw new ArgumentNullException(nameof(krakenCollectionService));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Collects data for a single symbol and timeframe
    /// </summary>
    public async Task CollectDataAsync(string symbol, Timeframe timeframe, int intervalsBack = 100)
    {
        _logger.LogInformation("Starting data collection for {Symbol} {Timeframe} ({Intervals} intervals)", symbol, timeframe, intervalsBack);

        try
        {
            await _krakenCollectionService.CollectDataAsync(symbol, timeframe, intervalsBack);
            _logger.LogInformation("✅ Successfully collected data for {Symbol} {Timeframe}", symbol, timeframe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to collect data for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    /// <summary>
    /// Collects recent data to fill gaps since last collection
    /// </summary>
    public async Task CollectRecentDataAsync(string symbol, Timeframe timeframe)
    {
        _logger.LogInformation("Collecting recent data for {Symbol} {Timeframe}", symbol, timeframe);

        try
        {
            await _krakenCollectionService.CollectRecentDataAsync(symbol, timeframe);
            _logger.LogInformation("✅ Successfully collected recent data for {Symbol} {Timeframe}", symbol, timeframe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to collect recent data for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    /// <summary>
    /// Collects data for multiple symbol/timeframe combinations
    /// </summary>
    public async Task CollectDataBulkAsync(List<SymbolConfig> symbolConfigs)
    {
        _logger.LogInformation("Starting bulk data collection for {Count} symbol configurations", symbolConfigs.Count);

        var totalSuccess = 0;
        var totalFailed = 0;

        foreach (var config in symbolConfigs)
        {
            foreach (var timeframe in config.Timeframes)
            {
                try
                {
                    await CollectDataAsync(config.Symbol, timeframe);
                    totalSuccess++;

                    // Small delay to respect API rate limits
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to collect data for {Symbol} {Timeframe}", config.Symbol, timeframe);
                    totalFailed++;
                }
            }
        }

        _logger.LogInformation("Bulk collection completed: ✅ {Success} successful, ❌ {Failed} failed",
            totalSuccess, totalFailed);
    }

    /// <summary>
    /// Collects historical data for a new symbol (large dataset)
    /// </summary>
    public async Task CollectHistoricalDataAsync(string symbol, List<Timeframe> timeframes, int daysBack = 30)
    {
        _logger.LogInformation("Collecting {Days} days of historical data for {Symbol} across {TimeframeCount} timeframes", daysBack, symbol, timeframes.Count);

        foreach (var timeframe in timeframes)
        {
            try
            {
                // Calculate intervals needed based on timeframe and days
                var intervalsNeeded = CalculateIntervalsForDays(timeframe, daysBack);

                _logger.LogInformation("Collecting {Intervals} intervals of {Timeframe} data for {Symbol}",
                    intervalsNeeded, timeframe, symbol);

                await CollectDataAsync(symbol, timeframe, intervalsNeeded);

                // Delay between timeframes to respect rate limits
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect historical {Timeframe} data for {Symbol}", timeframe, symbol);
            }
        }

        _logger.LogInformation("✅ Historical data collection completed for {Symbol}", symbol);
    }

    /// <summary>
    /// Collects optimized timeframe-specific data for a single symbol
    /// </summary>
    public async Task CollectOptimizedDataForSymbolAsync(string symbol)
    {
        _logger.LogInformation("📊 Collecting optimized data for {Symbol}", symbol);

        var collectionTasks = new List<Task>
        {
            // Different time periods optimized for each timeframe
            CollectHistoricalDataAsync(symbol, [Timeframe.OneMinute], 7),      // 7 days
            CollectHistoricalDataAsync(symbol, [Timeframe.FiveMinutes], 14),   // 14 days
            CollectHistoricalDataAsync(symbol, [Timeframe.FifteenMinutes], 30), // 30 days
            CollectHistoricalDataAsync(symbol, [Timeframe.OneHour], 90),       // 90 days
            CollectHistoricalDataAsync(symbol, [Timeframe.OneDay], 365)        // 1 year
        };

        try
        {
            // Execute all timeframes in parallel for this symbol
            await Task.WhenAll(collectionTasks);
            _logger.LogInformation("✅ Completed optimized collection for {Symbol}", symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed optimized collection for {Symbol}: {Message}", symbol, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Collects optimized historical data for multiple symbols in parallel
    /// COMBINES Option 2 (optimized time periods) with Option 3 (parallel execution)
    /// </summary>
    public async Task CollectOptimizedParallelDataAsync()
    {
        var symbols = new[] { "BTCUSDT", "ETHUSDT", "ADAUSDT" };

        _logger.LogInformation("⚡ Starting optimized parallel data collection for {Count} symbols", symbols.Length);
        _logger.LogInformation("⚡ Starting optimized parallel data collection...");
        _logger.LogInformation(new string('=', 60));

        // Create parallel tasks for each symbol with optimized timeframes
        var symbolTasks = symbols.Select(async symbol =>
        {
            try
            {
                _logger.LogInformation($"🔄 Starting optimized collection for {symbol}...");

                // Each symbol gets its own parallel timeframe collection
                await CollectOptimizedDataForSymbolAsync(symbol);

                _logger.LogInformation($"✅ Completed optimized collection for {symbol}");
                return new CollectionResult { Symbol = symbol, Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"❌ Failed optimized collection for {symbol}: {ex.Message}");
                _logger.LogError(ex, "Failed optimized parallel collection for {Symbol}", symbol);
                return new CollectionResult { Symbol = symbol, Success = false, Error = ex.Message };
            }
        });

        // Wait for all symbols to complete
        var results = await Task.WhenAll(symbolTasks);

        // Report final results
        _logger.LogInformation("");
        _logger.LogInformation("📋 Final Collection Results:");
        _logger.LogInformation(new string('-', 40));

        foreach (var result in results)
        {
            var status = result.Success ? "✅ SUCCESS" : "❌ FAILED";
            _logger.LogInformation($"  {result.Symbol}: {status}");
            if (!result.Success && !string.IsNullOrEmpty(result.Error))
            {
                _logger.LogInformation($"    Error: {result.Error}");
            }
        }

        var successCount = results.Count(r => r.Success);
        _logger.LogInformation("");
        _logger.LogInformation($"🎯 Final Results: {successCount}/{results.Length} symbols completed successfully");

        if (successCount == results.Length)
        {
            _logger.LogInformation("🎉 All optimized parallel data collection completed successfully!");
        }
        else
        {
            _logger.LogInformation("⚠️  Some collections failed. Check logs for details.");
        }

        _logger.LogInformation("Optimized parallel data collection completed: {Success}/{Total} successful",
            successCount, results.Length);
    }

    /// <summary>
    /// Alternative method for custom parallel collection with specific configurations
    /// </summary>
    public async Task CollectCustomParallelDataAsync(Dictionary<string, Dictionary<Timeframe, int>> symbolTimeframeConfig)
    {
        _logger.LogInformation("🔧 Starting custom parallel data collection");
        _logger.LogInformation("🔧 Starting custom parallel data collection...");

        var allTasks = new List<Task<CollectionResult>>();

        foreach (var symbolConfig in symbolTimeframeConfig)
        {
            var symbol = symbolConfig.Key;
            var timeframeConfigs = symbolConfig.Value;

            foreach (var timeframeConfig in timeframeConfigs)
            {
                var timeframe = timeframeConfig.Key;
                var daysBack = timeframeConfig.Value;

                var task = CollectDataWithResultAsync(symbol, timeframe, daysBack);
                allTasks.Add(task);
            }
        }

        var results = await Task.WhenAll(allTasks);

        // Group results by symbol for reporting
        var groupedResults = results.GroupBy(r => r.Symbol);

        _logger.LogInformation("");
        _logger.LogInformation("📊 Custom Collection Results:");
        _logger.LogInformation(new string('-', 40));

        foreach (var symbolGroup in groupedResults)
        {
            var symbolResults = symbolGroup.ToList();
            var successCount = symbolResults.Count(r => r.Success);
            var totalCount = symbolResults.Count;

            _logger.LogInformation($"  {symbolGroup.Key}: {successCount}/{totalCount} timeframes successful");

            foreach (var result in symbolResults.Where(r => !r.Success))
            {
                _logger.LogInformation($"    ❌ {result.Details}: {result.Error}");
            }
        }

        var overallSuccess = results.Count(r => r.Success);
        _logger.LogInformation("");
        _logger.LogInformation($"🎯 Overall Results: {overallSuccess}/{results.Length} collections successful");
    }

    /// <summary>
    /// Helper method to collect data and return result
    /// </summary>
    private async Task<CollectionResult> CollectDataWithResultAsync(string symbol, Timeframe timeframe, int daysBack)
    {
        try
        {
            await CollectHistoricalDataAsync(symbol, [timeframe], daysBack);
            return new CollectionResult
            {
                Symbol = symbol,
                Success = true,
                Details = $"{timeframe} ({daysBack} days)"
            };
        }
        catch (Exception ex)
        {
            return new CollectionResult
            {
                Symbol = symbol,
                Success = false,
                Error = ex.Message,
                Details = $"{timeframe} ({daysBack} days)"
            };
        }
    }

    /// <summary>
    /// Checks data availability and fills gaps if needed
    /// </summary>
    public async Task CheckAndFillGapsAsync(string symbol, Timeframe timeframe, int maxDaysBack = 7)
    {
        _logger.LogInformation("Checking for data gaps in {Symbol} {Timeframe} (last {Days} days)", symbol, timeframe, maxDaysBack);

        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-maxDaysBack);

            // Check for gaps using your market data service
            var gaps = await _marketDataService.GetDataGapsAsync(Exchange.Kraken, symbol, timeframe, startDate, endDate);
            var gapList = gaps.ToList();

            if (!gapList.Any())
            {
                _logger.LogInformation("✅ No gaps found in {Symbol} {Timeframe} data", symbol, timeframe);
                return;
            }

            _logger.LogWarning("Found {GapCount} gaps in {Symbol} {Timeframe} data", gapList.Count, symbol, timeframe);

            // Calculate total missing intervals
            var totalMissingIntervals = gapList.Sum(g => g.MissingDataPoints);

            _logger.LogInformation("Attempting to fill gaps by collecting {Intervals} intervals", totalMissingIntervals + 10);

            // Collect recent data to fill gaps
            await CollectRecentDataAsync(symbol, timeframe);

            _logger.LogInformation("✅ Gap filling attempt completed for {Symbol} {Timeframe}", symbol, timeframe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check/fill gaps for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    /// <summary>
    /// Gets current data status for a symbol
    /// </summary>
    public async Task<string> GetDataStatusAsync(string symbol, Timeframe timeframe)
    {
        try
        {
            var latestData = await _marketDataService.GetLatestMarketDataAsync(Exchange.Kraken, symbol, timeframe);

            if (latestData == null)
            {
                return $"❌ No data found for {symbol} {timeframe}";
            }

            var timeSinceLastData = DateTime.UtcNow - latestData.Timestamp;
            var status = timeSinceLastData.TotalHours switch
            {
                < 1 => "🟢 Current",
                < 24 => "🟡 Recent",
                < 168 => "🟠 Stale",
                _ => "🔴 Very Old"
            };

            return $"{status} - Latest: {latestData.Timestamp:yyyy-MM-dd HH:mm} UTC ({timeSinceLastData.TotalHours:F1}h ago)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data status for {Symbol} {Timeframe}", symbol, timeframe);
            return $"❌ Error checking status for {symbol} {timeframe}";
        }
    }

    /// <summary>
    /// Runs a comprehensive data collection routine
    /// </summary>
    public async Task RunFullCollectionAsync()
    {
        _logger.LogInformation("🚀 Starting comprehensive data collection routine");

        var symbolConfigs = new List<SymbolConfig>
        {
            new() { Symbol = "BTCUSDT", Timeframes = [Timeframe.OneMinute, Timeframe.FiveMinutes, Timeframe.OneHour, Timeframe.OneDay] },
            new() { Symbol = "ETHUSDT", Timeframes = [Timeframe.OneMinute, Timeframe.FiveMinutes, Timeframe.OneHour, Timeframe.OneDay] },
            new() { Symbol = "ADAUSDT", Timeframes = [Timeframe.OneHour, Timeframe.OneDay] },
            new() { Symbol = "DOTUSDT", Timeframes = [Timeframe.OneHour, Timeframe.OneDay] }
        };

        // Collect recent data for all symbols
        await CollectDataBulkAsync(symbolConfigs);

        _logger.LogInformation("✅ Comprehensive data collection routine completed");
    }

    #region Helper Methods

    private static int CalculateIntervalsForDays(Timeframe timeframe, int days)
    {
        var minutesPerDay = 1440;
        var totalMinutes = days * minutesPerDay;
        var intervalMinutes = (int)timeframe;

        return totalMinutes / intervalMinutes;
    }

    #endregion
}

/// <summary>
/// Supporting configuration class
/// </summary>
public class SymbolConfig
{
    public string Symbol { get; set; } = string.Empty;
    public List<Timeframe> Timeframes { get; set; } = [];
}

/// <summary>
/// Result class for tracking collection outcomes
/// </summary>
public class CollectionResult
{
    public string Symbol { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Details { get; set; }
}