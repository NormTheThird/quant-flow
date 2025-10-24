namespace QuantFlow.Domain.Indicators;

/// <summary>
/// MACD indicator result
/// </summary>
public class MacdResult
{
    public decimal MacdLine { get; set; }
    public decimal SignalLine { get; set; }
    public decimal Histogram { get; set; }
}

/// <summary>
/// Moving Average Convergence Divergence (MACD) indicator
/// </summary>
public class MacdIndicator
{
    private readonly ILogger<MacdIndicator> _logger;
    private readonly ExponentialMovingAverageIndicator _emaIndicator;

    public MacdIndicator(
        ILogger<MacdIndicator> logger,
        ExponentialMovingAverageIndicator emaIndicator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emaIndicator = emaIndicator ?? throw new ArgumentNullException(nameof(emaIndicator));
    }

    /// <summary>
    /// Calculates MACD for the most recent period
    /// </summary>
    /// <param name="data">Market data array</param>
    /// <param name="fastPeriod">Fast EMA period (default 12)</param>
    /// <param name="slowPeriod">Slow EMA period (default 26)</param>
    /// <param name="signalPeriod">Signal line period (default 9)</param>
    /// <returns>MACD result, or null if insufficient data</returns>
    public MacdResult? Calculate(MarketDataModel[] data, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        if (data == null || data.Length < slowPeriod + signalPeriod)
        {
            _logger.LogWarning("Insufficient data for MACD calculation. Need {Period} candles, have {Count}",
                slowPeriod + signalPeriod, data?.Length ?? 0);
            return null;
        }

        // Calculate fast and slow EMAs
        var fastEma = _emaIndicator.Calculate(data, fastPeriod);
        var slowEma = _emaIndicator.Calculate(data, slowPeriod);

        if (!fastEma.HasValue || !slowEma.HasValue)
            return null;

        // Calculate MACD line
        var macdLine = fastEma.Value - slowEma.Value;

        // Calculate signal line (EMA of MACD line)
        // We need to calculate MACD values for the signal period
        var macdValues = new List<decimal>();
        for (int i = slowPeriod - 1; i < data.Length; i++)
        {
            var slice = data.Take(i + 1).ToArray();
            var f = _emaIndicator.Calculate(slice, fastPeriod);
            var s = _emaIndicator.Calculate(slice, slowPeriod);
            if (f.HasValue && s.HasValue)
            {
                macdValues.Add(f.Value - s.Value);
            }
        }

        // Calculate signal line as EMA of MACD values
        if (macdValues.Count < signalPeriod)
            return null;

        var signalLine = CalculateEMA(macdValues.ToArray(), signalPeriod);
        var histogram = macdLine - signalLine;

        _logger.LogDebug("Calculated MACD({Fast},{Slow},{Signal}): MACD={Macd:F4}, Signal={Signal:F4}, Histogram={Histogram:F4}",
            fastPeriod, slowPeriod, signalPeriod, macdLine, signalLine, histogram);

        return new MacdResult
        {
            MacdLine = macdLine,
            SignalLine = signalLine,
            Histogram = histogram
        };
    }

    private decimal CalculateEMA(decimal[] values, int period)
    {
        var multiplier = 2.0m / (period + 1);
        var ema = values.Take(period).Average();

        for (int i = period; i < values.Length; i++)
        {
            ema = (values[i] - ema) * multiplier + ema;
        }

        return ema;
    }
}