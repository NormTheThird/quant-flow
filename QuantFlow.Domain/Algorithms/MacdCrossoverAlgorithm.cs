namespace QuantFlow.Domain.Algorithms;

/// <summary>
/// MACD Crossover trading algorithm
/// Buys when MACD line crosses above signal line, sells when crosses below
/// </summary>
public class MacdCrossoverAlgorithm : ITradingAlgorithm
{
    private readonly ILogger<MacdCrossoverAlgorithm> _logger;
    private readonly MacdIndicator _macdIndicator;
    private readonly AverageTrueRangeIndicator _atrIndicator;

    public Guid AlgorithmId { get; set; }
    public string Name => HardCodedAlgorithmName.MacdCrossover.GetDescription();
    public string Description => "Trend following strategy that buys when MACD line crosses above signal line and sells when MACD crosses below signal line";
    public AlgorithmType Type => AlgorithmType.TrendFollowing;
    public AlgorithmSource Source => AlgorithmSource.HardCoded;

    public MacdCrossoverAlgorithm(
        ILogger<MacdCrossoverAlgorithm> logger,
        MacdIndicator macdIndicator,
        AverageTrueRangeIndicator atrIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _macdIndicator = macdIndicator ?? throw new ArgumentNullException(nameof(macdIndicator));
        _atrIndicator = atrIndicator ?? throw new ArgumentNullException(nameof(atrIndicator));
    }

    public TradeSignalModel Analyze(MarketDataModel[] data, PositionModel? currentPosition, BaseParameters parameters)
    {
        if (parameters is not MacdCrossoverParameters macdParams)
            throw new ArgumentException("Invalid parameter type. Expected MacdCrossoverParameters.", nameof(parameters));

        var minDataRequired = Math.Max(macdParams.SlowPeriod, macdParams.FastPeriod) + macdParams.SignalPeriod + 1;
        if (data == null || data.Length < minDataRequired)
        {
            _logger.LogWarning("Insufficient data for MACD Crossover analysis");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient data" };
        }

        var currentBar = data[^1];
        var previousData = data[..^1];

        // Calculate current and previous MACD
        var currentMACD = _macdIndicator.Calculate(data, macdParams.FastPeriod, macdParams.SlowPeriod, macdParams.SignalPeriod);
        var previousMACD = _macdIndicator.Calculate(previousData, macdParams.FastPeriod, macdParams.SlowPeriod, macdParams.SignalPeriod);

        if (currentMACD == null || previousMACD == null)
        {
            _logger.LogWarning("Unable to calculate MACD");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Unable to calculate MACD" };
        }

        // Check for volume confirmation if required
        if (macdParams.RequireVolumeConfirmation)
        {
            var avgVolume = data.TakeLast(20).Average(_ => _.Volume);
            var currentVolume = currentBar.Volume;

            if (currentVolume < avgVolume * macdParams.VolumeMultiplier)
            {
                _logger.LogDebug("Volume confirmation failed. Current: {Current}, Required: {Required}",
                    currentVolume, avgVolume * macdParams.VolumeMultiplier);
                return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient volume" };
            }
        }

        // BUY SIGNAL: MACD line crosses above signal line (bullish crossover)
        if (previousMACD.MacdLine <= previousMACD.SignalLine &&
            currentMACD.MacdLine > currentMACD.SignalLine &&
            currentPosition == null)
        {
            var entryPrice = currentBar.Close;
            var stopLoss = CalculateStopLoss(entryPrice, data, macdParams, isLong: true);
            var takeProfit = CalculateTakeProfit(entryPrice, macdParams, isLong: true);

            _logger.LogInformation("MACD bullish crossover at {Price}. MACD: {Macd:F4}, Signal: {Signal:F4}",
                entryPrice, currentMACD.MacdLine, currentMACD.SignalLine);

            return new TradeSignalModel
            {
                Action = TradeSignal.Buy,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                PositionSizePercent = macdParams.PositionSizePercent,
                Reason = $"MACD bullish crossover (MACD: {currentMACD.MacdLine:F4} > Signal: {currentMACD.SignalLine:F4})",
                Confidence = 0.75m
            };
        }

        // SELL SIGNAL: MACD line crosses below signal line (bearish crossover)
        if (previousMACD.MacdLine >= previousMACD.SignalLine &&
            currentMACD.MacdLine < currentMACD.SignalLine &&
            currentPosition != null)
        {
            _logger.LogInformation("MACD bearish crossover at {Price}. MACD: {Macd:F4}, Signal: {Signal:F4}",
                currentBar.Close, currentMACD.MacdLine, currentMACD.SignalLine);

            return new TradeSignalModel
            {
                Action = TradeSignal.Sell,
                EntryPrice = currentBar.Close,
                Reason = $"MACD bearish crossover (MACD: {currentMACD.MacdLine:F4} < Signal: {currentMACD.SignalLine:F4})",
                Confidence = 0.75m
            };
        }

        // HOLD
        return new TradeSignalModel
        {
            Action = TradeSignal.Hold,
            Reason = "No MACD crossover detected"
        };
    }

    public BaseParameters GetDefaultParameters()
    {
        return new MacdCrossoverParameters();
    }

    public bool ValidateParameters(BaseParameters parameters, out string errorMessage)
    {
        if (parameters is not MacdCrossoverParameters macdParams)
        {
            errorMessage = "Invalid parameter type";
            return false;
        }

        if (macdParams.FastPeriod <= 0)
        {
            errorMessage = "Fast period must be greater than 0";
            return false;
        }

        if (macdParams.SlowPeriod <= 0 || macdParams.SlowPeriod <= macdParams.FastPeriod)
        {
            errorMessage = "Slow period must be greater than fast period";
            return false;
        }

        if (macdParams.SignalPeriod <= 0)
        {
            errorMessage = "Signal period must be greater than 0";
            return false;
        }

        if (macdParams.StopLossPercent <= 0 || macdParams.StopLossPercent > 100)
        {
            errorMessage = "Stop loss percent must be between 0 and 100";
            return false;
        }

        if (macdParams.TakeProfitPercent <= 0 || macdParams.TakeProfitPercent > 1000)
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
                Name = nameof(MacdCrossoverParameters.FastPeriod),
                DisplayName = "Fast EMA Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 5,
                MaxValue = 50,
                Description = "Fast EMA period for MACD (e.g., 12)",
                DisplayOrder = 1
            },
            new ParameterDefinition
            {
                Name = nameof(MacdCrossoverParameters.SlowPeriod),
                DisplayName = "Slow EMA Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 10,
                MaxValue = 100,
                Description = "Slow EMA period for MACD (e.g., 26)",
                DisplayOrder = 2
            },
            new ParameterDefinition
            {
                Name = nameof(MacdCrossoverParameters.SignalPeriod),
                DisplayName = "Signal Period",
                Type = ParameterType.Integer,
                DefaultValue = null!,
                MinValue = 3,
                MaxValue = 30,
                Description = "Signal line period (e.g., 9)",
                DisplayOrder = 3
            },
            new ParameterDefinition
            {
                Name = nameof(MacdCrossoverParameters.StopLossPercent),
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
                Name = nameof(MacdCrossoverParameters.TakeProfitPercent),
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
                Name = nameof(MacdCrossoverParameters.RequireVolumeConfirmation),
                DisplayName = "Require Volume Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = false,
                Description = "Require above-average volume for signals",
                DisplayOrder = 6
            },
            new ParameterDefinition
            {
                Name = nameof(MacdCrossoverParameters.VolumeMultiplier),
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

    private decimal CalculateStopLoss(decimal entryPrice, MarketDataModel[] data, MacdCrossoverParameters parameters, bool isLong)
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

    private decimal CalculateTakeProfit(decimal entryPrice, MacdCrossoverParameters parameters, bool isLong)
    {
        return isLong
            ? entryPrice * (1 + parameters.TakeProfitPercent / 100m)
            : entryPrice * (1 - parameters.TakeProfitPercent / 100m);
    }
}