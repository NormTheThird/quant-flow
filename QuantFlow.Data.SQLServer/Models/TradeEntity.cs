namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing an individual trade
/// </summary>
[Table("Trades")]
public class TradeEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid BacktestRunId { get; set; } = Guid.Empty;

    public Guid? EntryTradeId { get; set; } = null;

    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public int Type { get; set; } = 1; // TradeType enum

    public DateTime ExecutionTimestamp { get; set; } = new();

    [Precision(18, 8)]
    public decimal Quantity { get; set; } = 0.0m;

    [Precision(18, 8)]
    public decimal Price { get; set; } = 0.0m;

    [Precision(18, 2)]
    public decimal Value { get; set; } = 0.0m;

    [Precision(18, 6)]
    public decimal Commission { get; set; } = 0.0m;

    [Precision(18, 2)]
    public decimal NetValue { get; set; } = 0.0m;

    [Precision(18, 2)]
    public decimal PortfolioBalanceBefore { get; set; } = 0.0m;

    [Precision(18, 2)]
    public decimal PortfolioBalanceAfter { get; set; } = 0.0m;

    [MaxLength(500)]
    public string AlgorithmReason { get; set; } = string.Empty;

    [Precision(3, 2)]
    public decimal AlgorithmConfidence { get; set; } = 0.0m;

    [Precision(18, 2)]
    public decimal? RealizedProfitLoss { get; set; } = null;

    [Precision(8, 4)]
    public decimal? RealizedProfitLossPercent { get; set; } = null;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    [ForeignKey(nameof(EntryTradeId))]
    public virtual TradeEntity? EntryTrade { get; set; } = null;
}