namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for executing backtests
/// </summary>
public class BacktestExecutionService : IBacktestExecutionService
{
    private readonly ILogger<BacktestExecutionService> _logger;
    private readonly IBacktestRunRepository _backtestRunRepository;
    private readonly IMarketDataService _marketDataService;
    private readonly IAlgorithmService _algorithmService;
    private readonly IAlgorithmExecutionService _algorithmExecutionService;
    private readonly ITradeService _tradeService;

    public BacktestExecutionService(ILogger<BacktestExecutionService> logger, IBacktestRunRepository backtestRunRepository, IMarketDataService marketDataService,
                                    IAlgorithmService algorithmService, IAlgorithmExecutionService algorithmExecutionService, ITradeService tradeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _backtestRunRepository = backtestRunRepository ?? throw new ArgumentNullException(nameof(backtestRunRepository));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _algorithmExecutionService = algorithmExecutionService ?? throw new ArgumentNullException(nameof(algorithmExecutionService));
        _tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
    }

    public async Task<BacktestRunModel> ExecuteBacktestAsync(Guid backtestRunId)
    {
        _logger.LogInformation("Starting backtest execution: {BacktestRunId}", backtestRunId);

        var backtestRun = await _backtestRunRepository.GetByIdAsync(backtestRunId);
        if (backtestRun == null)
            throw new NotFoundException($"Backtest run {backtestRunId} not found");

        var startTime = DateTime.UtcNow;

        try
        {
            // Update status to Running
            backtestRun.Status = BacktestStatus.Running;
            await _backtestRunRepository.UpdateAsync(backtestRun);

            // Load algorithm
            var algorithm = await _algorithmService.GetAlgorithmByIdAsync(backtestRun.AlgorithmId);
            if (algorithm == null)
                throw new NotFoundException($"Algorithm {backtestRun.AlgorithmId} not found");

            // Load market data
            var marketData = await LoadMarketDataAsync(backtestRun);

            // Execute backtest
            var results = await RunBacktestAsync(backtestRun, algorithm, marketData);

            // Update backtest run with results
            backtestRun.Status = BacktestStatus.Completed;
            backtestRun.FinalBalance = results.FinalBalance;
            backtestRun.TotalReturnPercent = results.TotalReturnPercent;
            backtestRun.MaxDrawdownPercent = results.MaxDrawdownPercent;
            backtestRun.TotalTrades = results.TotalTrades;
            backtestRun.WinningTrades = results.WinningTrades;
            backtestRun.LosingTrades = results.LosingTrades;
            backtestRun.WinRatePercent = results.WinRatePercent;
            backtestRun.CompletedAt = DateTime.UtcNow;
            backtestRun.ExecutionDuration = DateTime.UtcNow - startTime;

            await _backtestRunRepository.UpdateAsync(backtestRun);

            _logger.LogInformation("Backtest execution completed: {BacktestRunId}", backtestRunId);
            return backtestRun;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backtest execution failed: {BacktestRunId}", backtestRunId);

            backtestRun.Status = BacktestStatus.Failed;
            backtestRun.ErrorMessage = ex.Message;
            backtestRun.CompletedAt = DateTime.UtcNow;
            backtestRun.ExecutionDuration = DateTime.UtcNow - startTime;

            await _backtestRunRepository.UpdateAsync(backtestRun);

            throw;
        }
    }

    private async Task<List<MarketDataModel>> LoadMarketDataAsync(BacktestRunModel backtestRun)
    {
        _logger.LogInformation("Loading market data for backtest: {BacktestRunId}", backtestRun.Id);

        var timeframe = ConvertTimeframeToString(backtestRun.Timeframe);
        var exchange = backtestRun.Exchange.ToString().ToLower();

        var marketData = await _marketDataService.GetMarketDataAsync(backtestRun.Exchange, backtestRun.Symbol, backtestRun.Timeframe,
            backtestRun.BacktestStartDate, backtestRun.BacktestEndDate);

        var dataList = marketData.ToList();
        _logger.LogInformation("Loaded {Count} market data points", dataList.Count);

        return dataList;
    }

    private async Task<BacktestResults> RunBacktestAsync(BacktestRunModel backtestRun, AlgorithmModel algorithm, List<MarketDataModel> marketData)
    {
        _logger.LogInformation("Running backtest simulation: {BacktestRunId} with {DataPoints} data points",
            backtestRun.Id, marketData.Count);

        var results = new BacktestResults
        {
            FinalBalance = backtestRun.InitialBalance,
            TotalReturnPercent = 0,
            MaxDrawdownPercent = 0,
            TotalTrades = 0,
            WinningTrades = 0,
            LosingTrades = 0,
            WinRatePercent = 0
        };

        try
        {
            // Trading state
            var currentBalance = backtestRun.InitialBalance;
            var currentPosition = (PositionModel?)null;
            var trades = new List<TradeModel>();
            var peakEquity = currentBalance;
            var maxDrawdown = 0m;

            // Sort market data by timestamp
            var sortedData = marketData.OrderBy(_ => _.Timestamp).ToList();

            // Execute algorithm on each data point
            for (int i = 0; i < sortedData.Count; i++)
            {
                var currentBar = sortedData[i];

                // Get historical data up to current point for algorithm
                var historicalData = sortedData.Take(i + 1).ToList();

                // Update current position with latest price
                if (currentPosition != null)
                {
                    currentPosition.CurrentValue = currentPosition.Quantity * currentBar.Close;
                    currentPosition.UnrealizedPnL = currentPosition.CurrentValue - (currentPosition.Quantity * currentPosition.EntryPrice);
                }

                // Execute algorithm to get signal
                var signal = await _algorithmExecutionService.ExecuteAsync(algorithm, historicalData, currentBar, currentPosition);

                // Process signal
                if (signal == TradeSignal.Buy && currentPosition == null && currentBalance > 0)
                {
                    // Calculate position size (use all available balance for now)
                    var buyPrice = currentBar.Close;
                    var commission = buyPrice * backtestRun.CommissionRate;
                    var totalCost = buyPrice + commission;

                    if (totalCost <= currentBalance)
                    {
                        var quantity = currentBalance / totalCost;

                        // Create new position
                        currentPosition = new PositionModel
                        {
                            Quantity = quantity,
                            EntryPrice = buyPrice,
                            EntryTime = currentBar.Timestamp,
                            CurrentValue = quantity * buyPrice,
                            UnrealizedPnL = 0
                        };

                        currentBalance = 0;

                        trades.Add(new TradeModel
                        {
                            Id = Guid.NewGuid(),
                            BacktestRunId = backtestRun.Id,
                            Symbol = backtestRun.Symbol,
                            Exchange = backtestRun.Exchange,
                            Type = TradeType.Buy,
                            Price = buyPrice,
                            Quantity = quantity,
                            Value = quantity * buyPrice,
                            Commission = commission,
                            ExecutionTimestamp = currentBar.Timestamp,
                            AlgorithmReason = $"{algorithm.Name} - Buy Signal",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "BacktestEngine"
                        });

                        _logger.LogDebug("BUY: {Quantity} @ {Price} on {Date}", quantity, buyPrice, currentBar.Timestamp);
                    }
                }
                else if (signal == TradeSignal.Sell && currentPosition != null)
                {
                    // Sell entire position
                    var sellPrice = currentBar.Close;
                    var saleValue = currentPosition.Quantity * sellPrice;
                    var commission = saleValue * backtestRun.CommissionRate;
                    currentBalance = saleValue - commission;

                    trades.Add(new TradeModel
                    {
                        Id = Guid.NewGuid(),
                        BacktestRunId = backtestRun.Id,
                        Symbol = backtestRun.Symbol,
                        Exchange = backtestRun.Exchange,
                        Type = TradeType.Sell,
                        Price = sellPrice,
                        Quantity = currentPosition.Quantity,
                        Value = saleValue,
                        Commission = commission,
                        ExecutionTimestamp = currentBar.Timestamp,
                        AlgorithmReason = $"{algorithm.Name} - Sell Signal",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "BacktestEngine"
                    });

                    _logger.LogDebug("SELL: {Quantity} @ {Price} on {Date}", currentPosition.Quantity, sellPrice, currentBar.Timestamp);

                    currentPosition = null;
                }

                // Calculate current equity
                var currentEquity = currentBalance + (currentPosition?.CurrentValue ?? 0);

                // Track drawdown
                if (currentEquity > peakEquity)
                {
                    peakEquity = currentEquity;
                }
                else
                {
                    var drawdown = (peakEquity - currentEquity) / peakEquity * 100m;
                    if (drawdown > maxDrawdown)
                    {
                        maxDrawdown = drawdown;
                    }
                }
            }

            // Close any open position at final price
            if (currentPosition != null)
            {
                var finalBar = sortedData.Last();
                var finalPrice = finalBar.Close;
                var finalValue = currentPosition.Quantity * finalPrice;
                var commission = finalValue * backtestRun.CommissionRate;
                currentBalance = finalValue - commission;

                trades.Add(new TradeModel
                {
                    Id = Guid.NewGuid(),
                    BacktestRunId = backtestRun.Id,
                    Symbol = backtestRun.Symbol,
                    Exchange = backtestRun.Exchange,
                    Type = TradeType.Sell,
                    Price = finalPrice,
                    Quantity = currentPosition.Quantity,
                    Value = finalValue,
                    Commission = commission,
                    ExecutionTimestamp = finalBar.Timestamp,
                    AlgorithmReason = $"{algorithm.Name} - Final Close",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "BacktestEngine"
                });

                currentPosition = null;
            }

            // Calculate results
            results.FinalBalance = currentBalance;
            results.TotalReturnPercent = ((currentBalance - backtestRun.InitialBalance) / backtestRun.InitialBalance) * 100m;
            results.MaxDrawdownPercent = maxDrawdown;
            results.TotalTrades = trades.Count;

            // Calculate win/loss
            decimal? lastBuyPrice = null;
            foreach (var trade in trades)
            {
                if (trade.Type == TradeType.Buy)
                {
                    lastBuyPrice = trade.Price;
                }
                else if (trade.Type == TradeType.Sell && lastBuyPrice.HasValue)
                {
                    if (trade.Price > lastBuyPrice.Value)
                        results.WinningTrades++;
                    else
                        results.LosingTrades++;
                }
            }

            results.WinRatePercent = results.TotalTrades > 0 ? (results.WinningTrades / (decimal)results.TotalTrades) * 100m : 0;

            // Store trades
            foreach (var trade in trades)
            {
                await _tradeService.CreateTradeAsync(trade);
            }

            _logger.LogInformation("Backtest completed: Final Balance = {Balance}, Return = {Return}%, Trades = {Trades}",
                results.FinalBalance, results.TotalReturnPercent, results.TotalTrades);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running backtest simulation");
            throw;
        }
    }

    private string ConvertTimeframeToString(Timeframe timeframe) => timeframe switch
    {
        Timeframe.OneMinute => "1m",
        Timeframe.FiveMinutes => "5m",
        Timeframe.FifteenMinutes => "15m",
        Timeframe.OneHour => "1h",
        Timeframe.FourHours => "4h",
        Timeframe.OneDay => "1d",
        _ => "1h"
    };

    private class BacktestResults
    {
        public decimal FinalBalance { get; set; }
        public decimal TotalReturnPercent { get; set; }
        public decimal MaxDrawdownPercent { get; set; }
        public int TotalTrades { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }
        public decimal WinRatePercent { get; set; }
    }
}