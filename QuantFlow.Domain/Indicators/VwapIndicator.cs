namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Volume Weighted Average Price (VWAP) indicator
/// </summary>
public class VwapIndicator
{
    private readonly ILogger<VwapIndicator> _logger;

    public VwapIndicator(ILogger<VwapIndicator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates VWAP for the most recent period
    /// Uses typical price (high + low + close) / 3
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Number of periods to calculate VWAP (default uses all available data for intraday)</param>
    /// <returns>VWAP value, or null if insufficient data</returns>
    public decimal? Calculate(MarketDataModel[] data, int? period = null)
    {
        if (data == null || data.Length == 0)
        {
            _logger.LogWarning("Insufficient data for VWAP calculation");
            return null;
        }

        // Determine how many bars to use
        var barsToUse = period ?? data.Length;
        if (barsToUse > data.Length)
            barsToUse = data.Length;

        var relevantData = data.TakeLast(barsToUse).ToArray();

        decimal cumulativePV = 0m; // Price * Volume
        decimal cumulativeVolume = 0m;

        foreach (var bar in relevantData)
        {
            // Typical price = (High + Low + Close) / 3
            var typicalPrice = (bar.High + bar.Low + bar.Close) / 3m;
            var pv = typicalPrice * bar.Volume;

            cumulativePV += pv;
            cumulativeVolume += bar.Volume;
        }

        if (cumulativeVolume == 0)
        {
            _logger.LogWarning("Zero cumulative volume for VWAP calculation");
            return null;
        }

        var vwap = cumulativePV / cumulativeVolume;

        _logger.LogDebug("Calculated VWAP over {Period} bars = {Vwap:F2}", barsToUse, vwap);
        return vwap;
    }

    /// <summary>
    /// Calculates VWAP bands (standard deviation bands around VWAP)
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Number of periods to calculate VWAP</param>
    /// <param name="standardDeviations">Number of standard deviations for bands</param>
    /// <returns>Tuple of (VWAP, UpperBand, LowerBand), or null if insufficient data</returns>
    public (decimal Vwap, decimal UpperBand, decimal LowerBand)? CalculateWithBands(
        MarketDataModel[] data,
        int? period = null,
        decimal standardDeviations = 2.0m)
    {
        var vwap = Calculate(data, period);
        if (!vwap.HasValue)
            return null;

        var barsToUse = period ?? data.Length;
        if (barsToUse > data.Length)
            barsToUse = data.Length;

        var relevantData = data.TakeLast(barsToUse).ToArray();

        // Calculate standard deviation of typical prices
        var typicalPrices = relevantData.Select(bar => (bar.High + bar.Low + bar.Close) / 3m).ToArray();
        var variance = typicalPrices.Sum(price => Math.Pow((double)(price - vwap.Value), 2)) / typicalPrices.Length;
        var stdDev = (decimal)Math.Sqrt(variance);

        var upperBand = vwap.Value + (standardDeviations * stdDev);
        var lowerBand = vwap.Value - (standardDeviations * stdDev);

        _logger.LogDebug("Calculated VWAP Bands: VWAP={Vwap:F2}, Upper={Upper:F2}, Lower={Lower:F2}",
            vwap.Value, upperBand, lowerBand);

        return (vwap.Value, upperBand, lowerBand);
    }
}