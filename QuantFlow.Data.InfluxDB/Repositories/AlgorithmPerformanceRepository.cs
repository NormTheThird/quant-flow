namespace QuantFlow.Data.InfluxDB.Repositories;

/// <summary>
/// InfluxDB implementation of algorithm performance repository
/// </summary>
public class AlgorithmPerformanceRepository : IAlgorithmPerformanceRepository
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<AlgorithmPerformanceRepository> _logger;

    public AlgorithmPerformanceRepository(
        InfluxDbContext context,
        ILogger<AlgorithmPerformanceRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Writes algorithm performance metrics to InfluxDB
    /// </summary>
    public async Task WritePerformanceMetricsAsync(AlgorithmPerformanceModel performance)
    {
        ArgumentNullException.ThrowIfNull(performance);

        try
        {
            var performancePoint = new AlgorithmPerformancePoint
            {
                AlgorithmId = performance.AlgorithmId.ToString(),
                AlgorithmName = performance.AlgorithmName,
                Version = performance.Version,
                Environment = performance.Environment,
                Symbol = performance.Symbol,
                Timeframe = performance.Timeframe,
                PnL = performance.PnL,
                CumulativePnL = performance.CumulativePnL,
                ReturnPercentage = performance.ReturnPercentage,
                CumulativeReturn = performance.CumulativeReturn,
                Drawdown = performance.Drawdown,
                MaxDrawdown = performance.MaxDrawdown,
                SharpeRatio = performance.SharpeRatio,
                WinRate = performance.WinRate,
                ProfitFactor = performance.ProfitFactor,
                TradeCount = performance.TradeCount,
                WinningTrades = performance.WinningTrades,
                LosingTrades = performance.LosingTrades,
                AvgTradeDurationMinutes = performance.AvgTradeDurationMinutes,
                PortfolioValue = performance.PortfolioValue,
                PositionSize = performance.PositionSize,
                Leverage = performance.Leverage,
                Volatility = performance.Volatility,
                ExecutionTimeMs = performance.ExecutionTimeMs,
                Timestamp = performance.Timestamp
            };

            await _context.WritePointAsync(performancePoint);
            _logger.LogDebug("Written performance metrics for algorithm {AlgorithmName} at {Timestamp}",
                performance.AlgorithmName, performance.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write performance metrics for algorithm {AlgorithmName}",
                performance.AlgorithmName);
            throw;
        }
    }

    /// <summary>
    /// Writes multiple performance metrics to InfluxDB
    /// </summary>
    public async Task WritePerformanceMetricsBatchAsync(IEnumerable<AlgorithmPerformanceModel> performanceList)
    {
        ArgumentNullException.ThrowIfNull(performanceList);

        var dataList = performanceList.ToList();
        if (!dataList.Any())
        {
            _logger.LogDebug("No performance metrics to write");
            return;
        }

        try
        {
            var performancePoints = dataList.Select(p => new AlgorithmPerformancePoint
            {
                AlgorithmId = p.AlgorithmId.ToString(),
                AlgorithmName = p.AlgorithmName,
                Version = p.Version,
                Environment = p.Environment,
                Symbol = p.Symbol,
                Timeframe = p.Timeframe,
                PnL = p.PnL,
                CumulativePnL = p.CumulativePnL,
                ReturnPercentage = p.ReturnPercentage,
                CumulativeReturn = p.CumulativeReturn,
                Drawdown = p.Drawdown,
                MaxDrawdown = p.MaxDrawdown,
                SharpeRatio = p.SharpeRatio,
                WinRate = p.WinRate,
                ProfitFactor = p.ProfitFactor,
                TradeCount = p.TradeCount,
                WinningTrades = p.WinningTrades,
                LosingTrades = p.LosingTrades,
                AvgTradeDurationMinutes = p.AvgTradeDurationMinutes,
                PortfolioValue = p.PortfolioValue,
                PositionSize = p.PositionSize,
                Leverage = p.Leverage,
                Volatility = p.Volatility,
                ExecutionTimeMs = p.ExecutionTimeMs,
                Timestamp = p.Timestamp
            });

            await _context.WritePointsAsync(performancePoints);
            _logger.LogDebug("Written {Count} performance metrics", dataList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch of {Count} performance metrics", dataList.Count);
            throw;
        }
    }

    /// <summary>
    /// Gets performance metrics for an algorithm within a time range
    /// </summary>
    public async Task<IEnumerable<AlgorithmPerformanceModel>> GetPerformanceMetricsAsync(
        Guid algorithmId,
        DateTime start,
        DateTime end,
        string? symbol = null,
        string? environment = null)
    {
        try
        {
            var symbolFilter = !string.IsNullOrEmpty(symbol) ?
                $"and r.symbol == \"{symbol}\"" : "";
            var environmentFilter = !string.IsNullOrEmpty(environment) ?
                $"and r.environment == \"{environment}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""algorithm_performance"")
                |> filter(fn: (r) => r.algorithm_id == ""{algorithmId}"")
                {symbolFilter}
                {environmentFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""])";

            var results = await _context.QueryAsync<AlgorithmPerformancePoint>(fluxQuery);

            var performanceMetrics = results.Select(p => new AlgorithmPerformanceModel
            {
                AlgorithmId = Guid.TryParse(p.AlgorithmId, out var id) ? id : Guid.Empty,
                AlgorithmName = p.AlgorithmName,
                Version = p.Version,
                Environment = p.Environment,
                Symbol = p.Symbol,
                Timeframe = p.Timeframe,
                PnL = p.PnL,
                CumulativePnL = p.CumulativePnL,
                ReturnPercentage = p.ReturnPercentage,
                CumulativeReturn = p.CumulativeReturn,
                Drawdown = p.Drawdown,
                MaxDrawdown = p.MaxDrawdown,
                SharpeRatio = p.SharpeRatio,
                WinRate = p.WinRate,
                ProfitFactor = p.ProfitFactor,
                TradeCount = p.TradeCount,
                WinningTrades = p.WinningTrades,
                LosingTrades = p.LosingTrades,
                AvgTradeDurationMinutes = p.AvgTradeDurationMinutes,
                PortfolioValue = p.PortfolioValue,
                PositionSize = p.PositionSize,
                Leverage = p.Leverage,
                Volatility = p.Volatility,
                ExecutionTimeMs = p.ExecutionTimeMs,
                Timestamp = p.Timestamp
            });

            _logger.LogDebug("Retrieved {Count} performance metrics for algorithm {AlgorithmId}",
                results.Count, algorithmId);
            return performanceMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance metrics for algorithm {AlgorithmId}", algorithmId);
            throw;
        }
    }

    /// <summary>
    /// Gets latest performance metrics for an algorithm
    /// </summary>
    public async Task<AlgorithmPerformanceModel?> GetLatestPerformanceAsync(
        Guid algorithmId,
        string? symbol = null,
        string? environment = null)
    {
        try
        {
            var symbolFilter = !string.IsNullOrEmpty(symbol) ?
                $"and r.symbol == \"{symbol}\"" : "";
            var environmentFilter = !string.IsNullOrEmpty(environment) ?
                $"and r.environment == \"{environment}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: -1d)
                |> filter(fn: (r) => r._measurement == ""algorithm_performance"")
                |> filter(fn: (r) => r.algorithm_id == ""{algorithmId}"")
                {symbolFilter}
                {environmentFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""], desc: true)
                |> limit(n: 1)";

            var results = await _context.QueryAsync<AlgorithmPerformancePoint>(fluxQuery);
            var latestPerformance = results.FirstOrDefault();

            if (latestPerformance == null)
            {
                _logger.LogDebug("No latest performance found for algorithm {AlgorithmId}", algorithmId);
                return null;
            }

            var performanceModel = new AlgorithmPerformanceModel
            {
                AlgorithmId = Guid.TryParse(latestPerformance.AlgorithmId, out var id) ? id : Guid.Empty,
                AlgorithmName = latestPerformance.AlgorithmName,
                Version = latestPerformance.Version,
                Environment = latestPerformance.Environment,
                Symbol = latestPerformance.Symbol,
                Timeframe = latestPerformance.Timeframe,
                PnL = latestPerformance.PnL,
                CumulativePnL = latestPerformance.CumulativePnL,
                ReturnPercentage = latestPerformance.ReturnPercentage,
                CumulativeReturn = latestPerformance.CumulativeReturn,
                Drawdown = latestPerformance.Drawdown,
                MaxDrawdown = latestPerformance.MaxDrawdown,
                SharpeRatio = latestPerformance.SharpeRatio,
                WinRate = latestPerformance.WinRate,
                ProfitFactor = latestPerformance.ProfitFactor,
                TradeCount = latestPerformance.TradeCount,
                WinningTrades = latestPerformance.WinningTrades,
                LosingTrades = latestPerformance.LosingTrades,
                AvgTradeDurationMinutes = latestPerformance.AvgTradeDurationMinutes,
                PortfolioValue = latestPerformance.PortfolioValue,
                PositionSize = latestPerformance.PositionSize,
                Leverage = latestPerformance.Leverage,
                Volatility = latestPerformance.Volatility,
                ExecutionTimeMs = latestPerformance.ExecutionTimeMs,
                Timestamp = latestPerformance.Timestamp
            };

            _logger.LogDebug("Retrieved latest performance for algorithm {AlgorithmId} at {Timestamp}",
                algorithmId, latestPerformance.Timestamp);
            return performanceModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest performance for algorithm {AlgorithmId}", algorithmId);
            throw;
        }
    }

    /// <summary>
    /// Gets performance summary statistics for an algorithm
    /// </summary>
    public async Task<PerformanceSummaryModel?> GetPerformanceSummaryAsync(
        Guid algorithmId,
        DateTime start,
        DateTime end,
        string? symbol = null)
    {
        try
        {
            var symbolFilter = !string.IsNullOrEmpty(symbol) ?
                $"and r.symbol == \"{symbol}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""algorithm_performance"")
                |> filter(fn: (r) => r.algorithm_id == ""{algorithmId}"")
                {symbolFilter}
                |> filter(fn: (r) => r._field == ""cumulative_pnl"" or r._field == ""max_drawdown"" or r._field == ""sharpe_ratio"" or r._field == ""win_rate"")
                |> group(columns: [""_field""])
                |> aggregateWindow(every: 1d, fn: last)
                |> yield(name: ""summary"")";

            var results = await _context.QueryRawAsync(fluxQuery);

            if (!results.Any() || !results.First().Records.Any())
            {
                _logger.LogDebug("No performance summary found for algorithm {AlgorithmId}", algorithmId);
                return null;
            }

            // Process the results to create summary
            var summary = new PerformanceSummaryModel
            {
                AlgorithmId = algorithmId,
                StartDate = start,
                EndDate = end,
                Symbol = symbol ?? "ALL"
            };

            foreach (var table in results)
            {
                foreach (var record in table.Records)
                {
                    var field = record.GetValueByKey("_field")?.ToString();
                    var value = record.GetValue();

                    switch (field)
                    {
                        case "cumulative_pnl":
                            if (decimal.TryParse(value?.ToString(), out var pnl))
                                summary.TotalPnL = pnl;
                            break;
                        case "max_drawdown":
                            if (decimal.TryParse(value?.ToString(), out var dd))
                                summary.MaxDrawdown = dd;
                            break;
                        case "sharpe_ratio":
                            if (decimal.TryParse(value?.ToString(), out var sharpe))
                                summary.SharpeRatio = sharpe;
                            break;
                        case "win_rate":
                            if (decimal.TryParse(value?.ToString(), out var winRate))
                                summary.WinRate = winRate;
                            break;
                    }
                }
            }

            _logger.LogDebug("Retrieved performance summary for algorithm {AlgorithmId}", algorithmId);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance summary for algorithm {AlgorithmId}", algorithmId);
            throw;
        }
    }
}