namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Base fixture class for algorithm testing with shared setup and utilities
/// </summary>
public abstract class AlgorithmTestFixture
{
    protected ILogger Logger { get; }
    protected SimpleMovingAverageIndicator SmaIndicator { get; }
    protected ExponentialMovingAverageIndicator EmaIndicator { get; }
    protected RelativeStrengthIndexIndicator RsiIndicator { get; }
    protected AverageTrueRangeIndicator AtrIndicator { get; }
    protected BollingerBandsIndicator BollingerBandsIndicator { get; }
    protected MacdIndicator MacdIndicator { get; }

    protected AlgorithmTestFixture()
    {
        // Create logger factory
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Initialize indicators with loggers
        Logger = loggerFactory.CreateLogger(GetType());
        SmaIndicator = new SimpleMovingAverageIndicator(loggerFactory.CreateLogger<SimpleMovingAverageIndicator>());
        EmaIndicator = new ExponentialMovingAverageIndicator(loggerFactory.CreateLogger<ExponentialMovingAverageIndicator>());
        RsiIndicator = new RelativeStrengthIndexIndicator(loggerFactory.CreateLogger<RelativeStrengthIndexIndicator>());
        AtrIndicator = new AverageTrueRangeIndicator(loggerFactory.CreateLogger<AverageTrueRangeIndicator>());
        BollingerBandsIndicator = new BollingerBandsIndicator(loggerFactory.CreateLogger<BollingerBandsIndicator>(), SmaIndicator);
        MacdIndicator = new MacdIndicator(loggerFactory.CreateLogger<MacdIndicator>(), EmaIndicator);
    }

    /// <summary>
    /// Asserts that a trade signal is a Buy signal
    /// </summary>
    protected void AssertBuySignal(TradeSignalModel signal, string? reason = null)
    {
        Assert.NotNull(signal);
        Assert.Equal(TradeSignal.Buy, signal.Action);
        Assert.True(signal.EntryPrice > 0, "Entry price should be greater than 0");
        Assert.True(signal.StopLoss > 0, "Stop loss should be greater than 0");
        Assert.True(signal.TakeProfit > 0, "Take profit should be greater than 0");

        if (reason != null)
        {
            Assert.Contains(reason, signal.Reason, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Asserts that a trade signal is a Sell signal
    /// </summary>
    protected void AssertSellSignal(TradeSignalModel signal, string? reason = null)
    {
        Assert.NotNull(signal);
        Assert.Equal(TradeSignal.Sell, signal.Action);
        Assert.True(signal.EntryPrice > 0, "Entry price should be greater than 0");

        if (reason != null)
        {
            Assert.Contains(reason, signal.Reason, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Asserts that a trade signal is a Hold signal
    /// </summary>
    protected void AssertHoldSignal(TradeSignalModel signal, string? reason = null)
    {
        Assert.NotNull(signal);
        Assert.Equal(TradeSignal.Hold, signal.Action);

        if (reason != null)
        {
            Assert.Contains(reason, signal.Reason, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Creates a mock current position for testing
    /// </summary>
    protected PositionModel CreateMockPosition(decimal entryPrice, decimal quantity)
    {
        return new PositionModel
        {
            Quantity = quantity,
            EntryPrice = entryPrice,
            EntryTime = DateTime.UtcNow.AddHours(-1),
            CurrentValue = entryPrice * quantity,
            UnrealizedPnL = 0m
        };
    }

    /// <summary>
    /// Logs market data summary for debugging
    /// </summary>
    protected void LogMarketDataSummary(MarketDataModel[] data, string scenario)
    {
        if (data == null || data.Length == 0)
            return;

        Logger.LogInformation("=== Market Data Summary: {Scenario} ===", scenario);
        Logger.LogInformation("Total Bars: {Count}", data.Length);
        Logger.LogInformation("Price Range: ${Low:F2} - ${High:F2}",
            data.Min(d => d.Low), data.Max(d => d.High));
        Logger.LogInformation("Start Price: ${Start:F2}, End Price: ${End:F2}",
            data[0].Open, data[^1].Close);
        Logger.LogInformation("Price Change: {Change:F2}%",
            ((data[^1].Close - data[0].Open) / data[0].Open) * 100);
        Logger.LogInformation("==========================================");
    }

    /// <summary>
    /// Validates that stop loss is properly set below entry for long positions
    /// </summary>
    protected void ValidateStopLossForLong(decimal entryPrice, decimal stopLoss)
    {
        Assert.True(stopLoss < entryPrice,
            $"Stop loss ({stopLoss:F2}) should be below entry price ({entryPrice:F2}) for long positions");

        var stopLossPercent = ((entryPrice - stopLoss) / entryPrice) * 100;
        Assert.True(stopLossPercent > 0 && stopLossPercent <= 50,
            $"Stop loss percent ({stopLossPercent:F2}%) should be between 0% and 50%");
    }

    /// <summary>
    /// Validates that take profit is properly set above entry for long positions
    /// </summary>
    protected void ValidateTakeProfitForLong(decimal entryPrice, decimal takeProfit)
    {
        Assert.True(takeProfit > entryPrice,
            $"Take profit ({takeProfit:F2}) should be above entry price ({entryPrice:F2}) for long positions");

        var takeProfitPercent = ((takeProfit - entryPrice) / entryPrice) * 100;
        Assert.True(takeProfitPercent > 0 && takeProfitPercent <= 1000,
            $"Take profit percent ({takeProfitPercent:F2}%) should be between 0% and 1000%");
    }

    /// <summary>
    /// Validates complete trade signal structure and values
    /// </summary>
    protected void ValidateTradeSignal(TradeSignalModel signal, TradeSignal expectedAction, decimal currentPrice)
    {
        Assert.NotNull(signal);
        Assert.Equal(expectedAction, signal.Action);
        Assert.False(string.IsNullOrWhiteSpace(signal.Reason), "Signal should have a reason");

        if (expectedAction == TradeSignal.Buy)
        {
            Assert.True(signal.EntryPrice.HasValue && signal.EntryPrice.Value > 0, "Entry price should be set for Buy signal");
            Assert.True(signal.StopLoss.HasValue && signal.StopLoss.Value > 0, "Stop loss should be set for Buy signal");
            Assert.True(signal.TakeProfit.HasValue && signal.TakeProfit.Value > 0, "Take profit should be set for Buy signal");
            Assert.True(signal.PositionSizePercent > 0 && signal.PositionSizePercent <= 100,
                "Position size percent should be between 0 and 100");

            ValidateStopLossForLong(signal.EntryPrice.Value, signal.StopLoss.Value);
            ValidateTakeProfitForLong(signal.EntryPrice.Value, signal.TakeProfit.Value);

            // Entry price should be close to current price (within 5%)
            var priceDiff = Math.Abs(signal.EntryPrice.Value - currentPrice) / currentPrice;
            Assert.True(priceDiff < 0.05m,
                $"Entry price ({signal.EntryPrice.Value:F2}) should be close to current price ({currentPrice:F2})");
        }
        else if (expectedAction == TradeSignal.Sell)
        {
            Assert.True(signal.EntryPrice.HasValue && signal.EntryPrice.Value > 0, "Entry price should be set for Sell signal");

            // Entry price should be close to current price (within 5%)
            var priceDiff = Math.Abs(signal.EntryPrice.Value - currentPrice) / currentPrice;
            Assert.True(priceDiff < 0.05m,
                $"Entry price ({signal.EntryPrice.Value:F2}) should be close to current price ({currentPrice:F2})");
        }
    }

    /// <summary>
    /// Creates default parameters for testing (can be overridden by specific tests)
    /// </summary>
    protected abstract BaseParameters CreateDefaultTestParameters();
}