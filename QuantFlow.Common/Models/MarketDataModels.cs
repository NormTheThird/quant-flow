namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing market data for trading
/// </summary>
public class MarketDataModel
{
    public string Symbol { get; set; } = string.Empty;
    public Timeframe Timeframe { get; set; } = Timeframe.Unknown;
    public Exchange Exchange { get; set; } = Exchange.Unknown;
    public decimal Open { get; set; } = 0.0m;
    public decimal High { get; set; } = 0.0m;
    public decimal Low { get; set; } = 0.0m;
    public decimal Close { get; set; } = 0.0m;
    public decimal Volume { get; set; } = 0.0m;
    public decimal? VWAP { get; set; } = null;
    public int? TradeCount { get; set; } = null;
    public decimal? Bid { get; set; } = null;
    public decimal? Ask { get; set; } = null;
    public decimal? QuoteVolume { get; set; } = null;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Summary model representing availability of market data for a specific symbol/exchange/timeframe combination
/// </summary>
public class MarketDataSummary
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RecordCount { get; set; }
}