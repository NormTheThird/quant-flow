namespace QuantFlow.Test.Integration.Algorithms;

/// <summary>
/// Integration tests for Moving Average Crossover algorithm
/// Tests real indicator calculations and trading signal generation
/// </summary>
public class MovingAverageCrossoverAlgorithmTests : AlgorithmTestFixture
{
    private readonly MovingAverageCrossoverAlgorithm _algorithm;

    public MovingAverageCrossoverAlgorithmTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MovingAverageCrossoverAlgorithm>();

        _algorithm = new MovingAverageCrossoverAlgorithm(logger, SmaIndicator, EmaIndicator, AtrIndicator);
    }

    protected override BaseParameters CreateDefaultTestParameters()
    {
        return new MovingAverageCrossoverParameters
        {
            FastPeriod = 9,
            SlowPeriod = 21,
            MAType = MovingAverageType.SimpleMovingAverage,
            StopLossPercent = 5.0m,
            TakeProfitPercent = 10.0m,
            RequireVolumeConfirmation = false,
            VolumeMultiplier = 1.3m,
            PositionSizePercent = 100m,
            UseATRForStops = false,
            ATRMultiplier = 2.0m
        };
    }

    #region Golden Cross Tests (Bullish Signal)

    [Fact]
    public void Analyze_GoldenCross_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateGoldenCross(barsBefore: 25, barsAfter: 10);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Golden Cross Scenario");

        // Act - Scan through the data to find where the golden cross occurs
        bool buySignalFound = false;
        for (int i = 25; i < data.Length; i++)  // Start after slow MA has enough data
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal, "Golden Cross");
                ValidateTradeSignal(signal, TradeSignal.Buy, testData[^1].Close);

                Logger.LogInformation("Golden Cross buy signal found at bar {Bar}, Entry={Entry:F2}, StopLoss={StopLoss:F2}, TakeProfit={TakeProfit:F2}",
                    i, signal.EntryPrice, signal.StopLoss, signal.TakeProfit);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Golden cross should generate a buy signal at some point in the dataset");
    }

    [Fact]
    public void Analyze_GoldenCross_WithEMA_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateGoldenCross(barsBefore: 25, barsAfter: 10);
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.MAType = MovingAverageType.ExponentialMovingAverage;

        LogMarketDataSummary(data, "Golden Cross with EMA");

        // Act - Scan through the data to find where the golden cross occurs
        bool buySignalFound = false;
        for (int i = 25; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal, "Golden Cross");
                Assert.Equal(MovingAverageType.ExponentialMovingAverage, parameters.MAType);

                Logger.LogInformation("EMA Golden Cross Signal at bar {Bar}: Entry={Entry:F2}", i, signal.EntryPrice);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Golden cross with EMA should generate a buy signal");
    }

    [Fact]
    public void Analyze_GoldenCross_WithVolumeConfirmation_HighVolume_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateGoldenCross(barsBefore: 25, barsAfter: 10);
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 1.2m; // Lower threshold so high volume passes

        LogMarketDataSummary(data, "Golden Cross with Volume Confirmation");

        // Act - Scan through the data to find where the golden cross occurs
        bool buySignalFound = false;
        for (int i = 25; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal);
                Logger.LogInformation("Volume confirmed buy signal at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Golden cross with volume confirmation should generate a buy signal");
    }

    [Fact]
    public void Analyze_GoldenCross_WithVolumeConfirmation_LowVolume_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateGoldenCross(barsBefore: 25, barsAfter: 10);
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 5.0m; // Very high threshold so volume fails

        LogMarketDataSummary(data, "Golden Cross - Volume Fails");

        // Act - Check all bars to ensure volume confirmation prevents buy
        bool anyBuySignal = false;
        for (int i = 25; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                anyBuySignal = true;
                break;
            }
        }

        // Assert - Should NOT find any buy signals due to volume requirement
        Assert.False(anyBuySignal, "High volume multiplier should prevent buy signals");
        Logger.LogInformation("Volume confirmation correctly prevented buy signals");
    }

    [Fact]
    public void Analyze_GoldenCross_WithATRStops_ShouldUseATRForStopLoss()
    {
        // Arrange
        var data = MarketDataFixture.CreateGoldenCross(barsBefore: 25, barsAfter: 10);
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.UseATRForStops = true;
        parameters.ATRMultiplier = 2.0m;

        LogMarketDataSummary(data, "Golden Cross with ATR Stops");

        // Act - Scan through the data to find where the golden cross occurs
        TradeSignalModel? buySignal = null;
        for (int i = 25; i < data.Length; i++)
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

        // Calculate what percentage-based stop loss would be
        var percentBasedStop = buySignal.EntryPrice!.Value * (1 - parameters.StopLossPercent / 100m);

        // ATR-based stop should be different from percentage-based
        Assert.NotEqual(percentBasedStop, buySignal.StopLoss!.Value);

        Logger.LogInformation("ATR Stop Loss: {ATRStop:F2} vs Percentage Stop: {PercentStop:F2}",
            buySignal.StopLoss.Value, percentBasedStop);
    }

    #endregion

    #region Death Cross Tests (Bearish Signal)

    [Fact]
    public void Analyze_DeathCross_WithOpenPosition_ShouldGenerateSellSignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateDeathCross(barsBefore: 30, barsAfter: 10);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "Death Cross Scenario");

        // Act - Scan through the data to find where the death cross occurs
        bool sellSignalFound = false;
        for (int i = 30; i < data.Length; i++)  // Start after slow MA has enough data
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, currentPosition, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                // Assert
                AssertSellSignal(signal, "Death Cross");
                Logger.LogInformation("Death Cross sell signal found at bar {Bar}, Price: {Price:F2}", i, signal.EntryPrice);
                sellSignalFound = true;
                break;
            }
        }

        // Assert that we found the sell signal somewhere in the data
        Assert.True(sellSignalFound, "Death cross should generate a sell signal at some point in the dataset");
    }

    [Fact]
    public void Analyze_DeathCross_WithoutPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateDeathCross(barsBefore: 25, barsAfter: 5);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Death Cross - No Position");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal);

        Logger.LogInformation("No position to sell, holding");
    }

    #endregion

    #region Sideways Market Tests

    [Fact]
    public void Analyze_SidewaysMarket_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateSidewaysMarket(bars: 50);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Sideways Market");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal);

        Logger.LogInformation("No crossover detected in sideways market");
    }

    #endregion

    #region Trend Tests

    [Fact]
    public void Analyze_BullTrend_AfterGoldenCross_WithPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateBullTrend(bars: 50);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "Bull Trend with Position");

        // Act
        var signal = _algorithm.Analyze(data, currentPosition, parameters);

        // Assert
        AssertHoldSignal(signal);

        Logger.LogInformation("Already in position during uptrend, holding");
    }

    [Fact]
    public void Analyze_BearTrend_NoPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateBearTrend(bars: 50);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Bear Trend - No Position");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal);

        Logger.LogInformation("Bearish trend but no position to sell, holding");
    }

    #endregion

    #region Insufficient Data Tests

    [Fact]
    public void Analyze_InsufficientData_ShouldReturnHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateInsufficientData(bars: 10);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Insufficient Data");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal, "Insufficient data");

        Logger.LogWarning("Not enough data for MA calculation");
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
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 0;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Fast period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_FastPeriodGreaterThanOrEqualToSlow_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 21;
        parameters.SlowPeriod = 21;  // Equal to fast (invalid)

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Fast period must be greater than 0 and less than slow period", errorMessage);

        Logger.LogInformation("Validation correctly failed: {Error}", errorMessage);
    }

    [Fact]
    public void ValidateParameters_FastPeriodGreaterThanSlow_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.FastPeriod = 30;
        parameters.SlowPeriod = 20;  // Less than fast (invalid)

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Fast period", errorMessage);

        Logger.LogInformation("Validation correctly failed: {Error}", errorMessage);
    }

    [Fact]
    public void ValidateParameters_InvalidStopLoss_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (MovingAverageCrossoverParameters)CreateDefaultTestParameters();
        parameters.StopLossPercent = -5m;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Stop loss", errorMessage);
    }

    #endregion

    #region Algorithm Metadata Tests

    [Fact]
    public void Algorithm_ShouldHaveCorrectMetadata()
    {
        // Assert
        Assert.Equal("Moving Average Crossover", _algorithm.Name);
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
        Assert.Contains(definitions, p => p.Name == "MAType");
        Assert.Contains(definitions, p => p.Name == "StopLossPercent");
        Assert.Contains(definitions, p => p.Name == "TakeProfitPercent");
    }

    #endregion
}