namespace QuantFlow.Common.Models;

/// <summary>
/// Represents an individual trade in the QuantFlow system
/// </summary>
public class TradeModel : BaseModel
{
    public required Guid BacktestRunId { get; set; } = Guid.Empty;
    public required Exchange Exchange { get; set; } = Exchange.Unknown;
    public required string Symbol { get; set; } = string.Empty;
    public required TradeType Type { get; set; } = TradeType.Buy;
    public DateTime ExecutionTimestamp { get; set; } = new();
    public decimal Quantity { get; set; } = 0.0m;
    public decimal Price { get; set; } = 0.0m;
    public decimal Value { get; set; } = 0.0m;
    public decimal Commission { get; set; } = 0.0m;
    public decimal NetValue { get; set; } = 0.0m;
    public decimal PortfolioBalanceBefore { get; set; } = 0.0m;
    public decimal PortfolioBalanceAfter { get; set; } = 0.0m;
    public string AlgorithmReason { get; set; } = string.Empty;
    public decimal AlgorithmConfidence { get; set; } = 0.0m;
    public decimal? RealizedProfitLoss { get; set; } = null;
    public decimal? RealizedProfitLossPercent { get; set; } = null;
    public Guid? EntryTradeId { get; set; } = null;

    // Navigation properties
    public BacktestRunModel BacktestRun { get; set; } = null!;
    public TradeModel? EntryTrade { get; set; } = null;
}