namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Exponential Moving Average (EMA) indicator
/// </summary>
public class ExponentialMovingAverageIndicator
{
    private readonly ILogger<ExponentialMovingAverageIndicator> _logger;

    public ExponentialMovingAverageIndicator(ILogger<ExponentialMovingAverageIndicator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the EMA for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for EMA calculation</param>
    /// <returns>EMA value, or null if insufficient data</returns>
    public decimal? Calculate(MarketDataModel[] data, int period)
    {
        if (data == null || data.Length < period)
        {
            _logger.LogWarning("Insufficient data for EMA calculation. Need {Period} candles, have {Count}", period, data?.Length ?? 0);
            return null;
        }

        var multiplier = 2.0m / (period + 1);

        // Start with SMA for first EMA value
        var initialSma = data.Take(period).Select(_ => _.Close).Average();
        var ema = initialSma;

        // Calculate EMA for remaining data points
        for (int i = period; i < data.Length; i++)
        {
            var close = data[i].Close;
            ema = (close - ema) * multiplier + ema;
        }

        _logger.LogDebug("Calculated EMA({Period}) = {EMA}", period, ema);
        return ema;
    }

    /// <summary>
    /// Calculates EMA series for all data points where calculation is possible
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for EMA calculation</param>
    /// <returns>Array of EMA values (null for points with insufficient data)</returns>
    public decimal?[] CalculateSeries(MarketDataModel[] data, int period)
    {
        if (data == null || data.Length < period)
        {
            _logger.LogWarning("Insufficient data for EMA series calculation");
            return Array.Empty<decimal?>();
        }

        var result = new decimal?[data.Length];
        var multiplier = 2.0m / (period + 1);

        // First EMA value is SMA
        var initialSma = data.Take(period).Select(_ => _.Close).Average();
        result[period - 1] = initialSma;

        // Calculate remaining EMA values
        for (int i = period; i < data.Length; i++)
        {
            var close = data[i].Close;
            var previousEma = result[i - 1]!.Value;
            result[i] = (close - previousEma) * multiplier + previousEma;
        }

        _logger.LogDebug("Calculated EMA({Period}) series with {Count} values", period, result.Count(_ => _.HasValue));
        return result;
    }
}