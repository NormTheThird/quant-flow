namespace QuantFlow.WorkerService.DataCollection.Services;

public class MarketDataCollectionService : BackgroundService
{
    private readonly ILogger<MarketDataCollectionService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DataCollectionConfiguration> _config;
    private readonly IOptionsMonitor<CollectionScheduleConfiguration> _scheduleConfig;
    private readonly List<Timer> _timers = new();

    public MarketDataCollectionService(
        ILogger<MarketDataCollectionService> logger,
        IServiceProvider serviceProvider,
        IOptionsMonitor<DataCollectionConfiguration> config,
        IOptionsMonitor<CollectionScheduleConfiguration> scheduleConfig)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
        _scheduleConfig = scheduleConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Market Data Collection Service starting...");

        await InitializeTimers(stoppingToken);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("🛑 Market Data Collection Service stopping...");
        DisposeTimers();
    }

    private async Task InitializeTimers(CancellationToken cancellationToken)
    {
        var schedule = _scheduleConfig.CurrentValue;

        // 1-minute data collection
        if (schedule.OneMinute.Enabled)
        {
            CreateTimer("1m", TimeSpan.FromMinutes(schedule.OneMinute.IntervalMinutes),
                schedule.OneMinute.LookbackMinutes, cancellationToken);
        }

        // 5-minute data collection  
        if (schedule.FiveMinute.Enabled)
        {
            CreateTimer("5m", TimeSpan.FromMinutes(schedule.FiveMinute.IntervalMinutes),
                schedule.FiveMinute.LookbackMinutes, cancellationToken);
        }

        // 15-minute data collection
        if (schedule.FifteenMinute.Enabled)
        {
            CreateTimer("15m", TimeSpan.FromMinutes(schedule.FifteenMinute.IntervalMinutes),
                schedule.FifteenMinute.LookbackMinutes, cancellationToken);
        }

        // 1-hour data collection
        if (schedule.OneHour.Enabled)
        {
            CreateTimer("1h", TimeSpan.FromMinutes(schedule.OneHour.IntervalMinutes),
                schedule.OneHour.LookbackMinutes, cancellationToken);
        }

        // 4-hour data collection
        if (schedule.FourHour.Enabled)
        {
            CreateTimer("4h", TimeSpan.FromMinutes(schedule.FourHour.IntervalMinutes),
                schedule.FourHour.LookbackMinutes, cancellationToken);
        }

        // Daily data collection
        if (schedule.OneDay.Enabled)
        {
            CreateTimer("1d", TimeSpan.FromMinutes(schedule.OneDay.IntervalMinutes),
                schedule.OneDay.LookbackMinutes, cancellationToken);
        }

        await Task.CompletedTask;
    }

    private void CreateTimer(string timeframe, TimeSpan interval, int lookbackMinutes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("⏰ Setting up {Timeframe} collection timer (every {Interval})",
            timeframe, interval);

        var timer = new Timer(async _ =>
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                await CollectDataForTimeframe(timeframe, lookbackMinutes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in {Timeframe} collection timer", timeframe);
            }
        }, null, TimeSpan.Zero, interval);

        _timers.Add(timer);
    }

    private async Task CollectDataForTimeframe(string timeframe, int lookbackMinutes, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IDataCollectionOrchestrator>();
        var config = _config.CurrentValue;

        _logger.LogInformation("📊 Starting {Timeframe} data collection for {SymbolCount} symbols",
            timeframe, config.Symbols.Count);

        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddMinutes(-lookbackMinutes);

        try
        {
            await orchestrator.CollectRecentDataAsync(
                symbols: config.Symbols,
                exchanges: config.Exchanges,
                timeframes: new[] { timeframe },
                startTime: startTime,
                endTime: endTime,
                cancellationToken: cancellationToken);

            _logger.LogInformation("✅ Completed {Timeframe} data collection", timeframe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed {Timeframe} data collection", timeframe);
        }
    }

    private void DisposeTimers()
    {
        foreach (var timer in _timers)
        {
            timer?.Dispose();
        }
        _timers.Clear();
    }

    public override void Dispose()
    {
        DisposeTimers();
        base.Dispose();
    }
}