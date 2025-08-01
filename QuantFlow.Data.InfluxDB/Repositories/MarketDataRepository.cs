﻿namespace QuantFlow.Data.InfluxDB.Repositories;

/// <summary>
/// InfluxDB implementation of market data repository
/// </summary>
public class MarketDataRepository : IMarketDataRepository
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<MarketDataRepository> _logger;

    public MarketDataRepository(InfluxDbContext context, ILogger<MarketDataRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task WritePriceDataAsync(MarketDataModel marketData)
    {
        ArgumentNullException.ThrowIfNull(marketData);

        try
        {
            var pricePoint = new PricePoint
            {
                Symbol = marketData.Symbol,
                Timeframe = marketData.Timeframe,
                DataSource = marketData.DataSource ?? "unknown",
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
                Timeframe = md.Timeframe,
                DataSource = md.DataSource ?? "unknown",
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

    public async Task<IEnumerable<MarketDataModel>> GetPriceDataAsync(string symbol, string timeframe, DateTime start, DateTime end, string? exchange = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);

        try
        {
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""prices"")
                |> filter(fn: (r) => r.symbol == ""{symbol}"")
                |> filter(fn: (r) => r.timeframe == ""{timeframe}"")
                {exchangeFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""])";

            var results = await _context.QueryAsync<PricePoint>(fluxQuery);

            var marketData = results.Select(p => new MarketDataModel
            {
                Symbol = p.Symbol,
                Timeframe = p.Timeframe,
                DataSource = p.DataSource,
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

            _logger.LogDebug("Retrieved {Count} price data points for {Symbol}", results.Count(), symbol);
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price data for {Symbol}", symbol);
            throw;
        }
    }

    public async Task<MarketDataModel?> GetLatestPriceAsync(string symbol, string? exchange = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: -1h)
                |> filter(fn: (r) => r._measurement == ""prices"")
                |> filter(fn: (r) => r.symbol == ""{symbol}"")
                {exchangeFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""], desc: true)
                |> limit(n: 1)";

            var results = await _context.QueryAsync<PricePoint>(fluxQuery);
            var latestPrice = results.FirstOrDefault();

            if (latestPrice == null)
            {
                _logger.LogDebug("No latest price found for {Symbol}", symbol);
                return null;
            }

            var marketData = new MarketDataModel
            {
                Symbol = latestPrice.Symbol,
                Timeframe = latestPrice.Timeframe,
                DataSource = latestPrice.DataSource,
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

            _logger.LogDebug("Retrieved latest price for {Symbol} at {Timestamp}", symbol, latestPrice.Timestamp);
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest price for {Symbol}", symbol);
            throw;
        }
    }

    public async Task WriteTradeDataAsync(TradeModel trade)
    {
        ArgumentNullException.ThrowIfNull(trade);

        try
        {
            var tradePoint = new TradePoint
            {
                Symbol = trade.Symbol,
                Exchange = "unknown",
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

    public async Task<IEnumerable<TradeModel>> GetTradeDataAsync(string symbol, DateTime start, DateTime end, string? exchange = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""trades"")
                |> filter(fn: (r) => r.symbol == ""{symbol}"")
                {exchangeFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""])";

            var results = await _context.QueryAsync<TradePoint>(fluxQuery);

            var trades = results.Select(t => new TradeModel
            {
                Id = Guid.TryParse(t.TradeId, out var id) ? id : Guid.NewGuid(),
                Symbol = t.Symbol,
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
    /// Deletes market data for a symbol within a time range
    /// </summary>
    public async Task DeleteMarketDataAsync(string symbol, string timeframe, DateTime startDate, DateTime endDate, string? dataSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);

        try
        {
            _logger.LogInformation("Deleting market data for {Symbol} ({Timeframe}) from {Start} to {End} for {DataSource}",
                symbol, timeframe, startDate, endDate, dataSource ?? "all sources");

            // Build predicate for deletion with proper escaping
            var predicateParts = new List<string>
        {
            "_measurement=\"prices\"",
            $"symbol=\"{symbol}\"",
            $"timeframe=\"{timeframe}\""
        };

            if (!string.IsNullOrEmpty(dataSource))
            {
                predicateParts.Add($"data_source=\"{dataSource}\"");
            }

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
    /// Deletes all market data for a symbol (use with caution)
    /// </summary>
    public async Task DeleteAllMarketDataAsync(string symbol, string? dataSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            _logger.LogWarning("⚠️  Deleting ALL market data for {Symbol} from {DataSource}",
                symbol, dataSource ?? "all sources");

            // Build predicate for deletion
            var predicateParts = new List<string>
        {
            "_measurement=\"prices\"",
            $"symbol=\"{symbol}\""
        };

            if (!string.IsNullOrEmpty(dataSource))
            {
                predicateParts.Add($"data_source=\"{dataSource}\"");
            }

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