namespace QuantFlow.Common.Models.AlgorithmParameters;

/// <summary>
/// Parameters for Bollinger Bands Breakout algorithm
/// </summary>
public class BollingerBandsBreakoutParameters : BaseParameters
{
    /// <summary>
    /// Bollinger Bands period
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// Number of standard deviations for bands
    /// </summary>
    public decimal StandardDeviations { get; set; }

    /// <summary>
    /// Require momentum confirmation for breakout signals
    /// </summary>
    public bool RequireMomentumConfirmation { get; set; }

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
            { nameof(StandardDeviations), StandardDeviations },
            { nameof(RequireMomentumConfirmation), RequireMomentumConfirmation },
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

        if (dict.TryGetValue(nameof(StandardDeviations), out var standardDeviations))
            StandardDeviations = Convert.ToDecimal(standardDeviations);

        if (dict.TryGetValue(nameof(RequireMomentumConfirmation), out var requireMomentumConfirmation))
            RequireMomentumConfirmation = Convert.ToBoolean(requireMomentumConfirmation);

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