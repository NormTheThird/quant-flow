namespace QuantFlow.Domain.Algorithms;

/// <summary>
/// Moving Average Crossover trading algorithm
/// Buys when fast MA crosses above slow MA, sells when fast crosses below slow MA
/// </summary>
public class MovingAverageCrossoverAlgorithm : ITradingAlgorithm
{
    private readonly ILogger<MovingAverageCrossoverAlgorithm> _logger;
    private readonly SimpleMovingAverageIndicator _smaIndicator;
    private readonly ExponentialMovingAverageIndicator _emaIndicator;
    private readonly AverageTrueRangeIndicator _atrIndicator;

    public Guid AlgorithmId { get; set; }
    public string Name => HardCodedAlgorithmName.MovingAverageCrossover.GetDescription();

    public string Description => "Classic trend following strategy that buys when fast MA crosses above slow MA (golden cross) and sells when fast crosses below slow MA (death cross)";
    public AlgorithmType Type => AlgorithmType.TrendFollowing;
    public AlgorithmSource Source => AlgorithmSource.HardCoded;

    public MovingAverageCrossoverAlgorithm(ILogger<MovingAverageCrossoverAlgorithm> logger, SimpleMovingAverageIndicator smaIndicator,
                                           ExponentialMovingAverageIndicator emaIndicator, AverageTrueRangeIndicator atrIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smaIndicator = smaIndicator ?? throw new ArgumentNullException(nameof(smaIndicator));
        _emaIndicator = emaIndicator ?? throw new ArgumentNullException(nameof(emaIndicator));
        _atrIndicator = atrIndicator ?? throw new ArgumentNullException(nameof(atrIndicator));
    }

    public TradeSignalModel Analyze(MarketDataModel[] data, PositionModel? currentPosition, BaseParameters parameters)
    {
        if (parameters is not MovingAverageCrossoverParameters maParams)
            throw new ArgumentException("Invalid parameter type. Expected MovingAverageCrossoverParameters.", nameof(parameters));

        if (data == null || data.Length < Math.Max(maParams.FastPeriod, maParams.SlowPeriod) + 1)
        {
            _logger.LogWarning("Insufficient data for MA Crossover analysis");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient data" };
        }

        var currentBar = data[^1];
        var previousData = data[..^1];

        // Calculate current and previous MAs
        decimal? currentFastMA, currentSlowMA, previousFastMA, previousSlowMA;

        if (maParams.MAType == MovingAverageType.SimpleMovingAverage)
        {
            currentFastMA = _smaIndicator.Calculate(data, maParams.FastPeriod);
            currentSlowMA = _smaIndicator.Calculate(data, maParams.SlowPeriod);
            previousFastMA = _smaIndicator.Calculate(previousData, maParams.FastPeriod);
            previousSlowMA = _smaIndicator.Calculate(previousData, maParams.SlowPeriod);
        }
        else
        {
            currentFastMA = _emaIndicator.Calculate(data, maParams.FastPeriod);
            currentSlowMA = _emaIndicator.Calculate(data, maParams.SlowPeriod);
            previousFastMA = _emaIndicator.Calculate(previousData, maParams.FastPeriod);
            previousSlowMA = _emaIndicator.Calculate(previousData, maParams.SlowPeriod);
        }

        if (!currentFastMA.HasValue || !currentSlowMA.HasValue || !previousFastMA.HasValue || !previousSlowMA.HasValue)
        {
            _logger.LogWarning("Unable to calculate moving averages");
            return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Unable to calculate MAs" };
        }

        // Check for volume confirmation if required
        if (maParams.RequireVolumeConfirmation)
        {
            var avgVolume = data.TakeLast(20).Average(_ => _.Volume);
            var currentVolume = currentBar.Volume;

            if (currentVolume < avgVolume * maParams.VolumeMultiplier)
            {
                _logger.LogDebug("Volume confirmation failed. Current: {Current}, Required: {Required}", currentVolume, avgVolume * maParams.VolumeMultiplier);
                return new TradeSignalModel { Action = TradeSignal.Hold, Reason = "Insufficient volume" };
            }
        }

        // BUY SIGNAL: Fast MA crosses above Slow MA (Golden Cross)
        if (previousFastMA.Value <= previousSlowMA.Value && currentFastMA.Value > currentSlowMA.Value && currentPosition == null)
        {
            var entryPrice = currentBar.Close;
            var stopLoss = CalculateStopLoss(entryPrice, data, maParams, isLong: true);
            var takeProfit = CalculateTakeProfit(entryPrice, maParams, isLong: true);

            _logger.LogInformation("Golden Cross detected at {Price}. Fast MA: {Fast}, Slow MA: {Slow}", entryPrice, currentFastMA.Value, currentSlowMA.Value);

            return new TradeSignalModel
            {
                Action = TradeSignal.Buy,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                PositionSizePercent = maParams.PositionSizePercent,
                Reason = $"Golden Cross: Fast MA ({currentFastMA.Value:F2}) crossed above Slow MA ({currentSlowMA.Value:F2})",
                Confidence = 0.75m
            };
        }

        // SELL SIGNAL: Fast MA crosses below Slow MA (Death Cross)
        if (previousFastMA.Value >= previousSlowMA.Value && currentFastMA.Value < currentSlowMA.Value && currentPosition != null)
        {
            _logger.LogInformation("Death Cross detected at {Price}. Fast MA: {Fast}, Slow MA: {Slow}", currentBar.Close, currentFastMA.Value, currentSlowMA.Value);

            return new TradeSignalModel
            {
                Action = TradeSignal.Sell,
                EntryPrice = currentBar.Close,
                Reason = $"Death Cross: Fast MA ({currentFastMA.Value:F2}) crossed below Slow MA ({currentSlowMA.Value:F2})",
                Confidence = 0.75m
            };
        }

        // HOLD
        return new TradeSignalModel
        {
            Action = TradeSignal.Hold,
            Reason = "No crossover detected"
        };
    }

    public BaseParameters GetDefaultParameters()
    {
        return new MovingAverageCrossoverParameters();
    }

    public bool ValidateParameters(BaseParameters parameters, out string errorMessage)
    {
        if (parameters is not MovingAverageCrossoverParameters maParams)
        {
            errorMessage = "Invalid parameter type";
            return false;
        }

        if (maParams.FastPeriod <= 0 || maParams.FastPeriod >= maParams.SlowPeriod)
        {
            errorMessage = "Fast period must be greater than 0 and less than slow period";
            return false;
        }

        if (maParams.SlowPeriod <= 0)
        {
            errorMessage = "Slow period must be greater than 0";
            return false;
        }

        if (maParams.StopLossPercent <= 0 || maParams.StopLossPercent > 100)
        {
            errorMessage = "Stop loss percent must be between 0 and 100";
            return false;
        }

        if (maParams.TakeProfitPercent <= 0 || maParams.TakeProfitPercent > 1000)
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
                Name = nameof(MovingAverageCrossoverParameters.FastPeriod),
                DisplayName = "Fast Period",
                Type = ParameterType.Integer,
                DefaultValue = 9,
                MinValue = 1,
                MaxValue = 200,
                Description = "Period for the fast moving average (e.g., 9)",
                DisplayOrder = 1
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.SlowPeriod),
                DisplayName = "Slow Period",
                Type = ParameterType.Integer,
                DefaultValue = 21,
                MinValue = 2,
                MaxValue = 500,
                Description = "Period for the slow moving average (e.g., 21)",
                DisplayOrder = 2
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.MAType),
                DisplayName = "Moving Average Type",
                Type = ParameterType.Enum,
                DefaultValue = (int)MovingAverageType.SimpleMovingAverage,
                Description = "Type of moving average calculation (SMA or EMA)",
                DisplayOrder = 3
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.StopLossPercent),
                DisplayName = "Stop Loss %",
                Type = ParameterType.Decimal,
                DefaultValue = 5.0m,
                MinValue = 0.1m,
                MaxValue = 50m,
                Description = "Stop loss percentage (e.g., 5.0 = 5% below entry)",
                DisplayOrder = 4
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.TakeProfitPercent),
                DisplayName = "Take Profit %",
                Type = ParameterType.Decimal,
                DefaultValue = 10.0m,
                MinValue = 0.1m,
                MaxValue = 100m,
                Description = "Take profit percentage (e.g., 10.0 = 10% above entry)",
                DisplayOrder = 5
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.RequireVolumeConfirmation),
                DisplayName = "Require Volume Confirmation",
                Type = ParameterType.Boolean,
                DefaultValue = false,
                Description = "Require above-average volume for signals",
                DisplayOrder = 6
            },
            new ParameterDefinition
            {
                Name = nameof(MovingAverageCrossoverParameters.VolumeMultiplier),
                DisplayName = "Volume Multiplier",
                Type = ParameterType.Decimal,
                DefaultValue = 1.3m,
                MinValue = 1.0m,
                MaxValue = 5.0m,
                Description = "Volume must be this multiple of average (e.g., 1.3 = 30% above average)",
                DisplayOrder = 7
            }
        };
    }

    private decimal CalculateStopLoss(decimal entryPrice, MarketDataModel[] data, MovingAverageCrossoverParameters parameters, bool isLong)
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

    private decimal CalculateTakeProfit(decimal entryPrice, MovingAverageCrossoverParameters parameters, bool isLong)
    {
        return isLong
            ? entryPrice * (1 + parameters.TakeProfitPercent / 100m)
            : entryPrice * (1 - parameters.TakeProfitPercent / 100m);
    }
}