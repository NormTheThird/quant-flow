namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a backtest execution in the QuantFlow system
/// </summary>
public class BacktestRunModel : BaseModel
{
    public required string Name { get; set; } = string.Empty;
    public required Guid AlgorithmId { get; set; } = Guid.Empty;
    public required Guid PortfolioId { get; set; } = Guid.Empty;
    public required Guid UserId { get; set; } = Guid.Empty;
    public required string Symbol { get; set; } = string.Empty;
    public required Exchange Exchange { get; set; } = Exchange.Binance;
    public required Timeframe Timeframe { get; set; } = Timeframe.OneHour;
    public DateTime BacktestStartDate { get; set; } = new();
    public DateTime BacktestEndDate { get; set; } = new();
    public required BacktestStatus Status { get; set; } = BacktestStatus.Pending;

    // Configuration
    public decimal InitialBalance { get; set; } = 10000.0m;
    public string AlgorithmParameters { get; set; } = "{}";
    public decimal CommissionRate { get; set; } = 0.001m;

    // Results
    public decimal FinalBalance { get; set; } = 0.0m;
    public decimal TotalReturnPercent { get; set; } = 0.0m;
    public decimal MaxDrawdownPercent { get; set; } = 0.0m;
    public decimal SharpeRatio { get; set; } = 0.0m;
    public int TotalTrades { get; set; } = 0;
    public int WinningTrades { get; set; } = 0;
    public int LosingTrades { get; set; } = 0;
    public decimal WinRatePercent { get; set; } = 0.0m;
    public decimal AverageTradeReturnPercent { get; set; } = 0.0m;
    public TimeSpan ExecutionDuration { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; } = null;

    // Navigation properties
    public UserModel User { get; set; } = null!;
    public PortfolioModel Portfolio { get; set; } = null!;
    public List<TradeModel> Trades { get; set; } = [];
}