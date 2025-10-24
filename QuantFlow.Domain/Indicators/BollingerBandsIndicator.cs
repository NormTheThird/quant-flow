namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Bollinger Bands indicator result
/// </summary>
public class BollingerBandsResult
{
    public decimal UpperBand { get; set; }
    public decimal MiddleBand { get; set; }
    public decimal LowerBand { get; set; }
}

/// <summary>
/// Bollinger Bands indicator
/// </summary>
public class BollingerBandsIndicator
{
    private readonly ILogger<BollingerBandsIndicator> _logger;
    private readonly SimpleMovingAverageIndicator _smaIndicator;

    public BollingerBandsIndicator(
        ILogger<BollingerBandsIndicator> logger,
        SimpleMovingAverageIndicator smaIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smaIndicator = smaIndicator ?? throw new ArgumentNullException(nameof(smaIndicator));
    }

    /// <summary>
    /// Calculates Bollinger Bands for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for calculation (default 20)</param>
    /// <param name="standardDeviations">Number of standard deviations (default 2.0)</param>
    /// <returns>Bollinger Bands result, or null if insufficient data</returns>
    public BollingerBandsResult? Calculate(MarketDataModel[] data, int period = 20, decimal standardDeviations = 2.0m)
    {
        if (data == null || data.Length < period)
        {
            _logger.LogWarning("Insufficient data for Bollinger Bands calculation. Need {Period} candles, have {Count}", period, data?.Length ?? 0);
            return null;
        }

        // Calculate middle band (SMA)
        var middleBand = _smaIndicator.Calculate(data, period);
        if (!middleBand.HasValue)
            return null;

        // Calculate standard deviation
        var closePrices = data.TakeLast(period).Select(_ => _.Close).ToList();
        var variance = closePrices.Sum(_ => Math.Pow((double)(_ - middleBand.Value), 2)) / period;
        var standardDeviation = (decimal)Math.Sqrt(variance);

        // Calculate upper and lower bands
        var upperBand = middleBand.Value + (standardDeviations * standardDeviation);
        var lowerBand = middleBand.Value - (standardDeviations * standardDeviation);

        _logger.LogDebug("Calculated Bollinger Bands({Period},{StdDev}): Upper={Upper:F2}, Middle={Middle:F2}, Lower={Lower:F2}",
            period, standardDeviations, upperBand, middleBand.Value, lowerBand);

        return new BollingerBandsResult
        {
            UpperBand = upperBand,
            MiddleBand = middleBand.Value,
            LowerBand = lowerBand
        };
    }
}