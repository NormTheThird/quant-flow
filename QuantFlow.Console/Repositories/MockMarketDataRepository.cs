namespace QuantFlow.Console.Repositories;

/// <summary>
/// Temporary mock repository for demonstration purposes
/// Remove this when the actual InfluxDB repository is available
/// </summary>
public class MockMarketDataRepository : IMarketDataRepository
{
    private readonly ILogger<MockMarketDataRepository> _logger;

    public MockMarketDataRepository(ILogger<MockMarketDataRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task WritePriceDataAsync(MarketDataModel marketData)
    {
        _logger.LogInformation("Mock: Writing price data for {Symbol}", marketData.Symbol);
        return Task.CompletedTask;
    }

    public Task WritePriceDataBatchAsync(IEnumerable<MarketDataModel> marketDataList)
    {
        var count = marketDataList.Count();
        _logger.LogInformation("Mock: Writing {Count} price data points", count);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<MarketDataModel>> GetPriceDataAsync(string symbol, string timeframe, DateTime start, DateTime end, string? exchange = null)
    {
        _logger.LogInformation("Mock: Getting price data for {Symbol} ({Timeframe}) from {Start} to {End}",
            symbol, timeframe, start, end);

        // Generate mock data for demonstration
        var mockData = GenerateMockData(symbol, timeframe, start, end, exchange);
        return Task.FromResult(mockData);
    }

    public Task<MarketDataModel?> GetLatestPriceAsync(string symbol, string? exchange = null)
    {
        _logger.LogInformation("Mock: Getting latest price for {Symbol}", symbol);

        var mockData = new MarketDataModel
        {
            Symbol = symbol,
            Exchange = exchange ?? "mock-exchange",
            Timeframe = "1h",
            DataSource = "mock",
            Open = 45000m,
            High = 45500m,
            Low = 44800m,
            Close = 45200m,
            Volume = 123.45m,
            VWAP = 45150m,
            TradeCount = 856,
            Bid = 45190m,
            Ask = 45210m,
            QuoteVolume = 5678901.23m,
            Timestamp = DateTime.UtcNow.AddMinutes(-5)
        };

        return Task.FromResult<MarketDataModel?>(mockData);
    }

    public Task WriteTradeDataAsync(TradeModel trade)
    {
        _logger.LogInformation("Mock: Writing trade data");
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TradeModel>> GetTradeDataAsync(string symbol, DateTime start, DateTime end, string? exchange = null)
    {
        _logger.LogInformation("Mock: Getting trade data for {Symbol}", symbol);
        return Task.FromResult(Enumerable.Empty<TradeModel>());
    }



    /// <summary>
    /// Generates mock market data for demonstration
    /// </summary>
    private static IEnumerable<MarketDataModel> GenerateMockData(string symbol, string timeframe, DateTime start, DateTime end, string? exchange)
    {
        var random = new Random();
        var current = start;
        var price = 45000m; // Starting price
        var data = new List<MarketDataModel>();

        // Determine interval based on timeframe
        var interval = timeframe.ToLowerInvariant() switch
        {
            "1m" => TimeSpan.FromMinutes(1),
            "5m" => TimeSpan.FromMinutes(5),
            "15m" => TimeSpan.FromMinutes(15),
            "30m" => TimeSpan.FromMinutes(30),
            "1h" => TimeSpan.FromHours(1),
            "4h" => TimeSpan.FromHours(4),
            "1d" => TimeSpan.FromDays(1),
            _ => TimeSpan.FromHours(1)
        };

        while (current <= end && data.Count < 1000) // Limit to 1000 points for demo
        {
            // Generate realistic price movement
            var change = (decimal)(random.NextDouble() - 0.5) * 100; // +/- $50 max change
            var open = price;
            var close = Math.Max(1m, price + change);
            var high = Math.Max(open, close) + (decimal)(random.NextDouble() * 50);
            var low = Math.Min(open, close) - (decimal)(random.NextDouble() * 50);
            var volume = (decimal)(random.NextDouble() * 100 + 10);

            data.Add(new MarketDataModel
            {
                Symbol = symbol,
                Exchange = exchange ?? "mock-exchange",
                Timeframe = timeframe,
                DataSource = "mock",
                Open = open,
                High = high,
                Low = Math.Max(1m, low),
                Close = close,
                Volume = volume,
                VWAP = (high + low + close) / 3,
                TradeCount = random.Next(100, 1000),
                Bid = close - 0.01m,
                Ask = close + 0.01m,
                QuoteVolume = volume * close,
                Timestamp = current
            });

            price = close;
            current = current.Add(interval);
        }

        return data;
    }
}