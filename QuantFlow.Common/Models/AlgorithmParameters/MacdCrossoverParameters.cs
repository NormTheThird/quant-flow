namespace QuantFlow.Common.Models.AlgorithmParameters;

/// <summary>
/// Parameters for MACD Crossover algorithm
/// </summary>
public class MacdCrossoverParameters : BaseParameters
{
    /// <summary>
    /// Fast EMA period for MACD calculation
    /// </summary>
    public int FastPeriod { get; set; }

    /// <summary>
    /// Slow EMA period for MACD calculation
    /// </summary>
    public int SlowPeriod { get; set; }

    /// <summary>
    /// Signal line period
    /// </summary>
    public int SignalPeriod { get; set; }

    /// <summary>
    /// Require volume confirmation for signals
    /// </summary>
    public bool RequireVolumeConfirmation { get; set; }

    /// <summary>
    /// Volume multiplier threshold (e.g., 1.3 = 30% above average)
    /// </summary>
    public decimal VolumeMultiplier { get; set; }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { nameof(FastPeriod), FastPeriod },
            { nameof(SlowPeriod), SlowPeriod },
            { nameof(SignalPeriod), SignalPeriod },
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

        if (dict.TryGetValue(nameof(SignalPeriod), out var signalPeriod))
            SignalPeriod = Convert.ToInt32(signalPeriod);

        if (dict.TryGetValue(nameof(RequireVolumeConfirmation), out var requireVolumeConfirmation))
            RequireVolumeConfirmation = Convert.ToBoolean(requireVolumeConfirmation);

        if (dict.TryGetValue(nameof(VolumeMultiplier), out var volumeMultiplier))
            VolumeMultiplier = Convert.ToDecimal(volumeMultiplier);

        if (dict.TryGetValue(nameof(StopLossPercent), out var stopLossPercent))
            StopLossPercent = Convert.ToDecimal(stopLossPercent);

        if (dict.TryGetValue(nameof(TakeProfitPercent), out var takeProfitPercent))
            TakeProfitPercent = Convert.ToDecimal(takeProfitPercent);

        if (dict.TryGetValue(nameof(PositionSizePercent), out var positionSizePercent))
            PositionSizePercent = Convert.ToDecimal(positionSizePercent);

        if (dict.TryGetValue(nameof(UseATRForStops), out var useATRForStops))
            UseATRForStops = Convert.ToBoolean(useATRForStops);

        if (dict.TryGetValue(nameof(ATRMultiplier), out var atrMultiplier))
            ATRMultiplier = Convert.ToDecimal(atrMultiplier);
    }
}