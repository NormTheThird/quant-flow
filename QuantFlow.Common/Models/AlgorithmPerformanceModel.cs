namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing algorithm performance metrics
/// </summary>
public class AlgorithmPerformanceModel
{
    public Guid AlgorithmId { get; set; } = Guid.Empty;
    public string AlgorithmName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public decimal PnL { get; set; } = 0.0m;
    public decimal CumulativePnL { get; set; } = 0.0m;
    public decimal ReturnPercentage { get; set; } = 0.0m;
    public decimal CumulativeReturn { get; set; } = 0.0m;
    public decimal Drawdown { get; set; } = 0.0m;
    public decimal MaxDrawdown { get; set; } = 0.0m;
    public decimal? SharpeRatio { get; set; } = null;
    public decimal? WinRate { get; set; } = null;
    public decimal? ProfitFactor { get; set; } = null;
    public int TradeCount { get; set; } = 0;
    public int WinningTrades { get; set; } = 0;
    public int LosingTrades { get; set; } = 0;
    public int? AvgTradeDurationMinutes { get; set; } = null;
    public decimal PortfolioValue { get; set; } = 0.0m;
    public decimal PositionSize { get; set; } = 0.0m;
    public decimal? Leverage { get; set; } = null;
    public decimal? Volatility { get; set; } = null;
    public int ExecutionTimeMs { get; set; } = 0;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}