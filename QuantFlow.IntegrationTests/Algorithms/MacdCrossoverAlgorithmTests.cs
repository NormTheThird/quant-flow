namespace QuantFlow.Test.Integration.Algorithms;

/// <summary>
/// Integration tests for MACD Crossover algorithm
/// Tests real MACD calculations and crossover trading signals
/// </summary>
public class MacdCrossoverAlgorithmTests : AlgorithmTestFixture
{
    private readonly MacdCrossoverAlgorithm _algorithm;

    public MacdCrossoverAlgorithmTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var algorithmLogger = loggerFactory.CreateLogger<MacdCrossoverAlgorithm>();
        var macdLogger = loggerFactory.CreateLogger<MacdIndicator>();

        var macdIndicator = new MacdIndicator(macdLogger, EmaIndicator);

        _algorithm = new MacdCrossoverAlgorithm(
            algorithmLogger,
            macdIndicator,
            AtrIndicator
        );
    }

    protected override BaseParameters CreateDefaultTestParameters()
    {
        return new MacdCrossoverParameters
        {
            FastPeriod = 12,
            SlowPeriod = 26,
            SignalPeriod = 9,
            StopLossPercent = 5.0m,
            TakeProfitPercent = 10.0m,
            RequireVolumeConfirmation = false,
            VolumeMultiplier = 1.3m,
            PositionSizePercent = 100m,
            UseATRForStops = false,
            ATRMultiplier = 2.0m
        };
    }

    #region Bullish Crossover Tests (Buy Signals)

    [Fact]
    public void Analyze_BullishCrossover_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBullishCrossover(bars: 50);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "MACD Bullish Crossover");

        // Act - Scan through data to find bullish crossover
        bool buySignalFound = false;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal);
                ValidateTradeSignal(signal, TradeSignal.Buy, testData[^1].Close);

                Logger.LogInformation("MACD bullish crossover buy signal at bar {Bar}, Entry={Entry:F2}", i, signal.EntryPrice);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "MACD bullish crossover should generate a buy signal");
    }

    [Fact]
    public void Analyze_BullishCrossover_WithVolumeConfirmation_HighVolume_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBullishCrossover(bars: 50);
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 1.0m; // Low threshold

        LogMarketDataSummary(data, "MACD Bullish Crossover with Volume");

        // Act
        bool buySignalFound = false;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Volume confirmed MACD bullish crossover at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "MACD bullish crossover with volume confirmation should generate buy signal");
    }

    [Fact]
    public void Analyze_BullishCrossover_WithVolumeConfirmation_LowVolume_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBullishCrossover(bars: 50);
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 5.0m; // Very high threshold

        LogMarketDataSummary(data, "MACD Bullish Crossover - Volume Fails");

        // Act - Check all bars
        bool anyBuySignal = false;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                anyBuySignal = true;
                break;
            }
        }

        // Assert
        Assert.False(anyBuySignal, "High volume requirement should prevent buy signals");
        Logger.LogInformation("Volume confirmation correctly prevented buy signals");
    }

    [Fact]
    public void Analyze_BullishCrossover_WithATRStops_ShouldUseATRForStopLoss()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBullishCrossover(bars: 50);
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.UseATRForStops = true;
        parameters.ATRMultiplier = 2.0m;

        LogMarketDataSummary(data, "MACD Bullish Crossover with ATR Stops");

        // Act
        TradeSignalModel? buySignal = null;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                buySignal = signal;
                break;
            }
        }

        // Assert
        Assert.NotNull(buySignal);
        AssertBuySignal(buySignal);

        var percentBasedStop = buySignal.EntryPrice!.Value * (1 - parameters.StopLossPercent / 100m);
        Assert.NotEqual(percentBasedStop, buySignal.StopLoss!.Value);

        Logger.LogInformation("ATR Stop Loss: {ATRStop:F2} vs Percentage Stop: {PercentStop:F2}",
            buySignal.StopLoss.Value, percentBasedStop);
    }

    [Fact]
    public void Analyze_BullishCrossover_CustomPeriods_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBullishCrossover(bars: 50);
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 8;  // Faster settings
        parameters.SlowPeriod = 17;
        parameters.SignalPeriod = 9;

        LogMarketDataSummary(data, "MACD Bullish Crossover - Custom Periods");

        // Act
        bool buySignalFound = false;
        for (int i = 30; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Buy signal with custom MACD periods at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Custom MACD periods should allow crossover detection");
    }

    #endregion

    #region Bearish Crossover Tests (Sell Signals)

    [Fact]
    public void Analyze_BearishCrossover_WithOpenPosition_ShouldGenerateSellSignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBearishCrossover(bars: 50);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "MACD Bearish Crossover");

        // Act
        bool sellSignalFound = false;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, currentPosition, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                AssertSellSignal(signal);
                Logger.LogInformation("MACD bearish crossover sell signal at bar {Bar}", i);
                sellSignalFound = true;
                break;
            }
        }

        Assert.True(sellSignalFound, "MACD bearish crossover should generate sell signal with open position");
    }

    [Fact]
    public void Analyze_BearishCrossover_WithoutPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateMacdBearishCrossover(bars: 50);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "MACD Bearish Crossover - No Position");

        // Act - Check all bars
        bool anySellSignal = false;
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                anySellSignal = true;
                break;
            }
        }

        // Assert
        Assert.False(anySellSignal, "Should not generate sell signal without position");
        Logger.LogInformation("No position to sell, correctly holding");
    }

    #endregion

    #region Market Condition Tests

    [Fact]
    public void Analyze_SidewaysMarket_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateSidewaysMarket(bars: 60);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Sideways Market");

        // Act - Check several points
        var signals = new List<TradeSignal>();
        for (int i = 35; i < data.Length; i += 5)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);
            signals.Add(signal.Action);
        }

        // Assert - Most signals should be Hold (no strong trend)
        var holdCount = signals.Count(s => s == TradeSignal.Hold);
        Assert.True(holdCount >= signals.Count * 0.6m, "Sideways market should generate mostly hold signals");

        Logger.LogInformation("Sideways market: {Hold} holds out of {Total} samples", holdCount, signals.Count);
    }

    [Fact]
    public void Analyze_BullTrend_AfterCrossover_WithPosition_ShouldHoldOrSell()
    {
        // Arrange
        var data = MarketDataFixture.CreateBullTrend(bars: 60);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "Bull Trend with Position");

        // Act - Check the end of the trend
        var signal = _algorithm.Analyze(data, currentPosition, parameters);

        // Assert - Should hold during uptrend or sell if bearish crossover
        Assert.True(signal.Action == TradeSignal.Hold || signal.Action == TradeSignal.Sell,
            "Should hold or sell during uptrend, not buy again");

        Logger.LogInformation("Signal during uptrend: {Action}", signal.Action);
    }

    [Fact]
    public void Analyze_BearTrend_NoPosition_MayGenerateBullishCrossover()
    {
        // Arrange
        var data = MarketDataFixture.CreateBearTrend(bars: 60, priceDecrease: 0.5m);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Bear Trend");

        // Act - During downtrend, check for any signals
        var signals = new List<TradeSignal>();
        for (int i = 35; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);
            signals.Add(signal.Action);
        }

        // Assert - Mostly holds during downtrend
        var holdCount = signals.Count(s => s == TradeSignal.Hold);
        Assert.True(holdCount > 0, "Should have some hold signals during downtrend");

        Logger.LogInformation("Bear trend signals: {Holds} holds, {Buys} buys out of {Total}",
            holdCount, signals.Count(s => s == TradeSignal.Buy), signals.Count);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Analyze_InsufficientData_ShouldReturnHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateInsufficientData(bars: 20);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Insufficient Data");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal, "Insufficient data");

        Logger.LogWarning("Not enough data for MACD calculation");
    }

    [Fact]
    public void Analyze_NullData_ShouldReturnHold()
    {
        // Arrange
        var parameters = CreateDefaultTestParameters();

        // Act
        var signal = _algorithm.Analyze(null!, null, parameters);

        // Assert
        AssertHoldSignal(signal, "Insufficient data");
    }

    [Fact]
    public void Analyze_EmptyData_ShouldReturnHold()
    {
        // Arrange
        var data = Array.Empty<MarketDataModel>();
        var parameters = CreateDefaultTestParameters();

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal, "Insufficient data");
    }

    #endregion

    #region Parameter Validation Tests

    [Fact]
    public void ValidateParameters_ValidParameters_ShouldReturnTrue()
    {
        // Arrange
        var parameters = CreateDefaultTestParameters();

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void ValidateParameters_FastPeriodZero_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 0;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Fast period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_SlowPeriodLessThanOrEqualToFast_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 26;
        parameters.SlowPeriod = 26; // Equal to fast

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Slow period must be greater than fast period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_SignalPeriodZero_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.SignalPeriod = 0;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Signal period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_InvalidStopLoss_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MacdCrossoverParameters)CreateDefaultTestParameters();
        parameters.StopLossPercent = -5m;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Stop loss", errorMessage);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Algorithm_ShouldHaveCorrectMetadata()
    {
        // Assert
        Assert.Contains("MACD", _algorithm.Name);
        Assert.Equal(AlgorithmType.TrendFollowing, _algorithm.Type);
        Assert.Equal(AlgorithmSource.HardCoded, _algorithm.Source);
        Assert.NotEmpty(_algorithm.Description);
    }

    [Fact]
    public void GetParameterDefinitions_ShouldReturnAllParameters()
    {
        // Act
        var definitions = _algorithm.GetParameterDefinitions();

        // Assert
        Assert.NotEmpty(definitions);
        Assert.Contains(definitions, p => p.Name == "FastPeriod");
        Assert.Contains(definitions, p => p.Name == "SlowPeriod");
        Assert.Contains(definitions, p => p.Name == "SignalPeriod");
        Assert.Contains(definitions, p => p.Name == "StopLossPercent");
        Assert.Contains(definitions, p => p.Name == "TakeProfitPercent");
    }

    #endregion
}