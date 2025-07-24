namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture class for creating TradeModel test data
/// </summary>
public static class TradeModelFixture
{
    /// <summary>
    /// Creates a default buy trade model for testing
    /// </summary>
    public static TradeModel CreateDefaultBuyTrade(Guid? backtestRunId = null, string symbol = "BTCUSDT")
    {
        return new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = backtestRunId ?? Guid.NewGuid(),
            Symbol = symbol,
            Type = TradeType.Buy,
            ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
            Quantity = 0.1m,
            Price = 50000.0m,
            Value = 5000.0m,
            Commission = 5.0m,
            NetValue = 4995.0m,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = 5000.0m,
            AlgorithmReason = "Buy signal detected",
            AlgorithmConfidence = 0.85m,
            RealizedProfitLoss = null,
            RealizedProfitLossPercent = null,
            EntryTradeId = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a default sell trade model for testing
    /// </summary>
    public static TradeModel CreateDefaultSellTrade(Guid? backtestRunId = null, string symbol = "BTCUSDT", Guid? entryTradeId = null)
    {
        return new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = backtestRunId ?? Guid.NewGuid(),
            Symbol = symbol,
            Type = TradeType.Sell,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 0.1m,
            Price = 52000.0m,
            Value = 5200.0m,
            Commission = 5.2m,
            NetValue = 5194.8m,
            PortfolioBalanceBefore = 5000.0m,
            PortfolioBalanceAfter = 10194.8m,
            AlgorithmReason = "Sell signal detected",
            AlgorithmConfidence = 0.90m,
            RealizedProfitLoss = 194.8m,
            RealizedProfitLossPercent = 3.9m,
            EntryTradeId = entryTradeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a profitable trade pair (buy + sell) for testing
    /// </summary>
    public static (TradeModel buyTrade, TradeModel sellTrade) CreateProfitableTradePair(Guid? backtestRunId = null, string symbol = "BTCUSDT")
    {
        var runId = backtestRunId ?? Guid.NewGuid();
        var buyTrade = CreateDefaultBuyTrade(runId, symbol);
        buyTrade.ExecutionTimestamp = DateTime.UtcNow.AddDays(-2);
        buyTrade.Price = 48000.0m;
        buyTrade.Value = 4800.0m;
        buyTrade.NetValue = 4795.2m;

        var sellTrade = CreateDefaultSellTrade(runId, symbol, buyTrade.Id);
        sellTrade.ExecutionTimestamp = DateTime.UtcNow.AddDays(-1);
        sellTrade.Price = 52000.0m;
        sellTrade.Value = 5200.0m;
        sellTrade.NetValue = 5194.8m;
        sellTrade.RealizedProfitLoss = 399.6m;
        sellTrade.RealizedProfitLossPercent = 8.33m;

        return (buyTrade, sellTrade);
    }

    /// <summary>
    /// Creates a losing trade pair (buy + sell) for testing
    /// </summary>
    public static (TradeModel buyTrade, TradeModel sellTrade) CreateLosingTradePair(Guid? backtestRunId = null, string symbol = "BTCUSDT")
    {
        var runId = backtestRunId ?? Guid.NewGuid();
        var buyTrade = CreateDefaultBuyTrade(runId, symbol);
        buyTrade.ExecutionTimestamp = DateTime.UtcNow.AddDays(-2);
        buyTrade.Price = 50000.0m;
        buyTrade.Value = 5000.0m;
        buyTrade.NetValue = 4995.0m;

        var sellTrade = CreateDefaultSellTrade(runId, symbol, buyTrade.Id);
        sellTrade.ExecutionTimestamp = DateTime.UtcNow.AddDays(-1);
        sellTrade.Price = 47000.0m;
        sellTrade.Value = 4700.0m;
        sellTrade.NetValue = 4695.3m;
        sellTrade.RealizedProfitLoss = -299.7m;
        sellTrade.RealizedProfitLossPercent = -6.0m;

        return (buyTrade, sellTrade);
    }

    /// <summary>
    /// Creates a custom trade model with specific parameters
    /// </summary>
    public static TradeModel CreateCustomTrade(Guid? backtestRunId = null, string symbol = "BTCUSDT", TradeType type = TradeType.Buy, decimal quantity = 0.1m,
        decimal price = 50000.0m, DateTime? executionTime = null, string algorithmReason = "Test signal", decimal confidence = 0.85m)
    {
        var value = quantity * price;
        var commission = value * 0.001m; // 0.1% commission
        var netValue = type == TradeType.Buy ? value + commission : value - commission;

        return new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = backtestRunId ?? Guid.NewGuid(),
            Symbol = symbol,
            Type = type,
            ExecutionTimestamp = executionTime ?? DateTime.UtcNow,
            Quantity = quantity,
            Price = price,
            Value = value,
            Commission = commission,
            NetValue = netValue,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = type == TradeType.Buy ? 10000.0m - netValue : 10000.0m + netValue,
            AlgorithmReason = algorithmReason,
            AlgorithmConfidence = confidence,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    /// <summary>
    /// Creates a batch of trades for bulk testing
    /// </summary>
    public static List<TradeModel> CreateTradeBatch(Guid? backtestRunId = null, int count = 5, string symbol = "BTCUSDT", bool alternateTypes = true)
    {
        var trades = new List<TradeModel>();
        var runId = backtestRunId ?? Guid.NewGuid();

        for (int i = 0; i < count; i++)
        {
            var type = alternateTypes && i % 2 == 1 ? TradeType.Sell : TradeType.Buy;
            var basePrice = 50000.0m + (i * 1000.0m); // Increment price for each trade
            var executionTime = DateTime.UtcNow.AddMinutes(-count + i); // Spread trades over time

            var trade = CreateCustomTrade(
                backtestRunId: runId,
                symbol: symbol,
                type: type,
                price: basePrice,
                executionTime: executionTime,
                algorithmReason: $"Signal {i + 1}",
                confidence: 0.8m + (i * 0.02m)
            );

            trades.Add(trade);
        }

        return trades;
    }

    /// <summary>
    /// Creates a multi-symbol trade batch for testing
    /// </summary>
    public static List<TradeModel> CreateMultiSymbolTradeBatch(Guid? backtestRunId = null)
    {
        var runId = backtestRunId ?? Guid.NewGuid();
        var trades = new List<TradeModel>();

        // BTCUSDT trades
        trades.Add(CreateCustomTrade(runId, "BTCUSDT", TradeType.Buy, 0.1m, 50000.0m, DateTime.UtcNow.AddDays(-2)));
        trades.Add(CreateCustomTrade(runId, "BTCUSDT", TradeType.Sell, 0.1m, 52000.0m, DateTime.UtcNow.AddDays(-1)));

        // ETHUSDT trades
        trades.Add(CreateCustomTrade(runId, "ETHUSDT", TradeType.Buy, 1.0m, 3000.0m, DateTime.UtcNow.AddHours(-12)));
        trades.Add(CreateCustomTrade(runId, "ETHUSDT", TradeType.Sell, 1.0m, 3100.0m, DateTime.UtcNow.AddHours(-6)));

        // ADAUSDT trades
        trades.Add(CreateCustomTrade(runId, "ADAUSDT", TradeType.Buy, 1000.0m, 0.5m, DateTime.UtcNow.AddHours(-3)));

        return trades;
    }

    /// <summary>
    /// Creates a trade for updating tests
    /// </summary>
    public static TradeModel CreateTradeForUpdate(Guid tradeId, Guid backtestRunId, TradeType newType = TradeType.Sell, decimal newPrice = 52000.0m,
        string newReason = "Updated signal")
    {
        return new TradeModel
        {
            Id = tradeId,
            BacktestRunId = backtestRunId,
            Symbol = "BTCUSDT",
            Type = newType,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 0.1m,
            Price = newPrice,
            Value = 0.1m * newPrice,
            Commission = (0.1m * newPrice) * 0.001m,
            NetValue = (0.1m * newPrice) - ((0.1m * newPrice) * 0.001m),
            PortfolioBalanceBefore = 5000.0m,
            PortfolioBalanceAfter = 10000.0m,
            AlgorithmReason = newReason,
            AlgorithmConfidence = 0.95m,
            RealizedProfitLoss = 194.8m,
            RealizedProfitLossPercent = 3.9m,
            EntryTradeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }
}