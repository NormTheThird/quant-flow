namespace QuantFlow.Domain.Indicators;

/// <summary>
/// Relative Strength Index (RSI) indicator
/// </summary>
public class RelativeStrengthIndexIndicator
{
    private readonly ILogger<RelativeStrengthIndexIndicator> _logger;

    public RelativeStrengthIndexIndicator(ILogger<RelativeStrengthIndexIndicator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the RSI for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for RSI calculation (default 14)</param>
    /// <returns>RSI value (0-100), or null if insufficient data</returns>
    public decimal? Calculate(MarketDataModel[] data, int period = 14)
    {
        if (data == null || data.Length < period + 1)
        {
            _logger.LogWarning("Insufficient data for RSI calculation. Need {Period} candles, have {Count}", period + 1, data?.Length ?? 0);
            return null;
        }

        var gains = new List<decimal>();
        var losses = new List<decimal>();

        // Calculate price changes
        for (int i = 1; i < data.Length; i++)
        {
            var change = data[i].Close - data[i - 1].Close;
            gains.Add(change > 0 ? change : 0);
            losses.Add(change < 0 ? Math.Abs(change) : 0);
        }

        // Calculate average gain and loss over the period
        var avgGain = gains.TakeLast(period).Average();
        var avgLoss = losses.TakeLast(period).Average();

        // Calculate RSI
        if (avgLoss == 0)
            return 100m;

        var rs = avgGain / avgLoss;
        var rsi = 100m - (100m / (1m + rs));

        _logger.LogDebug("Calculated RSI({Period}) = {RSI:F2}", period, rsi);
        return rsi;
    }

    /// <summary>
    /// Calculates RSI series for all data points where calculation is possible
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="period">Period for RSI calculation (default 14)</param>
    /// <returns>Array of RSI values (null for points with insufficient data)</returns>
    public decimal?[] CalculateSeries(MarketDataModel[] data, int period = 14)
    {
        if (data == null || data.Length < period + 1)
        {
            _logger.LogWarning("Insufficient data for RSI series calculation");
            return Array.Empty<decimal?>();
        }

        var result = new decimal?[data.Length];

        for (int i = period; i < data.Length; i++)
        {
            var slice = data.Take(i + 1).ToArray();
            result[i] = Calculate(slice, period);
        }

        _logger.LogDebug("Calculated RSI({Period}) series with {Count} values", period, result.Count(_ => _.HasValue));
        return result;
    }
}