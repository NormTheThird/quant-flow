namespace QuantFlow.Test.Integration.Algorithms;

/// <summary>
/// Integration tests for VWAP algorithm
/// Tests VWAP mean reversion trading signals
/// </summary>
public class VwapAlgorithmTests : AlgorithmTestFixture
{
    private readonly VwapAlgorithm _algorithm;

    public VwapAlgorithmTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var algorithmLogger = loggerFactory.CreateLogger<VwapAlgorithm>();

        _algorithm = new VwapAlgorithm(
            algorithmLogger,
            AtrIndicator
        );
    }

    protected override BaseParameters CreateDefaultTestParameters()
    {
        return new VwapParameters
        {
            Period = 14,
            DeviationThresholdPercent = 2.0m,
            StopLossPercent = 5.0m,
            TakeProfitPercent = 10.0m,
            RequireVolumeConfirmation = false,
            VolumeMultiplier = 1.3m,
            PositionSizePercent = 100m,
            UseATRForStops = false,
            ATRMultiplier = 2.0m
        };
    }

    #region Below VWAP Tests (Buy Signals)

    [Fact]
    public void Analyze_PriceBelowVWAP_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapBelowScenario(bars: 30);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Price Below VWAP");

        // Act - Scan through data to find below VWAP condition
        bool buySignalFound = false;
        for (int i = 15; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                // Assert
                AssertBuySignal(signal);
                ValidateTradeSignal(signal, TradeSignal.Buy, testData[^1].Close);

                Logger.LogInformation("Price below VWAP buy signal at bar {Bar}, Entry={Entry:F2}", i, signal.EntryPrice);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Price below VWAP should generate a buy signal");
    }

    [Fact]
    public void Analyze_PriceBelowVWAP_WithVolumeConfirmation_HighVolume_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapBelowScenario(bars: 30);
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 1.0m; // Low threshold

        LogMarketDataSummary(data, "Price Below VWAP with Volume");

        // Act
        bool buySignalFound = false;
        for (int i = 15; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Volume confirmed below VWAP at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Price below VWAP with volume confirmation should generate buy signal");
    }

    [Fact]
    public void Analyze_PriceBelowVWAP_WithVolumeConfirmation_LowVolume_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapBelowScenario(bars: 30);
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.RequireVolumeConfirmation = true;
        parameters.VolumeMultiplier = 5.0m; // Very high threshold

        LogMarketDataSummary(data, "Price Below VWAP - Volume Fails");

        // Act - Check all bars
        bool anyBuySignal = false;
        for (int i = 15; i < data.Length; i++)
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
    public void Analyze_PriceBelowVWAP_WithATRStops_ShouldUseATRForStopLoss()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapBelowScenario(bars: 30);
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.UseATRForStops = true;
        parameters.ATRMultiplier = 2.0m;

        LogMarketDataSummary(data, "Price Below VWAP with ATR Stops");

        // Act
        TradeSignalModel? buySignal = null;
        for (int i = 15; i < data.Length; i++)
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
    public void Analyze_PriceBelowVWAP_CustomThreshold_ShouldGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapBelowScenario(bars: 30);
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.DeviationThresholdPercent = 1.0m; // More sensitive (lower threshold)

        LogMarketDataSummary(data, "Price Below VWAP - Custom Threshold");

        // Act
        bool buySignalFound = false;
        for (int i = 15; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);

            if (signal.Action == TradeSignal.Buy)
            {
                AssertBuySignal(signal);
                Logger.LogInformation("Buy signal with custom threshold at bar {Bar}", i);
                buySignalFound = true;
                break;
            }
        }

        Assert.True(buySignalFound, "Custom threshold should allow VWAP detection");
    }

    [Fact]
    public void Analyze_PriceSlightlyBelowVWAP_HighThreshold_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapSlightlyBelowScenario(bars: 30);
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.DeviationThresholdPercent = 5.0m; // Very high threshold

        LogMarketDataSummary(data, "Price Slightly Below VWAP - High Threshold");

        // Act
        bool anyBuySignal = false;
        for (int i = 15; i < data.Length; i++)
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
        Assert.False(anyBuySignal, "Small deviation should not trigger buy with high threshold");
    }

    #endregion

    #region Above VWAP Tests (Sell Signals)

    [Fact]
    public void Analyze_PriceAboveVWAP_WithOpenPosition_ShouldGenerateSellSignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapAboveScenario(bars: 30);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 95m, quantity: 10m);

        LogMarketDataSummary(data, "Price Above VWAP");

        // Act
        bool sellSignalFound = false;
        for (int i = 15; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, currentPosition, parameters);

            if (signal.Action == TradeSignal.Sell)
            {
                AssertSellSignal(signal);
                Logger.LogInformation("Price above VWAP sell signal at bar {Bar}", i);
                sellSignalFound = true;
                break;
            }
        }

        Assert.True(sellSignalFound, "Price above VWAP should generate sell signal with open position");
    }

    [Fact]
    public void Analyze_PriceAboveVWAP_WithoutPosition_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateVwapAboveScenario(bars: 30);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Price Above VWAP - No Position");

        // Act - Check all bars
        bool anySellSignal = false;
        for (int i = 15; i < data.Length; i++)
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
    public void Analyze_PriceNearVWAP_ShouldHold()
    {
        // Arrange
        var data = MarketDataFixture.CreateSidewaysMarket(bars: 30);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Price Near VWAP");

        // Act - Check several points
        var signals = new List<TradeSignal>();
        for (int i = 15; i < data.Length; i += 3)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);
            signals.Add(signal.Action);
        }

        // Assert - Most signals should be Hold (price near VWAP)
        var holdCount = signals.Count(s => s == TradeSignal.Hold);
        Assert.True(holdCount >= signals.Count * 0.6m, "Price near VWAP should generate mostly hold signals");

        Logger.LogInformation("Near VWAP: {Hold} holds out of {Total} samples", holdCount, signals.Count);
    }

    [Fact]
    public void Analyze_BullTrend_AfterBuy_WithPosition_ShouldHoldOrSell()
    {
        // Arrange
        var data = MarketDataFixture.CreateBullTrend(bars: 30);
        var parameters = CreateDefaultTestParameters();
        var currentPosition = CreateMockPosition(entryPrice: 95m, quantity: 10m);

        LogMarketDataSummary(data, "Bull Trend with Position");

        // Act - Check the end of the trend
        var signal = _algorithm.Analyze(data, currentPosition, parameters);

        // Assert - Should hold or sell if price goes above VWAP threshold
        Assert.True(signal.Action == TradeSignal.Hold || signal.Action == TradeSignal.Sell,
            "Should hold or sell during uptrend, not buy again");

        Logger.LogInformation("Signal during uptrend: {Action}", signal.Action);
    }

    [Fact]
    public void Analyze_BearTrend_NoPosition_MayGenerateBuySignal()
    {
        // Arrange
        var data = MarketDataFixture.CreateBearTrend(bars: 30, priceDecrease: 0.8m);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Bear Trend");

        // Act
        var signals = new List<TradeSignal>();
        for (int i = 15; i < data.Length; i++)
        {
            var testData = data[..(i + 1)];
            var signal = _algorithm.Analyze(testData, null, parameters);
            signals.Add(signal.Action);
        }

        // Assert - Should have some holds, possibly buy signals
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
        var data = MarketDataFixture.CreateInsufficientData(bars: 10);
        var parameters = CreateDefaultTestParameters();

        LogMarketDataSummary(data, "Insufficient Data");

        // Act
        var signal = _algorithm.Analyze(data, null, parameters);

        // Assert
        AssertHoldSignal(signal, "Insufficient data");

        Logger.LogWarning("Not enough data for VWAP calculation");
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
    public void ValidateParameters_PeriodZero_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.Period = 0;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Period", errorMessage);
    }

    [Fact]
    public void ValidateParameters_DeviationThresholdZero_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.DeviationThresholdPercent = 0m;

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Deviation threshold", errorMessage);
    }

    [Fact]
    public void ValidateParameters_DeviationThresholdTooHigh_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (VwapParameters)CreateDefaultTestParameters();
        parameters.DeviationThresholdPercent = 60m; // Too high

        // Act
        var isValid = _algorithm.ValidateParameters(parameters, out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Deviation threshold", errorMessage);
    }

    [Fact]
    public void ValidateParameters_InvalidStopLoss_ShouldReturnFalse()
    {
        // Arrange
        var parameters = (VwapParameters)CreateDefaultTestParameters();
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
        Assert.Contains("VWAP", _algorithm.Name);
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
        Assert.Contains(definitions, p => p.Name == "Period");
        Assert.Contains(definitions, p => p.Name == "DeviationThresholdPercent");
        Assert.Contains(definitions, p => p.Name == "StopLossPercent");
        Assert.Contains(definitions, p => p.Name == "TakeProfitPercent");
    }

    #endregion
}