namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Average True Range (ATR) indicator
/// </summary>
public class AverageTrueRangeIndicator
{
    private readonly ILogger<AverageTrueRangeIndicator> _logger;

    public AverageTrueRangeIndicator(ILogger<AverageTrueRangeIndicator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the ATR for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for ATR calculation (default 14)</param>
    /// <returns>ATR value, or null if insufficient data</returns>
    public decimal? Calculate(MarketDataModel[] data, int period = 14)
    {
        if (data == null || data.Length < period + 1)
        {
            _logger.LogWarning("Insufficient data for ATR calculation. Need {Period} candles, have {Count}", period + 1, data?.Length ?? 0);
            return null;
        }

        var trueRanges = new List<decimal>();

        // Calculate True Range for each candle
        for (int i = 1; i < data.Length; i++)
        {
            var high = data[i].High;
            var low = data[i].Low;
            var previousClose = data[i - 1].Close;

            var tr1 = high - low;
            var tr2 = Math.Abs(high - previousClose);
            var tr3 = Math.Abs(low - previousClose);

            var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
            trueRanges.Add(trueRange);
        }

        // Calculate ATR as average of last N true ranges
        var atr = trueRanges.TakeLast(period).Average();

        _logger.LogDebug("Calculated ATR({Period}) = {ATR:F4}", period, atr);
        return atr;
    }

    /// <summary>
    /// Calculates ATR series for all data points where calculation is possible
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for ATR calculation (default 14)</param>
    /// <returns>Array of ATR values (null for points with insufficient data)</returns>
    public decimal?[] CalculateSeries(MarketDataModel[] data, int period = 14)
    {
        if (data == null || data.Length < period + 1)
        {
            _logger.LogWarning("Insufficient data for ATR series calculation");
            return Array.Empty<decimal?>();
        }

        var result = new decimal?[data.Length];

        for (int i = period; i < data.Length; i++)
        {
            var slice = data.Take(i + 1).ToArray();
            result[i] = Calculate(slice, period);
        }

        _logger.LogDebug("Calculated ATR({Period}) series with {Count} values", period, result.Count(_ => _.HasValue));
        return result;
    }
}