namespace QuantFlow.Test.Integration.Algorithms;

/// <summary>
/// Integration tests for RSI Mean Reversion algorithm
/// Tests real RSI calculations and mean reversion trading signals
/// </summary>
public class RsiMeanReversionAlgorithmTests : AlgorithmTestFixture
{
    private readonly RsiMeanReversionAlgorithm _algorithm;

    public RsiMeanReversionAlgorithmTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<RsiMeanReversionAlgorithm>();

        _algorithm = new RsiMeanReversionAlgorithm(logger, RsiIndicator, AtrIndicator);
    }

    protected override BaseParameters CreateDefaultTestParameters()
    {
        return new RsiMeanReversionParameters
        {
            RsiPeriod = 14,
            OversoldThreshold = 30m,
            OverboughtThreshold = 70m,
            StopLossPercent = 5.0m,
            TakeProfitPercent = 10.0m,
            RequireVolumeConfirmation = false,
            VolumeMultiplier = 1.3m,
            PositionSizePercent = 100m,
            UseATRForStops = false,
            ATRMultiplier = 2.0m
        };
    }

    #region Oversold Tests (Buy Signals)

    [Fact]
    public void Analyze_RsiOversold_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOversold(bars: 35);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "RSI Oversold Scenario");

        // Act - Scan through data to find oversold condition
        bool buySignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal, "oversold");
                ValidateTradeSignal(signal, TradeSignal.Buy, testData[^1].Close);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "RSI oversold should generate a buy signal");
    }

    [Fact]
    public void Analyze_RsiOversold_WithVolumeConfirmation_HighVolume_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOversold(bars: 30);
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 1.1m; // Low threshold

        LogMarketDataSummary(data, "RSI Oversold with Volume Confirmation");

        // Act
        bool buySignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Volume confirmed RSI buy signal at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "RSI oversold with volume confirmation should generate buy signal");
    }

    [Fact]
    public void Analyze_RsiOversold_WithVolumeConfirmation_LowVolume_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOversold(bars: 30);
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 5.0m; // Very high threshold

        LogMarketDataSummary(data, "RSI Oversold - Volume Fails");

        // Act - Check all bars
        bool anyBuySignal = false;
        for (int i = 20; i < data.Length; i++)
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
    public void Analyze_RsiOversold_WithATRStops_ShouldUseATRForStopLoss()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOversold(bars: 30);
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.UseATRForStops = true;
        parameters.ATRMultiplier = 2.0m;

        LogMarketDataSummary(data, "RSI Oversold with ATR Stops");

        // Act
        TradeSignalModel? buySignal = null;
        for (int i = 20; i < data.Length; i++)
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
    public void Analyze_RsiOversold_CustomThreshold_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOversold(bars: 30);
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.OversoldThreshold = 35m; // Higher threshold (more sensitive)

        LogMarketDataSummary(data, "RSI Oversold - Custom Threshold 35");

        // Act
        bool buySignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Buy signal with custom RSI threshold at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Custom oversold threshold should generate buy signal");
    }

    #endregion

    #region Overbought Tests (Sell Signals)

    [Fact]
    public void Analyze_RsiOverbought_WithOpenPosition_ShouldGenerateSellSignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOverbought(bars: 30);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "RSI Overbought Scenario");

        // Act
        bool sellSignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, currentPosition, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                AssertSellSignal(signal, "overbought");
                Logger.LogInformation("RSI overbought sell signal at bar {Bar}, Price: {Price:F2}", i, signal.EntryPrice);
                sellSignalFound = true;
                break;
            }
        }

        Assert.True(sellSignalFound, "RSI overbought should generate sell signal with open position");
    }

    [Fact]
    public void Analyze_RsiOverbought_WithoutPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOverbought(bars: 30);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "RSI Overbought - No Position");

        // Act - Check all bars
        bool anySellSignal = false;
        for (int i = 20; i < data.Length; i++)
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

    [Fact]
    public void Analyze_RsiOverbought_CustomThreshold_ShouldGenerateSellSignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateRsiOverbought(bars: 30);
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.OverboughtThreshold = 65m; // Lower threshold (more sensitive)
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "RSI Overbought - Custom Threshold 65");

        // Act
        bool sellSignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, currentPosition, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                AssertSellSignal(signal);
                Logger.LogInformation("Sell signal with custom RSI threshold at bar {Bar}", i);
                sellSignalFound = true;
                break;
            }
        }

        Assert.True(sellSignalFound, "Custom overbought threshold should generate sell signal");
    }

    #endregion

    #region Market Condition Tests

    [Fact]
    public void Analyze_SidewaysMarket_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateSidewaysMarket(bars: 50);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Sideways Market");

        // Act - Check several points
        var signals = new List<TradeSignal>();
        for (int i = 20; i < data.Length; i += 5)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);
            signals.Add(signal.Action);
        }

        // Assert - Most signals should be Hold (RSI stays in neutral range)
        var holdCount = signals.Count(s => s == TradeSignal.Hold);
        Assert.True(holdCount >= signals.Count * 0.5m, "Sideways market should generate mostly hold signals");

        Logger.LogInformation("Sideways market: {Hold} holds out of {Total} samples", holdCount, signals.Count);
    }

    [Fact]
    public void Analyze_BullTrend_WithPosition_MayTriggerSellIfOverbought()
    {
        // Arrange
        var data = MarketDataFixture.CreateBullTrend(bars: 50);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 100m, quantity: 10m);

        LogMarketDataSummary(data, "Bull Trend with Position");

        // Act
        var signal = _algorithm.Analyze(data, currentPosition, parameters);

        // Assert - In a strong bull trend, RSI may go overbought and trigger sell
        // This is CORRECT behavior for mean reversion - take profits when overbought
        if (signal.Action == TradeSignal.Sell)
        {
            AssertSellSignal(signal, "overbought");
            Logger.LogInformation("Correctly sold position due to overbought RSI during uptrend");
        }
        else
        {
            AssertHoldSignal(signal);
            Logger.LogInformation("RSI still in neutral range during uptrend");
        }

        // The important thing is we don't generate a BUY signal when we already have a position
        Assert.NotEqual(TradeSignal.Buy, signal.Action);
    }

    [Fact]
    public void Analyze_BearTrend_NoPosition_ShouldEventuallyBuy()
    {
        // Arrange
        var data = MarketDataFixture.CreateBearTrend(bars: 50, priceDecrease: 1.0m);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Bear Trend - Should Get Oversold");

        // Act - Strong downtrend should create oversold RSI
        bool buySignalFound = false;
        for (int i = 20; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                buySignalFound = true;
                Logger.LogInformation("Oversold buy signal during downtrend at bar {Bar}", i);
                break;
            }
        }

        Assert.True(buySignalFound, "Strong downtrend should eventually create oversold condition");
    }

    #endregion

    #region Edge Case Tests

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

        Logger.LogWarning("Not enough data for RSI calculation");
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
    public void ValidateParameters_RsiPeriodZero_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.RsiPeriod = 0;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("RSI period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_OversoldThresholdInvalid_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.OversoldThreshold = 80m; // Greater than overbought

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Oversold threshold", errorMessage);
    }

    [Fact]
    public void ValidateParameters_OverboughtThresholdInvalid_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
        parameters.OverboughtThreshold = 105m; // Greater than 100

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Overbought threshold", errorMessage);
    }

    [Fact]
    public void ValidateParameters_InvalidStopLoss_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (RsiMeanReversionParameters)CreateDefaultTestParameters();
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
        Assert.Equal("RSI Mean Reversion", _algorithm.Name);
        Assert.Equal(AlgorithmType.MeanReversion, _algorithm.Type);
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
        Assert.Contains(definitions, p => p.Name == "RsiPeriod");
        Assert.Contains(definitions, p => p.Name == "OversoldThreshold");
        Assert.Contains(definitions, p => p.Name == "OverboughtThreshold");
        Assert.Contains(definitions, p => p.Name == "StopLossPercent");
        Assert.Contains(definitions, p => p.Name == "TakeProfitPercent");
    }

    #endregion
}