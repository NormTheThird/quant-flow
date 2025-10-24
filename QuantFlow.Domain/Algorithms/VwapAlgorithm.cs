namespace QuantFlow.Domain.Algorithms;

/// <summary>
/// Volume Weighted Average Price trading algorithm
/// Buys when price is below VWAP by threshold, sells when above VWAP by threshold
/// </summary>
public class VwapAlgorithm : ITradingAlgorithm
{
    private readonly ILogger<VwapAlgorithm> _logger;
    private readonly AverageTrueRangeIndicator _atrIndicator;

    public Guid AlgorithmId { get; set; }
    public string Name => HardCodedAlgorithmName.VolumeWeightedAveragePrice.GetDescription();
    public string Description => "Mean reversion strategy that buys when price drops below VWAP and sells when price rises above VWAP";
    public AlgorithmType Type => AlgorithmType.MeanReversion;
    public AlgorithmSource Source => AlgorithmSource.HardCoded;

    public VwapAlgorithm(
        ILogger<VwapAlgorithm> logger,
        AverageTrueRangeIndicator atrIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _atrIndicator = atrIndicator ?? throw new ArgumentNullException(nameof(atrIndicator));
    }

    public TradeSignalModel Analyze(MarketDataModel[] data, PositionModel? currentPosition, BaseParameters parameters)
    {
        if (parameters is not VwapParameters vwapParams)
            throw new ArgumentException("Invalid parameter type. Expected VwapParameters.", nameof(parameters));

        if (data == null || data.Length < vwapParams.Period)
        {
            _logger.LogWarning("Insufficient data for VWAP analysis");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient data" };
        }

        var currentBar = data[^1];

        // Calculate VWAP for the specified period
        var vwap = CalculateVWAP(data, vwapParams.Period);
        if (vwap == 0)
        {
            _logger.LogWarning("Unable to calculate VWAP");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Unable to calculate VWAP" };
        }

        // Calculate deviation from VWAP as percentage
        var deviationPercent = ((currentBar.Close - vwap) / vwap) * 100m;

        // Check for volume confirmation if required
        if (vwapParams.RequireVolumeConfirmation)
        {
            var avgVolume = data.TakeLast(20).Average(_ => _.Volume);
            var currentVolume = currentBar.Volume;

            if (currentVolume < avgVolume * vwapParams.VolumeMultiplier)
            {
                _logger.LogDebug("Volume confirmation failed. Current: {Current}, Required: {Required}",
                    currentVolume, avgVolume * vwapParams.VolumeMultiplier);
                return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient volume" };
            }
        }

        // BUY SIGNAL: Price is below VWAP by threshold percentage (oversold)
        if (deviationPercent <= -vwapParams.DeviationThresholdPercent && currentPosition == null)
        {
            var entryPrice = currentBar.Close;
            var stopLoss = CalculateStopLoss(entryPrice, data, vwapParams, isLong: true);
            var takeProfit = CalculateTakeProfit(entryPrice, vwapParams, isLong: true);

            _logger.LogInformation("Price below VWAP at {Price}. VWAP: {Vwap:F2}, Deviation: {Deviation:F2}%",
                entryPrice, vwap, deviationPercent);

            return new TradeSignalModel
            {
                Action = TradeSignal.Buy,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                PositionSizePercent = vwapParams.PositionSizePercent,
                Reason = $"Price below VWAP (Deviation: {deviationPercent:F2}%)",
                Confidence = 0.70m
            };
        }

        // SELL SIGNAL: Price is above VWAP by threshold percentage (overbought)
        if (deviationPercent >= vwapParams.DeviationThresholdPercent && currentPosition != null)
        {
            _logger.LogInformation("Price above VWAP at {Price}. VWAP: {Vwap:F2}, Deviation: {Deviation:F2}%",
                currentBar.Close, vwap, deviationPercent);

            return new TradeSignalModel
            {
                Action = TradeSignal.Sell,
                EntryPrice = currentBar.Close,
                Reason = $"Price above VWAP (Deviation: {deviationPercent:F2}%)",
                Confidence = 0.70m
            };
        }

        // HOLD
        return new TradeSignalModel
        {
            Action = TradeSignal.Hold,
            Reason = $"Price near VWAP (Deviation: {deviationPercent:F2}%)"
        };
    }

    public BaseParameters GetDefaultParameters()
    {
        return new VwapParameters();
    }

    public bool ValidateParameters(BaseParameters parameters, out string errorMessage)
    {
        if (parameters is not VwapParameters vwapParams)
        {
            errorMessage = "Invalid parameter type";
            return false;
        }

        if (vwapParams.Period <= 0)
        {
            errorMessage = "Period must be greater than 0";
            return false;
        }

        if (vwapParams.DeviationThresholdPercent <= 0 || vwapParams.DeviationThresholdPercent > 50)
        {
            errorMessage = "Deviation threshold must be between 0 and 50";
            return false;
        }

        if (vwapParams.StopLossPercent <= 0 || vwapParams.StopLossPercent > 100)
        {
            errorMessage = "Stop loss percent must be between 0 and 100";
            return false;
        }

        if (vwapParams.TakeProfitPercent <= 0 || vwapParams.TakeProfitPercent > 1000)
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
                Name = nameof(VwapParameters.Period),
                DisplayName = "VWAP Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 5,
                MaxValue = 50,
                Description = "Period for VWAP calculation (e.g., 14)",
                DisplayOrder = 1
            },
            new ParameterDefinition
            {
                Name = nameof(VwapParameters.DeviationThresholdPercent),
                DisplayName = "Deviation Threshold %",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 0.5m,
                MaxValue = 10.0m,
                Description = "Percentage deviation from VWAP to trigger signals (e.g., 2.0 = 2%)",
                DisplayOrder = 2
            },
            new ParameterDefinition
            {
                Name = nameof(VwapParameters.StopLossPercent),
                DisplayName = "Stop Loss %",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 0.1m,
                MaxValue = 50m,
                Description = "Stop loss percentage (e.g., 5.0 = 5% below entry)",
                DisplayOrder = 3
            },
            new ParameterDefinition
            {
                Name = nameof(VwapParameters.TakeProfitPercent),
                DisplayName = "Take Profit %",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 0.1m,
                MaxValue = 100m,
                Description = "Take profit percentage (e.g., 10.0 = 10% above entry)",
                DisplayOrder = 4
            },
            new ParameterDefinition
            {
                Name = nameof(VwapParameters.RequireVolumeConfirmation),
                DisplayName = "Require Volume Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = false,
                Description = "Require above-average volume for signals",
                DisplayOrder = 5
            },
            new ParameterDefinition
            {
                Name = nameof(VwapParameters.VolumeMultiplier),
                DisplayName = "Volume Multiplier",
                Type = ParameterType.Decimal,
                DefaultValue = null!,
                MinValue = 1.0m,
                MaxValue = 5.0m,
                Description = "Volume must be this multiple of average (e.g., 1.3 = 30% above average)",
                DisplayOrder = 6
            }
        };
    }

    private decimal CalculateVWAP(MarketDataModel[] data, int period)
    {
        var recentData = data.TakeLast(period).ToList();

        decimal totalPriceVolume = 0;
        decimal totalVolume = 0;

        foreach (var bar in recentData)
        {
            var typicalPrice = (bar.High + bar.Low + bar.Close) / 3m;
            totalPriceVolume += typicalPrice * bar.Volume;
            totalVolume += bar.Volume;
        }

        if (totalVolume == 0)
            return 0;

        var vwap = totalPriceVolume / totalVolume;

        _logger.LogDebug("Calculated VWAP({Period}) = {Vwap:F2}", period, vwap);
        return vwap;
    }

    private decimal CalculateStopLoss(decimal entryPrice, MarketDataModel[] data, VwapParameters parameters, bool isLong)
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

    private decimal CalculateTakeProfit(decimal entryPrice, VwapParameters parameters, bool isLong)
    {
        return isLong
            ? entryPrice * (1 + parameters.TakeProfitPercent / 100m)
            : entryPrice * (1 - parameters.TakeProfitPercent / 100m);
    }
}