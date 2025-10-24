namespace QuantFlow.Domain.Algorithms;

/// <summary>
/// RSI Mean Reversion trading algorithm
/// Buys when RSI crosses below oversold threshold, sells when RSI crosses above overbought threshold
/// </summary>
public class RsiMeanReversionAlgorithm : ITradingAlgorithm
{
    private readonly ILogger<RsiMeanReversionAlgorithm> _logger;
    private readonly RelativeStrengthIndexIndicator _rsiIndicator;
    private readonly AverageTrueRangeIndicator _atrIndicator;

    public Guid AlgorithmId { get; set; }
    public string Name => HardCodedAlgorithmName.RsiMeanReversion.GetDescription();
    public string Description => "Mean reversion strategy that buys when RSI crosses into oversold territory and sells when RSI crosses into overbought territory";
    public AlgorithmType Type => AlgorithmType.MeanReversion;
    public AlgorithmSource Source => AlgorithmSource.HardCoded;

    public RsiMeanReversionAlgorithm(
        ILogger<RsiMeanReversionAlgorithm> logger,
        RelativeStrengthIndexIndicator rsiIndicator,
        AverageTrueRangeIndicator atrIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rsiIndicator = rsiIndicator ?? throw new ArgumentNullException(nameof(rsiIndicator));
        _atrIndicator = atrIndicator ?? throw new ArgumentNullException(nameof(atrIndicator));
    }

    public TradeSignalModel Analyze(MarketDataModel[] data, PositionModel? currentPosition, BaseParameters parameters)
    {
        if (parameters is not RsiMeanReversionParameters rsiParams)
            throw new ArgumentException("Invalid parameter type. Expected RsiMeanReversionParameters.", nameof(parameters));

        if (data == null || data.Length < rsiParams.RsiPeriod + 1)
        {
            _logger.LogWarning("Insufficient data for RSI Mean Reversion analysis");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient data" };
        }

        var currentBar = data[^1];

        // Calculate current RSI
        var currentRSI = _rsiIndicator.Calculate(data, rsiParams.RsiPeriod);

        if (!currentRSI.HasValue)
        {
            _logger.LogWarning("Unable to calculate RSI");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Unable to calculate RSI" };
        }

        // Check for volume confirmation if required
        if (rsiParams.RequireVolumeConfirmation)
        {
            var avgVolume = data.TakeLast(20).Average(_ => _.Volume);
            var currentVolume = currentBar.Volume;

            if (currentVolume < avgVolume * rsiParams.VolumeMultiplier)
            {
                _logger.LogDebug("Volume confirmation failed. Current: {Current}, Required: {Required}",
                    currentVolume, avgVolume * rsiParams.VolumeMultiplier);
                return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient volume" };
            }
        }

        // BUY SIGNAL: RSI is in oversold zone (below threshold)
        if (currentRSI.Value < rsiParams.OversoldThreshold && currentPosition == null)
        {
            var entryPrice = currentBar.Close;
            var stopLoss = CalculateStopLoss(entryPrice, data, rsiParams, isLong: true);
            var takeProfit = CalculateTakeProfit(entryPrice, rsiParams, isLong: true);

            _logger.LogInformation("RSI Oversold signal at {Price}. RSI: {RSI:F2}", entryPrice, currentRSI.Value);

            return new TradeSignalModel
            {
                Action = TradeSignal.Buy,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                PositionSizePercent = rsiParams.PositionSizePercent,
                Reason = $"RSI in oversold zone ({currentRSI.Value:F2} < {rsiParams.OversoldThreshold})",
                Confidence = 0.70m
            };
        }

        // SELL SIGNAL: RSI is in overbought zone (above threshold) AND we have a position
        if (currentRSI.Value > rsiParams.OverboughtThreshold && currentPosition != null)
        {
            _logger.LogInformation("RSI Overbought signal at {Price}. RSI: {RSI:F2}", currentBar.Close, currentRSI.Value);

            return new TradeSignalModel
            {
                Action = TradeSignal.Sell,
                EntryPrice = currentBar.Close,
                Reason = $"RSI in overbought zone ({currentRSI.Value:F2} > {rsiParams.OverboughtThreshold})",
                Confidence = 0.70m
            };
        }

        // HOLD
        return new TradeSignalModel
        {
            Action = TradeSignal.Hold,
            Reason = $"RSI neutral (RSI: {currentRSI.Value:F2})"
        };
    }

    public BaseParameters GetDefaultParameters()
    {
        return new RsiMeanReversionParameters();
    }

    public bool ValidateParameters(BaseParameters parameters, out string errorMessage)
    {
        if (parameters is not RsiMeanReversionParameters rsiParams)
        {
            errorMessage = "Invalid parameter type";
            return false;
        }

        if (rsiParams.RsiPeriod <= 0)
        {
            errorMessage = "RSI period must be greater than 0";
            return false;
        }

        if (rsiParams.OversoldThreshold <= 0 || rsiParams.OversoldThreshold >= 50)
        {
            errorMessage = "Oversold threshold must be between 0 and 50";
            return false;
        }

        if (rsiParams.OverboughtThreshold <= 50 || rsiParams.OverboughtThreshold >= 100)
        {
            errorMessage = "Overbought threshold must be between 50 and 100";
            return false;
        }

        if (rsiParams.OversoldThreshold >= rsiParams.OverboughtThreshold)
        {
            errorMessage = "Oversold threshold must be less than overbought threshold";
            return false;
        }

        if (rsiParams.StopLossPercent <= 0 || rsiParams.StopLossPercent > 100)
        {
            errorMessage = "Stop loss percent must be between 0 and 100";
            return false;
        }

        if (rsiParams.TakeProfitPercent <= 0 || rsiParams.TakeProfitPercent > 1000)
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
                Name = nameof(RsiMeanReversionParameters.RsiPeriod),
                DisplayName = "RSI Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 2,
                MaxValue = 50,
                Description = "Period for RSI calculation (e.g., 14)",
                DisplayOrder = 1
            },
            new ParameterDefinition
            {
                Name = nameof(RsiMeanReversionParameters.OversoldThreshold),
                DisplayName = "Oversold Threshold",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 10m,
                MaxValue = 40m,
                Description = "RSI level considered oversold (e.g., 30)",
                DisplayOrder = 2
            },
            new ParameterDefinition
            {
                Name = nameof(RsiMeanReversionParameters.OverboughtThreshold),
                DisplayName = "Overbought Threshold",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 60m,
                MaxValue = 90m,
                Description = "RSI level considered overbought (e.g., 70)",
                DisplayOrder = 3
            },
            new ParameterDefinition
            {
                Name = nameof(RsiMeanReversionParameters.StopLossPercent),
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
                Name = nameof(RsiMeanReversionParameters.TakeProfitPercent),
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
                Name = nameof(RsiMeanReversionParameters.RequireVolumeConfirmation),
                DisplayName = "Require Volume Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = false,
                Description = "Require above-average volume for signals",
                DisplayOrder = 6
            },
            new ParameterDefinition
            {
                Name = nameof(RsiMeanReversionParameters.VolumeMultiplier),
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

    private decimal CalculateStopLoss(decimal entryPrice, MarketDataModel[] data, RsiMeanReversionParameters parameters, bool isLong)
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

    private decimal CalculateTakeProfit(decimal entryPrice, RsiMeanReversionParameters parameters, bool isLong)
    {
        return isLong
            ? entryPrice * (1 + parameters.TakeProfitPercent / 100m)
            : entryPrice * (1 - parameters.TakeProfitPercent / 100m);
    }
}