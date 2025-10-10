namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture class providing test data for BacktestRunModel integration tests
/// </summary>
public static class BacktestRunModelFixture
{
    /// <summary>
    /// Creates a default BacktestRunModel with all properties populated for general testing
    /// </summary>
    public static BacktestRunModel CreateDefault(Guid? id = null, Guid? portfolioId = null, string name = "Test Run")
    {
        return new BacktestRunModel
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId ?? Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-10),
            BacktestEndDate = DateTime.UtcNow,
            Status = BacktestStatus.Completed,
            InitialBalance = 10000.0m,
            AlgorithmParameters = "{}",
            CommissionRate = 0.001m,
            FinalBalance = 10500.0m,
            TotalReturnPercent = 5.0m,
            MaxDrawdownPercent = 2.0m,
            SharpeRatio = 1.5m,
            TotalTrades = 20,
            WinningTrades = 14,
            LosingTrades = 6,
            WinRatePercent = 70.0m,
            AverageTradeReturnPercent = 0.25m,
            ExecutionDuration = TimeSpan.FromHours(1),
            ErrorMessage = "",
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test",
            User = UserModelFixture.CreateDefault(),
            //Portfolio = PortfolioModelFixture.CreateDefault(),
            //Trades = new List<TradeModel> { TradeModelFixture.CreateDefault() }
        };
    }

    /// <summary>
    /// Creates a standard completed backtest run model for testing
    /// </summary>
    public static BacktestRunModel CreateCompletedBacktestRun(Guid? userId = null, Guid? portfolioId = null, string name = "Integration Test Backtest")
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = BacktestStatus.Completed,
            InitialBalance = 10000.0m,
            FinalBalance = 12000.0m,
            TotalReturnPercent = 20.0m,
            MaxDrawdownPercent = -5.0m,
            SharpeRatio = 1.5m,
            TotalTrades = 10,
            WinningTrades = 7,
            LosingTrades = 3,
            WinRatePercent = 70.0m,
            AverageTradeReturnPercent = 2.0m,
            ExecutionDuration = TimeSpan.FromMinutes(5),
            AlgorithmParameters = "{\"rsi_period\": 14, \"ma_period\": 20}",
            CommissionRate = 0.001m,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a pending backtest run model for testing
    /// </summary>
    public static BacktestRunModel CreatePendingBacktestRun(Guid? userId = null, Guid? portfolioId = null, string name = "Pending Backtest")
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Symbol = "ETHUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.FifteenMinutes,
            BacktestStartDate = DateTime.UtcNow.AddDays(-60),
            BacktestEndDate = DateTime.UtcNow.AddDays(-30),
            Status = BacktestStatus.Pending,
            InitialBalance = 25000.0m,
            AlgorithmParameters = "{\"param1\": \"value1\", \"param2\": 42}",
            CommissionRate = 0.0015m,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a running backtest run model for testing
    /// </summary>
    public static BacktestRunModel CreateRunningBacktestRun(Guid? userId = null, Guid? portfolioId = null, string name = "Running Backtest")
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Symbol = "ETHUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = BacktestStatus.Running,
            InitialBalance = 10000.0m,
            AlgorithmParameters = "{\"strategy\": \"momentum\", \"lookback\": 10}",
            CommissionRate = 0.001m,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a failed backtest run model for testing
    /// </summary>
    public static BacktestRunModel CreateFailedBacktestRun(Guid? userId = null, Guid? portfolioId = null, string name = "Failed Backtest")
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = BacktestStatus.Failed,
            InitialBalance = 10000.0m,
            FinalBalance = 8000.0m,
            TotalReturnPercent = -20.0m,
            MaxDrawdownPercent = -15.0m,
            SharpeRatio = -0.5m,
            TotalTrades = 5,
            WinningTrades = 1,
            LosingTrades = 4,
            WinRatePercent = 20.0m,
            AverageTradeReturnPercent = -4.0m,
            ExecutionDuration = TimeSpan.FromMinutes(3),
            ErrorMessage = "Algorithm execution failed",
            AlgorithmParameters = "{\"strategy\": \"mean_reversion\"}",
            CommissionRate = 0.001m,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a list of multiple backtest runs for testing pagination and filtering
    /// </summary>
    public static List<BacktestRunModel> CreateMultipleBacktestRuns(Guid userId, Guid portfolioId, int count = 5)
    {
        var backtestRuns = new List<BacktestRunModel>();
        var symbols = new[] { "BTCUSDT", "ETHUSDT", "ADAUSDT", "DOTUSDT", "LINKUSDT" };
        var timeframes = new[] { Timeframe.FiveMinutes, Timeframe.FifteenMinutes, Timeframe.OneHour, Timeframe.FourHours, Timeframe.OneDay };
        var statuses = new[] { BacktestStatus.Completed, BacktestStatus.Pending, BacktestStatus.Running, BacktestStatus.Failed };

        for (int i = 0; i < count; i++)
        {
            backtestRuns.Add(new BacktestRunModel
            {
                Id = Guid.NewGuid(),
                Name = $"Backtest Run {i + 1}",
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = portfolioId,
                UserId = userId,
                Symbol = symbols[i % symbols.Length],
                Exchange = Exchange.Kraken,
                Timeframe = timeframes[i % timeframes.Length],
                BacktestStartDate = DateTime.UtcNow.AddDays(-30 - i),
                BacktestEndDate = DateTime.UtcNow.AddDays(-1 - i),
                Status = statuses[i % statuses.Length],
                InitialBalance = 10000.0m + i * 1000,
                FinalBalance = 10000.0m + i * 1000 + Random.Shared.Next(-2000, 3000),
                TotalReturnPercent = Random.Shared.Next(-30, 50),
                MaxDrawdownPercent = Random.Shared.Next(-25, 0),
                SharpeRatio = (decimal)(Random.Shared.NextDouble() * 3 - 1),
                TotalTrades = Random.Shared.Next(5, 50),
                WinningTrades = Random.Shared.Next(2, 30),
                LosingTrades = Random.Shared.Next(1, 20),
                WinRatePercent = Random.Shared.Next(30, 80),
                AverageTradeReturnPercent = (decimal)(Random.Shared.NextDouble() * 10 - 2),
                ExecutionDuration = TimeSpan.FromMinutes(Random.Shared.Next(1, 10)),
                AlgorithmParameters = $"{{\"strategy\": \"strategy_{i}\", \"param\": {i * 10}}}",
                CommissionRate = 0.001m,
                CreatedAt = DateTime.UtcNow.AddDays(-i - 1),
                CreatedBy = "test"
            });
        }

        return backtestRuns;
    }

    /// <summary>
    /// Creates a backtest run model with custom parameters for specific test scenarios
    /// </summary>
    public static BacktestRunModel CreateCustomBacktestRun(Guid userId, Guid portfolioId, string name, string symbol = "BTCUSDT", Exchange exchange = Exchange.Kraken,
        Timeframe timeframe = Timeframe.OneHour, BacktestStatus status = BacktestStatus.Completed, decimal initialBalance = 10000.0m, decimal finalBalance = 0.0m,
        string? algorithmParameters = null, decimal commissionRate = 0.001m)
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId,
            UserId = userId,
            Symbol = symbol,
            Exchange = exchange,
            Timeframe = timeframe,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = status,
            InitialBalance = initialBalance,
            FinalBalance = finalBalance,
            AlgorithmParameters = algorithmParameters ?? "{\"default\": \"params\"}",
            CommissionRate = commissionRate,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a minimal backtest run model with only required fields
    /// </summary>
    public static BacktestRunModel CreateMinimalBacktestRun(Guid userId, Guid portfolioId, string name = "Minimal Backtest")
    {
        return new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId,
            UserId = userId,
            Symbol = "BTCUSDT",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = BacktestStatus.Pending,
            InitialBalance = 10000.0m,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }
}