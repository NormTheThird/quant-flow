namespace QuantFlow.Domain.Algorithms;

/// <summary>
/// Bollinger Bands Breakout trading algorithm
/// Buys on lower band touch with momentum, sells on upper band touch
/// </summary>
public class BollingerBandsBreakoutAlgorithm : ITradingAlgorithm
{
    private readonly ILogger<BollingerBandsBreakoutAlgorithm> _logger;
    private readonly BollingerBandsIndicator _bbIndicator;
    private readonly AverageTrueRangeIndicator _atrIndicator;

    public Guid AlgorithmId { get; set; }
    public string Name => HardCodedAlgorithmName.BollingerBandsBreakout.GetDescription();
    public string Description => "Breakout strategy that buys when price touches lower Bollinger Band with momentum confirmation and sells on upper band touch";
    public AlgorithmType Type => AlgorithmType.Breakout;
    public AlgorithmSource Source => AlgorithmSource.HardCoded;

    public BollingerBandsBreakoutAlgorithm(
        ILogger<BollingerBandsBreakoutAlgorithm> logger,
        BollingerBandsIndicator bbIndicator,
        AverageTrueRangeIndicator atrIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bbIndicator = bbIndicator ?? throw new ArgumentNullException(nameof(bbIndicator));
        _atrIndicator = atrIndicator ?? throw new ArgumentNullException(nameof(atrIndicator));
    }

    public TradeSignalModel Analyze(MarketDataModel[] data, PositionModel? currentPosition, BaseParameters parameters)
    {
        if (parameters is not BollingerBandsBreakoutParameters bbParams)
            throw new ArgumentException("Invalid parameter type. Expected BollingerBandsBreakoutParameters.", nameof(parameters));

        if (data == null || data.Length < bbParams.Period + 1)
        {
            _logger.LogWarning("Insufficient data for Bollinger Bands Breakout analysis");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient data" };
        }

        var currentBar = data[^1];
        var previousBar = data[^2];

        // Calculate Bollinger Bands
        var currentBB = _bbIndicator.Calculate(data, bbParams.Period, bbParams.StandardDeviations);
        var previousData = data[..^1];
        var previousBB = _bbIndicator.Calculate(previousData, bbParams.Period, bbParams.StandardDeviations);

        if (currentBB == null || previousBB == null)
        {
            _logger.LogWarning("Unable to calculate Bollinger Bands");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Unable to calculate Bollinger Bands" };
        }

        // Check for volume confirmation if required
        if (bbParams.RequireVolumeConfirmation)
        {
            var avgVolume = data.TakeLast(20).Average(_ => _.Volume);
            var currentVolume = currentBar.Volume;

            if (currentVolume < avgVolume * bbParams.VolumeMultiplier)
            {
                _logger.LogDebug("Volume confirmation failed. Current: {Current}, Required: {Required}",
                    currentVolume, avgVolume * bbParams.VolumeMultiplier);
                return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient volume" };
            }
        }

        // Check for momentum confirmation if required
        bool hasMomentum = true;
        if (bbParams.RequireMomentumConfirmation)
        {
            // Simple momentum check: current close > previous close
            hasMomentum = currentBar.Close > previousBar.Close;
        }

        // BUY SIGNAL: Price touches or crosses below lower band with momentum
        if (previousBar.Low >= previousBB.LowerBand &&
            currentBar.Low <= currentBB.LowerBand &&
            currentPosition == null &&
            hasMomentum)
        {
            var entryPrice = currentBar.Close;
            var stopLoss = CalculateStopLoss(entryPrice, data, bbParams, isLong: true);
            var takeProfit = CalculateTakeProfit(entryPrice, bbParams, isLong: true);

            _logger.LogInformation("Bollinger Bands lower band touch at {Price}. Lower Band: {LowerBand:F2}",
                entryPrice, currentBB.LowerBand);

            return new TradeSignalModel
            {
                Action = TradeSignal.Buy,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                PositionSizePercent = bbParams.PositionSizePercent,
                Reason = $"Lower BB touch ({currentBB.LowerBand:F2}) with momentum",
                Confidence = 0.70m
            };
        }

        // SELL SIGNAL: Price touches or crosses above upper band
        if (previousBar.High <= previousBB.UpperBand &&
            currentBar.High >= currentBB.UpperBand &&
            currentPosition != null)
        {
            _logger.LogInformation("Bollinger Bands upper band touch at {Price}. Upper Band: {UpperBand:F2}",
                currentBar.Close, currentBB.UpperBand);

            return new TradeSignalModel
            {
                Action = TradeSignal.Sell,
                EntryPrice = currentBar.Close,
                Reason = $"Upper BB touch ({currentBB.UpperBand:F2})",
                Confidence = 0.70m
            };
        }

        // HOLD
        return new TradeSignalModel
        {
            Action = TradeSignal.Hold,
            Reason = "No Bollinger Band breakout detected"
        };
    }

    public BaseParameters GetDefaultParameters()
    {
        return new BollingerBandsBreakoutParameters();
    }

    public bool ValidateParameters(BaseParameters parameters, out string errorMessage)
    {
        if (parameters is not BollingerBandsBreakoutParameters bbParams)
        {
            errorMessage = "Invalid parameter type";
            return false;
        }

        if (bbParams.Period <= 0)
        {
            errorMessage = "Period must be greater than 0";
            return false;
        }

        if (bbParams.StandardDeviations <= 0 || bbParams.StandardDeviations > 5)
        {
            errorMessage = "Standard deviations must be between 0 and 5";
            return false;
        }

        if (bbParams.StopLossPercent <= 0 || bbParams.StopLossPercent > 100)
        {
            errorMessage = "Stop loss percent must be between 0 and 100";
            return false;
        }

        if (bbParams.TakeProfitPercent <= 0 || bbParams.TakeProfitPercent > 1000)
        {
            errorMessage = "Take profit percent must be between 0 and 1000";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public List<ParameterDefinition> GetParameterDefinitions()
    {
        return new List<ParameterDefinition>
        {
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.Period),
                DisplayName = "BB Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 5,
                MaxValue = 100,
                Description = "Period for Bollinger Bands calculation (e.g., 20)",
                DisplayOrder = 1
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.StandardDeviations),
                DisplayName = "Standard Deviations",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 1.0m,
                MaxValue = 3.0m,
                Description = "Number of standard deviations for bands (e.g., 2.0)",
                DisplayOrder = 2
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.RequireMomentumConfirmation),
                DisplayName = "Require Momentum Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = true,
                Description = "Require price momentum for breakout signals",
                DisplayOrder = 3
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.StopLossPercent),
                DisplayName = "Stop Loss %",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 0.1m,
                MaxValue = 50m,
                Description = "Stop loss percentage (e.g., 5.0 = 5% below entry)",
                DisplayOrder = 4
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.TakeProfitPercent),
                DisplayName = "Take Profit %",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 0.1m,
                MaxValue = 100m,
                Description = "Take profit percentage (e.g., 10.0 = 10% above entry)",
                DisplayOrder = 5
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.RequireVolumeConfirmation),
                DisplayName = "Require Volume Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = false,
                Description = "Require above-average volume for signals",
                DisplayOrder = 6
            },
            new ParameterDefinition
            {
                Name = nameof(BollingerBandsBreakoutParameters.VolumeMultiplier),
                DisplayName = "Volume Multiplier",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 1.0m,
                MaxValue = 5.0m,
                Description = "Volume must be this multiple of average (e.g., 1.3 = 30% above average)",
                DisplayOrder = 7
            }
        };
    }

    private decimal CalculateStopLoss(decimal entryPrice, MarketDataModel[] data, BollingerBandsBreakoutParameters parameters, bool isLong)
    {
        if (parameters.UseATRForStops)
        {
            var atr = _atrIndicator.Calculate(data, 14);
            if (atr.HasValue)
            {
                return isLong
                    ? entryPrice - (atr.Value * parameters.ATRMultiplier)
                    : entryPrice + (atr.Value * parameters.ATRMultiplier);
            }
        }

        return isLong
            ? entryPrice * (1 - parameters.StopLossPercent / 100m)
            : entryPrice * (1 + parameters.StopLossPercent / 100m);
    }

    private decimal CalculateTakeProfit(decimal entryPrice, BollingerBandsBreakoutParameters parameters, bool isLong)
    {
        return isLong
            ? entryPrice * (1 + parameters.TakeProfitPercent / 100m)
            : entryPrice * (1 - parameters.TakeProfitPercent / 100m);
    }
}