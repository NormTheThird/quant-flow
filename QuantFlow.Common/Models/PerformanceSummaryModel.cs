namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing aggregated algorithm performance summary
/// </summary>
public class PerformanceSummaryModel
{
    public Guid AlgorithmId { get; set; } = Guid.Empty;
    public string Symbol { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public decimal TotalPnL { get; set; } = 0.0m;
    public decimal MaxDrawdown { get; set; } = 0.0m;
    public decimal? SharpeRatio { get; set; } = null;
    public decimal? WinRate { get; set; } = null;
    public int TotalTrades { get; set; } = 0;
    public int WinningTrades { get; set; } = 0;
    public int LosingTrades { get; set; } = 0;
    public decimal AvgReturnPerTrade { get; set; } = 0.0m;
    public decimal BestTrade { get; set; } = 0.0m;
    public decimal WorstTrade { get; set; } = 0.0m;
    public decimal? ProfitFactor { get; set; } = null;
    public decimal? AvgHoldingPeriodHours { get; set; } = null;
}