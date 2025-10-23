namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Simple Moving Average (SMA) indicator
/// </summary>
public class SimpleMovingAverageIndicator
{
    private readonly ILogger<SimpleMovingAverageIndicator> _logger;

    public SimpleMovingAverageIndicator(ILogger<SimpleMovingAverageIndicator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the SMA for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for SMA calculation</param>
    /// <returns>SMA value, or null if insufficient data</returns>
    public decimal? Calculate(MarketDataModel[] data, int period)
    {
        if (data == null || data.Length < period)
        {
            _logger.LogWarning("Insufficient data for SMA calculation. Need {Period} candles, have {Count}", period, data?.Length ?? 0);
            return null;
        }

        var closePrices = data.TakeLast(period).Select(_ => _.Close);
        var sma = closePrices.Average();

        _logger.LogDebug("Calculated SMA({Period}) = {SMA}", period, sma);
        return sma;
    }

    /// <summary>
    /// Calculates SMA series for all data points where calculation is possible
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for SMA calculation</param>
    /// <returns>Array of SMA values (null for points with insufficient data)</returns>
    public decimal?[] CalculateSeries(MarketDataModel[] data, int period)
    {
        if (data == null || data.Length < period)
        {
            _logger.LogWarning("Insufficient data for SMA series calculation");
            return Array.Empty<decimal?>();
        }

        var result = new decimal?[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            if (i < period - 1)
            {
                result[i] = null;
                continue;
            }

            var slice = data.Skip(i - period + 1).Take(period).Select(_ => _.Close);
            result[i] = slice.Average();
        }

        _logger.LogDebug("Calculated SMA({Period}) series with {Count} values", period, result.Count(_ => _.HasValue));
        return result;
    }
}