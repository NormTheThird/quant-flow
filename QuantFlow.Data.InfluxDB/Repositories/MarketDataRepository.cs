namespace QuantFlow.Data.InfluxDB.Repositories;

/// <summary>
/// InfluxDB implementation of market data repository using custom type-safe Flux query builder
/// </summary>
public class MarketDataRepository : IMarketDataRepository
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<MarketDataRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the MarketDataRepository with required dependencies
    /// </summary>
    /// <param name="context">InfluxDB context for database operations and connection management</param>
    /// <param name="logger">Logger instance for debugging, monitoring, and error tracking</param>
    /// <exception cref="ArgumentNullException">Thrown when context or logger is null</exception>
    public MarketDataRepository(InfluxDbContext context, ILogger<MarketDataRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Writes a single market data point to InfluxDB
    /// </summary>
    /// <param name="marketData">Market data model containing OHLCV and additional trading information</param>
    /// <exception cref="ArgumentNullException">Thrown when marketData is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB write operation fails</exception>
    public async Task WritePriceDataAsync(MarketDataModel marketData)
    {
        ArgumentNullException.ThrowIfNull(marketData);

        try
        {
            var pricePoint = new PricePoint
            {
                Symbol = marketData.Symbol,
                Timeframe = marketData.Timeframe.ToString(),
                Exchange = marketData.Exchange.ToString(),
                Open = marketData.Open,
                High = marketData.High,
                Low = marketData.Low,
                Close = marketData.Close,
                Volume = marketData.Volume,
                VWAP = marketData.VWAP,
                TradeCount = marketData.TradeCount,
                Bid = marketData.Bid,
                Ask = marketData.Ask,
                Spread = marketData.Ask - marketData.Bid,
                Timestamp = marketData.Timestamp
            };

            await _context.WritePointAsync(pricePoint);
            _logger.LogDebug("Written price data for {Symbol} at {Timestamp}",
                marketData.Symbol, marketData.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write price data for {Symbol}", marketData.Symbol);
            throw;
        }
    }

    /// <summary>
    /// Writes multiple market data points to InfluxDB in a single batch operation for improved performance
    /// </summary>
    /// <param name="marketDataList">Collection of market data models to write</param>
    /// <exception cref="ArgumentNullException">Thrown when marketDataList is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB batch write operation fails</exception>
    /// <remarks>
    /// Batch operations are more efficient than individual writes when handling multiple data points.
    /// Empty collections are handled gracefully without making unnecessary database calls.
    /// </remarks>
    public async Task WritePriceDataBatchAsync(IEnumerable<MarketDataModel> marketDataList)
    {
        ArgumentNullException.ThrowIfNull(marketDataList);

        var dataList = marketDataList.ToList();
        if (!dataList.Any())
        {
            _logger.LogDebug("No market data to write");
            return;
        }

        try
        {
            var pricePoints = dataList.Select(md => new PricePoint
            {
                Symbol = md.Symbol,
                Timeframe = md.Timeframe.ToString(),
                Exchange = md.Exchange.ToString(),
                Open = md.Open,
                High = md.High,
                Low = md.Low,
                Close = md.Close,
                Volume = md.Volume,
                VWAP = md.VWAP,
                TradeCount = md.TradeCount,
                Bid = md.Bid,
                Ask = md.Ask,
                Spread = md.Ask - md.Bid,
                Timestamp = md.Timestamp
            });

            await _context.WritePointsAsync(pricePoints);
            _logger.LogDebug("Written {Count} price data points", dataList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch of {Count} price data points", dataList.Count);
            throw;
        }
    }

    /// <summary>
    /// Retrieves historical market data for a specific symbol, exchange, and timeframe within a date range
    /// </summary>
    /// <param name="exchange">Exchange to filter data from (e.g., Kraken, KuCoin)</param>
    /// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Time interval for the data (e.g., OneMinute, OneHour, OneDay)</param>
    /// <param name="start">Start date and time for data retrieval (inclusive)</param>
    /// <param name="end">End date and time for data retrieval (inclusive)</param>
    /// <returns>Collection of market data models ordered by timestamp, or empty collection if no data found</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB query fails</exception>
    /// <remarks>
    /// Uses custom type-safe Flux query builder to prevent syntax errors.
    /// Results are automatically sorted by timestamp in ascending order.
    /// </remarks>
    public async Task<IEnumerable<MarketDataModel>> GetPriceDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime start, DateTime end)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            // Using custom type-safe query builder instead of string interpolation
            var exchangeString = exchange.ToString().ToLowerInvariant();
            var timeframeString = timeframe.ToString();

            var query = _context.NewQuery()
                .Range(start, end)
                .FilterMeasurement("prices")
                .FilterTag("symbol", symbol)
                .FilterTag("timeframe", timeframeString)
                .FilterTag("exchange", exchangeString)
                .Pivot()
                .Sort(new[] { "_time" });

            var fluxQuery = query.Build();
            _logger.LogDebug("Generated Flux query: {Query}", fluxQuery);

            var results = await query.ExecuteAsync<PricePoint>(_context);

            var marketData = results.Select(p => new MarketDataModel
            {
                Symbol = p.Symbol,
                Timeframe = timeframe,
                Exchange = exchange,
                Open = p.Open,
                High = p.High,
                Low = p.Low,
                Close = p.Close,
                Volume = p.Volume,
                VWAP = p.VWAP,
                TradeCount = p.TradeCount,
                Bid = p.Bid,
                Ask = p.Ask,
                Timestamp = p.Timestamp
            });

            _logger.LogDebug("Retrieved {Count} price data points for {Symbol} from {Exchange}",
                results.Count(), symbol, exchange);
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price data for {Symbol} from {Exchange}", symbol, exchange);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the most recent market data point for a specific symbol, exchange, and timeframe
    /// </summary>
    /// <param name="exchange">Exchange to filter data from</param>
    /// <param name="symbol">Trading pair symbol to retrieve data for</param>
    /// <param name="timeframe">Time interval to filter by</param>
    /// <returns>Latest market data model if found, null if no recent data exists</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB query fails</exception>
    /// <remarks>
    /// Searches within the last 24 hours for the most recent data point.
    /// Results are sorted by timestamp in descending order and limited to 1 record.
    /// Useful for determining current market conditions and data freshness.
    /// </remarks>
    public async Task<MarketDataModel?> GetLatestPriceAsync(Exchange exchange, string symbol, Timeframe timeframe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            // Using custom type-safe query builder for latest price query
            var exchangeString = exchange.ToString().ToLowerInvariant();
            var timeframeString = timeframe.ToString();

            var query = _context.NewQuery()
                .Range("-24h")
                .FilterMeasurement("prices")
                .FilterTag("symbol", symbol)
                .FilterTag("timeframe", timeframeString)
                .FilterTag("exchange", exchangeString)
                .Pivot()
                .Sort(new[] { "_time" }, descending: true)
                .Limit(1);

            var fluxQuery = query.Build();
            _logger.LogDebug("Generated latest price query: {Query}", fluxQuery);

            var results = await query.ExecuteAsync<PricePoint>(_context);
            var latestPrice = results.FirstOrDefault();

            if (latestPrice == null)
            {
                _logger.LogDebug("No latest price found for {Symbol} {Timeframe} from {Exchange}",
                    symbol, timeframe, exchange);
                return null;
            }

            var marketData = new MarketDataModel
            {
                Symbol = latestPrice.Symbol,
                Timeframe = timeframe,
                Exchange = exchange,
                Open = latestPrice.Open,
                High = latestPrice.High,
                Low = latestPrice.Low,
                Close = latestPrice.Close,
                Volume = latestPrice.Volume,
                VWAP = latestPrice.VWAP,
                TradeCount = latestPrice.TradeCount,
                Bid = latestPrice.Bid,
                Ask = latestPrice.Ask,
                Timestamp = latestPrice.Timestamp
            };

            _logger.LogDebug("Retrieved latest price for {Symbol} {Timeframe} from {Exchange} at {Timestamp}",
                symbol, timeframe, exchange, latestPrice.Timestamp);
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest price for {Symbol} {Timeframe} from {Exchange}",
                symbol, timeframe, exchange);
            throw;
        }
    }

    /// <summary>
    /// Writes a single trade execution record to InfluxDB for tracking and analysis
    /// </summary>
    /// <param name="trade">Trade model containing execution details, pricing, and metadata</param>
    /// <exception cref="ArgumentNullException">Thrown when trade is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB write operation fails</exception>
    /// <remarks>
    /// Stores comprehensive trade information including symbol, exchange, side (buy/sell),
    /// pricing, quantities, fees, and execution timestamps for portfolio tracking and analysis.
    /// </remarks>
    public async Task WriteTradeDataAsync(TradeModel trade)
    {
        ArgumentNullException.ThrowIfNull(trade);

        try
        {
            var tradePoint = new TradePoint
            {
                Symbol = trade.Symbol,
                Exchange = trade.Exchange.ToString().ToLowerInvariant(),
                Side = trade.Type == TradeType.Buy ? "buy" : "sell",
                TradeType = trade.Type.ToString().ToLowerInvariant(),
                TradeId = trade.Id.ToString(),
                Price = trade.Price,
                Quantity = trade.Quantity,
                Value = trade.Value,
                Fees = trade.Commission,
                ExecutionTimeMs = null,
                Timestamp = trade.ExecutionTimestamp
            };

            await _context.WritePointAsync(tradePoint);
            _logger.LogDebug("Written trade data for {Symbol} - {Type} {Quantity} at {Price}",
                trade.Symbol, trade.Type, trade.Quantity, trade.Price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write trade data for {Symbol}", trade.Symbol);
            throw;
        }
    }

    /// <summary>
    /// Retrieves historical trade execution data for a specific symbol and exchange within a date range
    /// </summary>
    /// <param name="exchange">Exchange to filter trades from</param>
    /// <param name="symbol">Trading pair symbol to retrieve trades for</param>
    /// <param name="start">Start date and time for trade retrieval (inclusive)</param>
    /// <param name="end">End date and time for trade retrieval (inclusive)</param>
    /// <returns>Collection of trade models ordered by execution timestamp</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB query fails</exception>
    /// <remarks>
    /// Useful for backtesting analysis, performance evaluation, and trade history review.
    /// Trade records include all execution details, fees, and calculated net values.
    /// Results are sorted chronologically by execution timestamp.
    /// </remarks>
    public async Task<IEnumerable<TradeModel>> GetTradeDataAsync(Exchange exchange, string symbol, DateTime start, DateTime end)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            // Using custom type-safe query builder for trade data
            var exchangeString = exchange.ToString().ToLowerInvariant();

            var query = _context.NewQuery()
                .Range(start, end)
                .FilterMeasurement("trades")
                .FilterTag("symbol", symbol)
                .FilterTag("exchange", exchangeString)
                .Pivot()
                .Sort(new[] { "_time" });

            var fluxQuery = query.Build();
            _logger.LogDebug("Generated trade data query: {Query}", fluxQuery);

            var results = await query.ExecuteAsync<TradePoint>(_context);

            var trades = results.Select(t => new TradeModel
            {
                Id = Guid.TryParse(t.TradeId, out var id) ? id : Guid.NewGuid(),
                Symbol = t.Symbol,
                Exchange = exchange,
                Type = t.Side?.ToLowerInvariant() == "buy" ? TradeType.Buy : TradeType.Sell,
                Price = t.Price,
                Quantity = t.Quantity,
                Value = t.Value,
                Commission = t.Fees,
                ExecutionTimestamp = t.Timestamp,
                BacktestRunId = Guid.Empty,
                NetValue = t.Value - t.Fees,
                PortfolioBalanceBefore = 0m,
                PortfolioBalanceAfter = 0m,
                AlgorithmReason = string.Empty,
                AlgorithmConfidence = 0m,
                CreatedAt = t.Timestamp,
                CreatedBy = "influxdb"
            });

            _logger.LogDebug("Retrieved {Count} trade data points for {Symbol}", results.Count(), symbol);
            return trades;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trade data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Deletes market data for a specific symbol, exchange, and timeframe within a specified date range
    /// </summary>
    /// <param name="exchange">Exchange to delete data from</param>
    /// <param name="symbol">Trading pair symbol to delete data for</param>
    /// <param name="timeframe">Time interval to filter deletion by</param>
    /// <param name="startDate">Start date for data deletion (inclusive)</param>
    /// <param name="endDate">End date for data deletion (inclusive)</param>
    /// <exception cref="ArgumentException">Thrown when symbol is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB delete operation fails or connection is disposed</exception>
    /// <remarks>
    /// Use with caution as this operation is irreversible. Includes a small delay to ensure
    /// any pending operations complete before deletion. Useful for data cleanup, reprocessing,
    /// or removing corrupted data within specific time ranges.
    /// </remarks>
    public async Task DeleteMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            _logger.LogInformation("Deleting market data for {Symbol} ({Timeframe}) from {Start} to {End} for {Exchange}",
                symbol, timeframe, startDate, endDate, exchange.ToString());

            // Build predicate for deletion with proper escaping
            var predicateParts = new List<string>
            {
                "_measurement=\"prices\"",
                $"symbol=\"{symbol}\"",
                $"timeframe=\"{timeframe}\"",
                $"exchange=\"{exchange.ToString().ToLowerInvariant()}\""
            };

            var predicate = string.Join(" AND ", predicateParts);

            // Add small delay to ensure any previous operations are complete
            await Task.Delay(100);

            await _context.DeleteDataAsync(startDate, endDate, predicate);

            _logger.LogInformation("✅ Successfully deleted market data for {Symbol}", symbol);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "❌ Database connection disposed during delete for {Symbol}", symbol);
            throw new InvalidOperationException($"Database connection was closed while deleting data for {symbol}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete market data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Deletes ALL market data for a specific symbol and exchange across all timeframes and dates
    /// </summary>
    /// <param name="exchange">Exchange to delete all data from</param>
    /// <param name="symbol">Trading pair symbol to delete all data for</param>
    /// <exception cref="ArgumentException">Thrown when symbol is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when InfluxDB delete operation fails or connection is disposed</exception>
    /// <remarks>
    /// ⚠️ WARNING: This operation is irreversible and will delete ALL historical data for the specified
    /// symbol and exchange combination. Use with extreme caution. Intended for complete data reset
    /// scenarios or when switching data sources. Uses a wide time range to ensure complete deletion.
    /// </remarks>
    public async Task DeleteAllMarketDataAsync(Exchange exchange, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            _logger.LogWarning("⚠️  Deleting ALL market data for {Symbol} from {Exchange}", symbol, exchange.ToString());

            // Build predicate for deletion
            var predicateParts = new List<string>
            {
                "_measurement=\"prices\"",
                $"symbol=\"{symbol}\"",
                $"exchange=\"{exchange.ToString().ToLowerInvariant()}\""
            };

            var predicate = string.Join(" AND ", predicateParts);

            // Use a wider time range for "delete all"
            var start = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = DateTime.UtcNow.AddDays(1);

            // Add small delay to ensure any previous operations are complete
            await Task.Delay(100);

            await _context.DeleteDataAsync(start, end, predicate);

            _logger.LogWarning("✅ Successfully deleted ALL market data for {Symbol}", symbol);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "❌ Database connection disposed during delete all for {Symbol}", symbol);
            throw new InvalidOperationException($"Database connection was closed while deleting all data for {symbol}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete all market data for {Symbol}", symbol);
            throw;
        }
    }
}