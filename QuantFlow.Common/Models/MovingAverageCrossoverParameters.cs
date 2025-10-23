namespace QuantFlow.Common.Models;

/// <summary>
/// Parameters for Moving Average Crossover algorithm
/// </summary>
public class MovingAverageCrossoverParameters : AlgorithmParameters
{
    /// <summary>
    /// Fast moving average period (default 9)
    /// </summary>
    public int FastPeriod { get; set; } = 9;

    /// <summary>
    /// Slow moving average period (default 21)
    /// </summary>
    public int SlowPeriod { get; set; } = 21;

    /// <summary>
    /// Type of moving average to use (SMA or EMA)
    /// </summary>
    public MovingAverageType MAType { get; set; } = MovingAverageType.SimpleMovingAverage;

    /// <summary>
    /// Require volume confirmation for signals
    /// </summary>
    public bool RequireVolumeConfirmation { get; set; } = false;

    /// <summary>
    /// Volume multiplier threshold (e.g., 1.3 = 30% above average)
    /// </summary>
    public decimal VolumeMultiplier { get; set; } = 1.3m;

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { nameof(FastPeriod), FastPeriod },
            { nameof(SlowPeriod), SlowPeriod },
            { nameof(MAType), (int)MAType },
            { nameof(RequireVolumeConfirmation), RequireVolumeConfirmation },
            { nameof(VolumeMultiplier), VolumeMultiplier },
            { nameof(StopLossPercent), StopLossPercent },
            { nameof(TakeProfitPercent), TakeProfitPercent },
            { nameof(PositionSizePercent), PositionSizePercent },
            { nameof(UseATRForStops), UseATRForStops },
            { nameof(ATRMultiplier), ATRMultiplier }
        };
    }

    public override void FromDictionary(Dictionary<string, object> dict)
    {
        if (dict.TryGetValue(nameof(FastPeriod), out var fastPeriod))
            FastPeriod = Convert.ToInt32(fastPeriod);

        if (dict.TryGetValue(nameof(SlowPeriod), out var slowPeriod))
            SlowPeriod = Convert.ToInt32(slowPeriod);

        if (dict.TryGetValue(nameof(MAType), out var maType))
            MAType = (MovingAverageType)Convert.ToInt32(maType);

        if (dict.TryGetValue(nameof(RequireVolumeConfirmation), out var volumeConf))
            RequireVolumeConfirmation = Convert.ToBoolean(volumeConf);

        if (dict.TryGetValue(nameof(VolumeMultiplier), out var volumeMult))
            VolumeMultiplier = Convert.ToDecimal(volumeMult);

        if (dict.TryGetValue(nameof(StopLossPercent), out var stopLoss))
            StopLossPercent = Convert.ToDecimal(stopLoss);

        if (dict.TryGetValue(nameof(TakeProfitPercent), out var takeProfit))
            TakeProfitPercent = Convert.ToDecimal(takeProfit);

        if (dict.TryGetValue(nameof(PositionSizePercent), out var positionSize))
            PositionSizePercent = Convert.ToDecimal(positionSize);

        if (dict.TryGetValue(nameof(UseATRForStops), out var useATR))
            UseATRForStops = Convert.ToBoolean(useATR);

        if (dict.TryGetValue(nameof(ATRMultiplier), out var atrMult))
            ATRMultiplier = Convert.ToDecimal(atrMult);
    }
}