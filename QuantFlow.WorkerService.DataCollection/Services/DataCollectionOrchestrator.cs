namespace QuantFlow.WorkerService.DataCollection.Services;

public class DataCollectionOrchestrator : IDataCollectionOrchestrator
{
    private readonly ILogger<DataCollectionOrchestrator> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly IKrakenMarketDataCollectionService _krakenMarketDataCollectionService;
    private readonly IOptionsMonitor<DataCollectionConfiguration> _config;

    public DataCollectionOrchestrator(ILogger<DataCollectionOrchestrator> logger, IMarketDataService marketDataService,
        IKrakenMarketDataCollectionService krakenMarketDataCollectionService, IOptionsMonitor<DataCollectionConfiguration> config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _krakenMarketDataCollectionService = krakenMarketDataCollectionService ?? throw new ArgumentNullException(nameof(krakenMarketDataCollectionService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task CollectRecentDataAsync(IEnumerable<string> symbols, IEnumerable<string> exchanges, IEnumerable<string> timeframes,
                                             DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var config = _config.CurrentValue;
        var semaphore = new SemaphoreSlim(config.MaxConcurrentCollections);
        var tasks = new List<Task>();

        foreach (var symbol in symbols)
        {
            foreach (var exchange in exchanges)
            {
                foreach (var timeframe in timeframes)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var task = CollectWithSemaphore(semaphore, symbol, exchange, timeframe,
                        startTime, endTime, cancellationToken);
                    tasks.Add(task);
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task CollectWithSemaphore(SemaphoreSlim semaphore, string symbol, string exchange, string timeframe,
                                            DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            await CollectDataForSymbol(symbol, exchange, timeframe, startTime, endTime, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task CollectDataForSymbol(string symbol, string exchange, string timeframe, DateTime startTime,
                                            DateTime endTime, CancellationToken cancellationToken)
    {
        var config = _config.CurrentValue;

        for (int attempt = 1; attempt <= config.RetryAttempts; attempt++)
        {
            try
            {
                _logger.LogDebug("📥 Collecting {Symbol} {Timeframe} data from {Exchange} (attempt {Attempt})",
                    symbol, timeframe, exchange, attempt);

                var marketData = await GetMarketDataFromExchange(symbol, exchange, timeframe, startTime, endTime);

                if (marketData.Any())
                {
                    var stored = await _marketDataService.StoreMarketDataAsync(marketData);
                    _logger.LogDebug("✅ Stored {Count} points for {Symbol} {Timeframe} from {Exchange}",
                        stored, symbol, timeframe, exchange);
                }
                else
                {
                    _logger.LogDebug("ℹ️  No new data for {Symbol} {Timeframe} from {Exchange}",
                        symbol, timeframe, exchange);
                }

                return; // Success!
            }
            catch (Exception ex) when (attempt < config.RetryAttempts)
            {
                _logger.LogWarning(ex, "⚠️  Attempt {Attempt} failed for {Symbol} {Timeframe} from {Exchange}, retrying...",
                    attempt, symbol, timeframe, exchange);

                await Task.Delay(TimeSpan.FromSeconds(config.RetryDelaySeconds * attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ All attempts failed for {Symbol} {Timeframe} from {Exchange}",
                    symbol, timeframe, exchange);
            }
        }
    }

    private async Task<IEnumerable<MarketDataModel>> GetMarketDataFromExchange(string symbol, string exchange, string timeframe,
                                                                               DateTime startTime, DateTime endTime)
    {
        return exchange.ToLowerInvariant() switch
        {
            "kraken" => await GetKrakenMarketData(symbol, timeframe, startTime, endTime),
            "kucoin" => await GetKucoinMarketData(symbol, timeframe, startTime, endTime),
            _ => throw new ArgumentException($"Unsupported exchange: {exchange}")
        };
    }

    private async Task<IEnumerable<MarketDataModel>> GetKrakenMarketData(string symbol, string timeframe,
                                                                         DateTime startTime, DateTime endTime)
    {
        // Delegate to the specialized Kraken service
        return await _krakenMarketDataCollectionService.GetMarketDataAsync(symbol, timeframe, startTime, endTime);
    }

    private async Task<IEnumerable<MarketDataModel>> GetKucoinMarketData(string symbol, string timeframe,
                                                                         DateTime startTime, DateTime endTime)
    {
        // TODO: Implement when KucoinMarketDataCollectionService is available
        // return await _kucoinMarketDataCollectionService.GetMarketDataAsync(symbol, timeframe, startTime, endTime);

        await Task.Delay(100); // Simulate async delay

        _logger.LogWarning("Kucoin data collection not yet implemented for {Symbol} {Timeframe}", symbol, timeframe);
        return Enumerable.Empty<MarketDataModel>();
    }
}