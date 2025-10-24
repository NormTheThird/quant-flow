namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture for generating realistic market data for testing algorithms
/// </summary>
public static class MarketDataFixture
{
    /// <summary>
    /// Creates a trending upward market (bull trend)
    /// </summary>
    public static MarketDataModel[] CreateBullTrend(int bars = 50, decimal startPrice = 100m, decimal priceIncrease = 0.5m)
    {
        var data = new List<MarketDataModel>();
        var currentPrice = startPrice;
        var baseTime = DateTime.UtcNow.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 2 - 1); // -1 to +1
            var open = currentPrice;
            var close = currentPrice + priceIncrease + (noise * 0.3m);
            var high = Math.Max(open, close) + Math.Abs(noise * 0.5m);
            var low = Math.Min(open, close) - Math.Abs(noise * 0.3m);
            var volume = 1000000m + (decimal)(new Random(i + 1000).NextDouble() * 500000);

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates a trending downward market (bear trend)
    /// </summary>
    public static MarketDataModel[] CreateBearTrend(int bars = 50, decimal startPrice = 100m, decimal priceDecrease = 0.5m)
    {
        var data = new List<MarketDataModel>();
        var currentPrice = startPrice;
        var baseTime = DateTime.UtcNow.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 2 - 1);
            var open = currentPrice;
            var close = currentPrice - priceDecrease + (noise * 0.3m);
            var high = Math.Max(open, close) + Math.Abs(noise * 0.3m);
            var low = Math.Min(open, close) - Math.Abs(noise * 0.5m);
            var volume = 1000000m + (decimal)(new Random(i + 1000).NextDouble() * 500000);

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates a sideways/ranging market
    /// </summary>
    public static MarketDataModel[] CreateSidewaysMarket(int bars = 50, decimal centerPrice = 100m, decimal range = 5m)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            var random = new Random(i);
            var oscillation = (decimal)Math.Sin(i * 0.3) * range;
            var noise = (decimal)(random.NextDouble() * 2 - 1);

            var close = centerPrice + oscillation + (noise * 0.5m);
            var open = close + (noise * 0.3m);
            var high = Math.Max(open, close) + Math.Abs(noise * 0.4m);
            var low = Math.Min(open, close) - Math.Abs(noise * 0.4m);
            var volume = 1000000m + (decimal)(random.NextDouble() * 300000);

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume,
                TradeCount = 1000
            });
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates a golden cross scenario (fast MA crosses above slow MA)
    /// </summary>
    public static MarketDataModel[] CreateGoldenCross(int barsBefore = 20, int barsAfter = 10)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-(barsBefore + barsAfter));
        var currentPrice = 100m;

        // Downtrend before cross
        for (int i = 0; i < barsBefore; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 0.3m;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Strong uptrend after cross
        for (int i = 0; i < barsAfter; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.8m;
            var high = close + 0.3m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(barsBefore + i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1500000m, // Higher volume on breakout
                TradeCount = 1500
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates a death cross scenario (fast MA crosses below slow MA)
    /// </summary>
    public static MarketDataModel[] CreateDeathCross(int barsBefore = 30, int barsAfter = 10)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-(barsBefore + barsAfter));
        var currentPrice = 100m;

        // Strong uptrend before cross (need enough bars for 21-period MA)
        for (int i = 0; i < barsBefore; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.5m; // Stronger uptrend
            var high = close + 0.2m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Strong downtrend after cross
        for (int i = 0; i < barsAfter; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 1.2m; // Aggressive downtrend
            var high = open + 0.1m;
            var low = close - 0.3m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(barsBefore + i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1500000m,
                TradeCount = 1500
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates RSI oversold scenario (RSI drops below 30)
    /// </summary>
    public static MarketDataModel[] CreateRsiOversold(int bars = 35)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Need stable period first for RSI calculation
        for (int i = 0; i < 15; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 0.4 - 0.2);
            var open = currentPrice;
            var close = currentPrice + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Aggressive selling pressure to create oversold RSI
        for (int i = 15; i < bars - 3; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 2.5m; // Strong selling
            var high = open + 0.2m;
            var low = close - 0.5m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1500000m,
                TradeCount = 1500
            });

            currentPrice = close;
        }

        // Small recovery at end
        for (int i = bars - 3; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.3m;
            var high = close + 0.2m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1200000m,
                TradeCount = 1200
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates RSI overbought scenario (RSI rises above 70)
    /// </summary>
    public static MarketDataModel[] CreateRsiOverbought(int bars = 35)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Need stable period first for RSI calculation
        for (int i = 0; i < 15; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 0.4 - 0.2);
            var open = currentPrice;
            var close = currentPrice + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Aggressive buying pressure to create overbought RSI
        for (int i = 15; i < bars - 3; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 2.5m; // Strong buying
            var high = close + 0.5m;
            var low = open - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1500000m,
                TradeCount = 1500
            });

            currentPrice = close;
        }

        // Small pullback at end
        for (int i = bars - 3; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 0.3m;
            var high = open + 0.1m;
            var low = close - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1200000m,
                TradeCount = 1200
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates Bollinger upper band breakout scenario (price breaks above upper band)
    /// </summary>
    public static MarketDataModel[] CreateBollingerUpperBreakout(int bars = 40)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Stable period to establish bands
        for (int i = 0; i < 25; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 0.8 - 0.4);
            var open = currentPrice;
            var close = currentPrice + noise;
            var high = Math.Max(open, close) + 0.3m;
            var low = Math.Min(open, close) - 0.3m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Strong breakout above upper band
        for (int i = 25; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 1.5m; // Strong upward movement
            var high = close + 0.5m;
            var low = open - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1500000m, // High volume on breakout
                TradeCount = 1500
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates Bollinger lower band touch scenario (price touches lower band)
    /// </summary>
    public static MarketDataModel[] CreateBollingerLowerBreakdown(int bars = 40)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Create stable sideways market to establish bands around 100
        for (int i = 0; i < 25; i++)
        {
            var noise = (decimal)(new Random(i * 42).NextDouble() * 0.6 - 0.3);
            var open = 100m;
            var close = 100m + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });
        }

        // Add a few more stable bars
        for (int i = 25; i < 30; i++)
        {
            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = 100m,
                High = 100.5m,
                Low = 99.5m,
                Close = 100m,
                Volume = 1000000m,
                TradeCount = 1000
            });
        }

        // Now create a bar that drops to lower band
        // With 20 period and 2 std dev, lower band should be around 98-99
        // So let's create a bar with low around 96 to ensure we touch it
        data.Add(new MarketDataModel
        {
            Symbol = "BTCUSD",
            Exchange = Exchange.Kraken,
            Timeframe = Timeframe.OneHour,
            Timestamp = baseTime.AddHours(30),
            Open = 100m,
            High = 100.2m,
            Low = 96m, // This should definitely touch lower band
            Close = 96.5m,
            Volume = 1500000m,
            TradeCount = 1500
        });

        // Add recovery bars with upward momentum
        currentPrice = 96.5m;
        for (int i = 31; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.5m; // Gradual recovery
            var high = close + 0.2m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1200000m,
                TradeCount = 1200
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates Bollinger Band squeeze and breakout scenario
    /// </summary>
    public static MarketDataModel[] CreateBollingerBandBreakout(int barsConsolidation = 20, int barsBreakout = 10)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-(barsConsolidation + barsBreakout));
        var centerPrice = 100m;

        // Consolidation phase (tight range)
        for (int i = 0; i < barsConsolidation; i++)
        {
            var noise = (decimal)(new Random(i).NextDouble() * 0.6 - 0.3);
            var close = centerPrice + noise;
            var open = close + (noise * 0.5m);
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 800000m, // Low volume during consolidation
                TradeCount = 800
            });
        }

        // Breakout phase (strong move down to lower band then reversal)
        var currentPrice = centerPrice;
        for (int i = 0; i < barsBreakout; i++)
        {
            var open = currentPrice;
            var close = currentPrice + (i < 3 ? -2m : 1.5m); // Drop then recovery
            var high = Math.Max(open, close) + 0.5m;
            var low = Math.Min(open, close) - 0.5m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(barsConsolidation + i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 2000000m, // High volume on breakout
                TradeCount = 2000
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates insufficient data scenario for testing error handling
    /// </summary>
    public static MarketDataModel[] CreateInsufficientData(int bars = 5)
    {
        return CreateBullTrend(bars);
    }

    /// <summary>
    /// Creates MACD bullish crossover scenario (MACD line crosses above signal line)
    /// </summary>
    public static MarketDataModel[] CreateMacdBullishCrossover(int bars = 60)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Create steady downtrend for 35 bars (MACD will be strongly negative)
        for (int i = 0; i < 35; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 0.5m; // Stronger decline
            var high = open + 0.1m;
            var low = close - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Strong reversal - sharp uptrend (creates bullish crossover)
        for (int i = 35; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 1.2m; // Very strong upward movement
            var high = close + 0.4m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1800000m, // High volume
                TradeCount = 1800
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates MACD bearish crossover scenario (MACD line crosses below signal line)
    /// </summary>
    public static MarketDataModel[] CreateMacdBearishCrossover(int bars = 60)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Create steady uptrend for 35 bars (MACD will be strongly positive)
        for (int i = 0; i < 35; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.5m; // Stronger rise
            var high = close + 0.2m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });

            currentPrice = close;
        }

        // Strong reversal - sharp downtrend (creates bearish crossover)
        for (int i = 35; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 1.2m; // Very strong downward movement
            var high = open + 0.1m;
            var low = close - 0.4m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1800000m, // High volume
                TradeCount = 1800
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates scenario where price drops significantly below VWAP
    /// </summary>
    public static MarketDataModel[] CreateVwapBelowScenario(int bars = 30)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Create stable price period (VWAP will be around 100)
        for (int i = 0; i < 15; i++)
        {
            var noise = (decimal)(new Random(i * 37).NextDouble() * 0.4 - 0.2);
            var open = 100m;
            var close = 100m + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });
        }

        // Price drops significantly below VWAP
        currentPrice = 100m;
        for (int i = 15; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice - 0.6m; // Strong decline
            var high = open + 0.1m;
            var low = close - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1200000m,
                TradeCount = 1200
            });

            currentPrice = close;
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates scenario where price is only slightly below VWAP
    /// </summary>
    public static MarketDataModel[] CreateVwapSlightlyBelowScenario(int bars = 30)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);

        // Create stable price around 100
        for (int i = 0; i < bars; i++)
        {
            var noise = (decimal)(new Random(i * 41).NextDouble() * 0.6 - 0.35); // Slightly below average
            var open = 100m;
            var close = 100m + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });
        }

        return data.ToArray();
    }

    /// <summary>
    /// Creates scenario where price rises significantly above VWAP
    /// </summary>
    public static MarketDataModel[] CreateVwapAboveScenario(int bars = 30)
    {
        var data = new List<MarketDataModel>();
        var baseTime = DateTime.UtcNow.AddDays(-bars);
        var currentPrice = 100m;

        // Create stable price period (VWAP will be around 100)
        for (int i = 0; i < 15; i++)
        {
            var noise = (decimal)(new Random(i * 43).NextDouble() * 0.4 - 0.2);
            var open = 100m;
            var close = 100m + noise;
            var high = Math.Max(open, close) + 0.2m;
            var low = Math.Min(open, close) - 0.2m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000000m,
                TradeCount = 1000
            });
        }

        // Price rises significantly above VWAP
        currentPrice = 100m;
        for (int i = 15; i < bars; i++)
        {
            var open = currentPrice;
            var close = currentPrice + 0.6m; // Strong rise
            var high = close + 0.2m;
            var low = open - 0.1m;

            data.Add(new MarketDataModel
            {
                Symbol = "BTCUSD",
                Exchange = Exchange.Kraken,
                Timeframe = Timeframe.OneHour,
                Timestamp = baseTime.AddHours(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1200000m,
                TradeCount = 1200
            });

            currentPrice = close;
        }

        return data.ToArray();
    }
}