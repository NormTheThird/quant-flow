namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// InfluxDB measurement for algorithm performance metrics
/// </summary>
[Measurement("algorithm_performance")]
public class AlgorithmPerformancePoint : BaseTimeSeriesPoint
{
    // Tags
    [Column("algorithm_id", IsTag = true)]
    public string AlgorithmId { get; set; } = string.Empty;

    [Column("algorithm_name", IsTag = true)]
    public string AlgorithmName { get; set; } = string.Empty;

    [Column("version", IsTag = true)]
    public string Version { get; set; } = string.Empty;

    [Column("environment", IsTag = true)]
    public string Environment { get; set; } = string.Empty; // dev, test, prod

    [Column("symbol", IsTag = true)]
    public string Symbol { get; set; } = string.Empty;

    [Column("timeframe", IsTag = true)]
    public string Timeframe { get; set; } = string.Empty;

    // Performance Fields
    [Column("pnl")]
    public decimal PnL { get; set; } = 0.0m;

    [Column("cumulative_pnl")]
    public decimal CumulativePnL { get; set; } = 0.0m;

    [Column("return_percentage")]
    public decimal ReturnPercentage { get; set; } = 0.0m;

    [Column("cumulative_return")]
    public decimal CumulativeReturn { get; set; } = 0.0m;

    [Column("drawdown")]
    public decimal Drawdown { get; set; } = 0.0m;

    [Column("max_drawdown")]
    public decimal MaxDrawdown { get; set; } = 0.0m;

    [Column("sharpe_ratio")]
    public decimal? SharpeRatio { get; set; } = null;

    [Column("win_rate")]
    public decimal? WinRate { get; set; } = null;

    [Column("profit_factor")]
    public decimal? ProfitFactor { get; set; } = null;

    [Column("trade_count")]
    public int TradeCount { get; set; } = 0;

    [Column("winning_trades")]
    public int WinningTrades { get; set; } = 0;

    [Column("losing_trades")]
    public int LosingTrades { get; set; } = 0;

    [Column("avg_trade_duration_minutes")]
    public int? AvgTradeDurationMinutes { get; set; } = null;

    [Column("portfolio_value")]
    public decimal PortfolioValue { get; set; } = 0.0m;

    [Column("position_size")]
    public decimal PositionSize { get; set; } = 0.0m;

    [Column("leverage")]
    public decimal? Leverage { get; set; } = null;

    [Column("volatility")]
    public decimal? Volatility { get; set; } = null;

    [Column("execution_time_ms")]
    public int ExecutionTimeMs { get; set; } = 0;
}