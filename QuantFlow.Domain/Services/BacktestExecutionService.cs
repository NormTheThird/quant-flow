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

    public BacktestExecutionService(ILogger<BacktestExecutionService> logger, IBacktestRunRepository backtestRunRepository, IMarketDataService marketDataService,
                                    IAlgorithmService algorithmService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _backtestRunRepository = backtestRunRepository ?? throw new ArgumentNullException(nameof(backtestRunRepository));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
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
        _logger.LogInformation("Running backtest simulation: {BacktestRunId}", backtestRun.Id);

        await Task.Delay(1000); // Simulate processing time
        // TODO: Implement algorithm execution logic
        // For now, return dummy results
        return new BacktestResults
        {
            FinalBalance = backtestRun.InitialBalance,
            TotalReturnPercent = 0,
            MaxDrawdownPercent = 0,
            TotalTrades = 0,
            WinningTrades = 0,
            LosingTrades = 0,
            WinRatePercent = 0
        };
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