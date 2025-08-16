namespace QuantFlow.WorkerService.DataCollection.Services;

/// <summary>
/// Optimized market data collection service that runs immediately on startup
/// then continues once per hour with hierarchical data collection to minimize API calls and avoid rate limits
/// Ensures only completed/closed candles are collected
/// </summary>
public class MarketDataCollectionService : BackgroundService
{
    private readonly ILogger<MarketDataCollectionService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DataCollectionConfiguration> _config;
    private readonly IOptionsMonitor<CollectionScheduleConfiguration> _scheduleConfig;
    private Timer? _hourlyTimer;

    public MarketDataCollectionService(ILogger<MarketDataCollectionService> logger, IServiceProvider serviceProvider,
                                       IOptionsMonitor<DataCollectionConfiguration> config, IOptionsMonitor<CollectionScheduleConfiguration> scheduleConfig)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
        _scheduleConfig = scheduleConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Optimized Market Data Collection Service starting...");

        try
        {
            // **IMMEDIATE EXECUTION** - Run first collection cycle right away on startup
            _logger.LogInformation("⚡ Executing initial data collection on startup...");
            await ExecuteHourlyCollectionCycle(stoppingToken);
            _logger.LogInformation("✅ Startup collection completed successfully");

            // Calculate delay until next hour boundary for subsequent runs
            var nextHour = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour + 1);
            var delayUntilNextHour = nextHour - DateTime.UtcNow;

            _logger.LogInformation("⏰ Next scheduled collection at {NextHour} UTC (in {Delay})", nextHour, delayUntilNextHour);

            // Create timer for hourly execution starting from next hour boundary
            _hourlyTimer = new Timer(async _ =>
            {
                if (stoppingToken.IsCancellationRequested) return;

                try
                {
                    await ExecuteHourlyCollectionCycle(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Critical error in scheduled hourly collection cycle");
                }
            }, null, delayUntilNextHour, TimeSpan.FromHours(1));

            // Keep service running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 Market Data Collection Service stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "⚠️ Startup collection failed - attempting to continue with scheduled collections");

            // Still try to set up the hourly timer even if startup fails
            try
            {
                var nextHour = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour + 1);
                var delayUntilNextHour = nextHour - DateTime.UtcNow;

                _logger.LogInformation("⏰ Setting up scheduled collections despite startup failure at {NextHour} UTC (in {Delay})", nextHour, delayUntilNextHour);

                _hourlyTimer = new Timer(async _ =>
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    try
                    {
                        await ExecuteHourlyCollectionCycle(stoppingToken);
                    }
                    catch (Exception timerEx)
                    {
                        _logger.LogError(timerEx, "❌ Critical error in scheduled hourly collection cycle");
                    }
                }, null, delayUntilNextHour, TimeSpan.FromHours(1));

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Market Data Collection Service stopping after startup failure...");
            }
        }
    }

    /// <summary>
    /// Executes the complete hourly data collection cycle
    /// Collections are done sequentially to avoid rate limits
    /// </summary>
    private async Task ExecuteHourlyCollectionCycle(CancellationToken cancellationToken)
    {
        var currentTime = DateTime.UtcNow;
        var currentHour = currentTime.Hour;
        var cycleName = $"Collection-{currentTime:yyyy-MM-dd-HH-mm}";

        _logger.LogInformation("📊 Starting {CycleName} at {Time} UTC (Hour: {Hour})", cycleName, currentTime, currentHour);

        var startTime = DateTime.UtcNow;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<IDataCollectionOrchestrator>();
            var config = _config.CurrentValue;
            var schedule = _scheduleConfig.CurrentValue;

            // Validate configuration before proceeding
            if (!ValidateConfiguration(config, schedule))
            {
                _logger.LogError("❌ Invalid configuration detected - skipping {CycleName}", cycleName);
                return;
            }

            // Sequential collection to optimize API usage and avoid rate limits
            await CollectHighFrequencyData(orchestrator, config, schedule, cancellationToken);
            await CollectMediumFrequencyData(orchestrator, config, schedule, cancellationToken);
            await CollectLowFrequencyData(orchestrator, config, schedule, currentHour, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("✅ Completed {CycleName} successfully in {Duration}", cycleName, duration);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "❌ Failed {CycleName} after {Duration}", cycleName, duration);
            throw; // Re-throw to be handled by caller
        }
    }

    /// <summary>
    /// Validates the configuration before executing collection cycle
    /// </summary>
    private bool ValidateConfiguration(DataCollectionConfiguration config, CollectionScheduleConfiguration schedule)
    {
        if (config?.Symbols == null || !config.Symbols.Any())
        {
            _logger.LogWarning("⚠️ No symbols configured for collection");
            return false;
        }

        if (config?.Exchanges == null || !config.Exchanges.Any())
        {
            _logger.LogWarning("⚠️ No exchanges configured for collection");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Collects high-frequency data (1m, 5m, 15m) every hour
    /// Uses safe end times to ensure only completed candles are collected
    /// </summary>
    private async Task CollectHighFrequencyData(IDataCollectionOrchestrator orchestrator, DataCollectionConfiguration config,
                                                CollectionScheduleConfiguration schedule, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        // 1-minute data: Ensure we only get completed 1-minute candles
        if (schedule.OneMinute.Enabled)
        {
            var safeEndTime = GetSafeEndTime("1m", bufferMinutes: 2);
            var startTime = safeEndTime.AddMinutes(-schedule.OneMinute.LookbackMinutes);

            _logger.LogInformation("📈 Collecting 1m data from {Start} to {End} (safe completed candles only)",
                startTime, safeEndTime);

            tasks.Add(orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { "1m" },
                startTime: startTime,
                endTime: safeEndTime,
                cancellationToken: cancellationToken));
        }

        // Add delay between calls to respect rate limits
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            tasks.Clear();
        }

        // 5-minute data: Ensure completed 5-minute candles
        if (schedule.FiveMinute.Enabled)
        {
            var safeEndTime = GetSafeEndTime("5m", bufferMinutes: 2);
            var startTime = safeEndTime.AddMinutes(-schedule.FiveMinute.LookbackMinutes);

            _logger.LogInformation("📈 Collecting 5m data from {Start} to {End} (safe completed candles only)",
                startTime, safeEndTime);

            tasks.Add(orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { "5m" },
                startTime: startTime,
                endTime: safeEndTime,
                cancellationToken: cancellationToken));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            tasks.Clear();
        }

        // 15-minute data: Ensure completed 15-minute candles
        if (schedule.FifteenMinute.Enabled)
        {
            var safeEndTime = GetSafeEndTime("15m", bufferMinutes: 2);
            var startTime = safeEndTime.AddMinutes(-schedule.FifteenMinute.LookbackMinutes);

            _logger.LogInformation("📈 Collecting 15m data from {Start} to {End} (safe completed candles only)",
                startTime, safeEndTime);

            tasks.Add(orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { "15m" },
                startTime: startTime,
                endTime: safeEndTime,
                cancellationToken: cancellationToken));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Collects 1-hour data every hour with safe candle boundaries
    /// </summary>
    private async Task CollectMediumFrequencyData(IDataCollectionOrchestrator orchestrator, DataCollectionConfiguration config,
                                                  CollectionScheduleConfiguration schedule, CancellationToken cancellationToken)
    {
        if (!schedule.OneHour.Enabled) return;

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Rate limit protection

        // 1-hour data: Ensure completed hourly candles
        var safeEndTime = GetSafeEndTime("1h", bufferMinutes: 5);
        var startTime = safeEndTime.AddMinutes(-schedule.OneHour.LookbackMinutes);

        _logger.LogInformation("📈 Collecting 1h data from {Start} to {End} (safe completed candles only)",
            startTime, safeEndTime);

        await orchestrator.CollectRecentDataAsync(
            symbols: config.Symbols,
            exchanges: config.Exchanges,
            timeframes: new[] { "1h" },
            startTime: startTime,
            endTime: safeEndTime,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Collects low-frequency data based on optimal schedule with safe candle boundaries:
    /// - 4h data: Every 4 hours (00:00, 04:00, 08:00, 12:00, 16:00, 20:00 UTC)
    /// - Daily data: Once per day at 00:00 UTC
    /// </summary>
    private async Task CollectLowFrequencyData(IDataCollectionOrchestrator orchestrator, DataCollectionConfiguration config,
                                               CollectionScheduleConfiguration schedule, int currentHour, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        // Collect 4-hour data every 4 hours at optimal times
        if (schedule.FourHour.Enabled && currentHour % 4 == 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Rate limit protection

            var safeEndTime = GetSafeEndTime("4h", bufferMinutes: 10);
            var startTime = safeEndTime.AddMinutes(-schedule.FourHour.LookbackMinutes);

            _logger.LogInformation("📈 Collecting 4h data from {Start} to {End} (scheduled 4-hour interval at {Hour}:00 UTC)",
                startTime, safeEndTime, currentHour);

            tasks.Add(orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { "4h" },
                startTime: startTime,
                endTime: safeEndTime,
                cancellationToken: cancellationToken));
        }

        // Collect daily data once per day at midnight UTC
        if (schedule.OneDay.Enabled && currentHour == 0)
        {
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            var safeEndTime = GetSafeEndTime("1d", bufferMinutes: 15);
            var startTime = safeEndTime.AddMinutes(-schedule.OneDay.LookbackMinutes);

            _logger.LogInformation("📈 Collecting 1d data from {Start} to {End} (daily close at midnight UTC)",
                startTime, safeEndTime);

            tasks.Add(orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { "1d" },
                startTime: startTime,
                endTime: safeEndTime,
                cancellationToken: cancellationToken));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Gets the safe end time for collecting completed candles only
    /// Ensures we only collect candles that have fully closed
    /// </summary>
    /// <param name="timeframe">The timeframe being collected (1m, 5m, 15m, etc.)</param>
    /// <param name="bufferMinutes">Safety buffer to ensure candle completion</param>
    /// <returns>Safe end time for collection</returns>
    private DateTime GetSafeEndTime(string timeframe, int bufferMinutes)
    {
        var now = DateTime.UtcNow;
        var timeframeMinutes = GetTimeframeMinutes(timeframe);

        // Calculate the last completed candle boundary
        var lastCandleBoundary = GetLastCandleBoundary(now, timeframeMinutes);

        // Add buffer to ensure candle is fully closed and processed by exchange
        var safeEndTime = lastCandleBoundary.AddMinutes(-bufferMinutes);

        _logger.LogDebug("🕒 Safe end time calculation: Now={Now}, Boundary={Boundary}, Buffer={Buffer}min, SafeEnd={SafeEnd}",
            now, lastCandleBoundary, bufferMinutes, safeEndTime);

        return safeEndTime;
    }

    /// <summary>
    /// Gets the boundary of the last completed candle
    /// </summary>
    private DateTime GetLastCandleBoundary(DateTime currentTime, int timeframeMinutes)
    {
        if (timeframeMinutes >= 1440) // Daily or larger
        {
            // For daily candles, use the start of current day
            return currentTime.Date;
        }
        else if (timeframeMinutes >= 60) // Hourly
        {
            // For hourly candles, use the start of current hour
            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0, DateTimeKind.Utc);
        }
        else // Minutes
        {
            // For minute-based candles
            var totalMinutes = (int)currentTime.TimeOfDay.TotalMinutes;
            var completedPeriods = totalMinutes / timeframeMinutes;
            var lastBoundaryMinutes = completedPeriods * timeframeMinutes;

            return currentTime.Date.AddMinutes(lastBoundaryMinutes);
        }
    }

    /// <summary>
    /// Converts timeframe string to minutes
    /// </summary>
    private int GetTimeframeMinutes(string timeframe) => timeframe.ToLowerInvariant() switch
    {
        "1m" => 1,
        "5m" => 5,
        "15m" => 15,
        "1h" => 60,
        "4h" => 240,
        "1d" => 1440,
        _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
    };

    public override void Dispose()
    {
        _logger.LogInformation("🧹 Disposing Market Data Collection Service");
        _hourlyTimer?.Dispose();
        base.Dispose();
    }
}