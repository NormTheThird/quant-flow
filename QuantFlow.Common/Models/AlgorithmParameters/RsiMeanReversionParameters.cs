namespace QuantFlow.Common.Models.AlgorithmParameters;

/// <summary>
/// Parameters for RSI Mean Reversion algorithm
/// </summary>
public class RsiMeanReversionParameters : BaseParameters
{
    /// <summary>
    /// RSI calculation period
    /// </summary>
    public int RsiPeriod { get; set; }

    /// <summary>
    /// RSI threshold for oversold condition (buy signal)
    /// </summary>
    public decimal OversoldThreshold { get; set; }

    /// <summary>
    /// RSI threshold for overbought condition (sell signal)
    /// </summary>
    public decimal OverboughtThreshold { get; set; }

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
            { nameof(RsiPeriod), RsiPeriod },
            { nameof(OversoldThreshold), OversoldThreshold },
            { nameof(OverboughtThreshold), OverboughtThreshold },
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
        if (dict.TryGetValue(nameof(RsiPeriod), out var rsiPeriod))
            RsiPeriod = Convert.ToInt32(rsiPeriod);

        if (dict.TryGetValue(nameof(OversoldThreshold), out var oversoldThreshold))
            OversoldThreshold = Convert.ToDecimal(oversoldThreshold);

        if (dict.TryGetValue(nameof(OverboughtThreshold), out var overboughtThreshold))
            OverboughtThreshold = Convert.ToDecimal(overboughtThreshold);

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