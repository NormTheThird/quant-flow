namespace QuantFlow.Common.Models.AlgorithmParameters;

/// <summary>
/// Parameters for Volume Weighted Average Price algorithm
/// </summary>
public class VwapParameters : BaseParameters
{
    /// <summary>
    /// VWAP calculation period
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// Percentage deviation threshold from VWAP to trigger signals
    /// </summary>
    public decimal DeviationThresholdPercent { get; set; }

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
            { nameof(Period), Period },
            { nameof(DeviationThresholdPercent), DeviationThresholdPercent },
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
        if (dict.TryGetValue(nameof(Period), out var period))
            Period = Convert.ToInt32(period);

        if (dict.TryGetValue(nameof(DeviationThresholdPercent), out var deviationThresholdPercent))
            DeviationThresholdPercent = Convert.ToDecimal(deviationThresholdPercent);

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