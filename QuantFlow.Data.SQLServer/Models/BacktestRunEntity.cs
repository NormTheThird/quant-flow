//namespace QuantFlow.Data.SQLServer.Models;

///// <summary>
///// SQL Server entity representing a backtest execution
///// </summary>
//[Table("BacktestRuns")]
//public class BacktestRunEntity
//{
//    [Key]
//    public Guid Id { get; set; } = Guid.Empty;

//    [Required]
//    [MaxLength(200)]
//    public string Name { get; set; } = string.Empty;

//    [Required]
//    public Guid AlgorithmId { get; set; } = Guid.Empty;

//    [Required]
//    public Guid PortfolioId { get; set; } = Guid.Empty;

//    [Required]
//    public Guid UserId { get; set; } = Guid.Empty;

//    [Required]
//    [MaxLength(20)]
//    public string Symbol { get; set; } = string.Empty;

//    [Required]
//    public int Exchange { get; set; } = 1; // Exchange enum

//    [Required]
//    public int Timeframe { get; set; } = 1; // Timeframe enum

//    public DateTime BacktestStartDate { get; set; } = new();
//    public DateTime BacktestEndDate { get; set; } = new();

//    [Required]
//    public int Status { get; set; } = 1; // BacktestStatus enum

//    // Configuration
//    [Precision(18, 2)]
//    public decimal InitialBalance { get; set; } = 10000.0m;

//    public string AlgorithmParameters { get; set; } = "{}";

//    [Precision(8, 6)]
//    public decimal CommissionRate { get; set; } = 0.001m;

//    // Results
//    [Precision(18, 2)]
//    public decimal FinalBalance { get; set; } = 0.0m;

//    [Precision(8, 4)]
//    public decimal TotalReturnPercent { get; set; } = 0.0m;

//    [Precision(8, 4)]
//    public decimal MaxDrawdownPercent { get; set; } = 0.0m;

//    [Precision(8, 4)]
//    public decimal SharpeRatio { get; set; } = 0.0m;

//    public int TotalTrades { get; set; } = 0;
//    public int WinningTrades { get; set; } = 0;
//    public int LosingTrades { get; set; } = 0;

//    [Precision(5, 2)]
//    public decimal WinRatePercent { get; set; } = 0.0m;

//    [Precision(8, 4)]
//    public decimal AverageTradeReturnPercent { get; set; } = 0.0m;

//    public long ExecutionDurationTicks { get; set; } = 0; // TimeSpan stored as ticks

//    [MaxLength(1000)]
//    public string ErrorMessage { get; set; } = string.Empty;

//    public DateTime? CompletedAt { get; set; } = null;

//    public DateTime CreatedAt { get; set; } = new();
//    public DateTime? UpdatedAt { get; set; } = null;
//    public bool IsDeleted { get; set; } = false;

//    [MaxLength(100)]
//    public string CreatedBy { get; set; } = string.Empty;

//    [MaxLength(100)]
//    public string? UpdatedBy { get; set; } = null;

//    // EF Core navigation properties
//    [ForeignKey(nameof(UserId))]
//    public virtual UserEntity User { get; set; } = null!;

//    [ForeignKey(nameof(PortfolioId))]
//    public virtual PortfolioEntity Portfolio { get; set; } = null!;

//    public virtual ICollection<TradeEntity> Trades { get; set; } = [];
//}