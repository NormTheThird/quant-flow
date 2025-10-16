namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing a trading portfolio
/// </summary>
[Table("Portfolios")]
public class PortfolioEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Precision(18, 2)]
    public decimal InitialBalance { get; set; } = 0.0m;

    [Required]
    [Precision(18, 2)]
    public decimal CurrentBalance { get; set; } = 0.0m;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Inactive";

    [Required]
    [MaxLength(50)]
    public string Mode { get; set; } = "TestMode";

    [Required]
    [MaxLength(50)]
    public string Exchange { get; set; } = "Unknown"; // Exchange enum as string (Unknown for test mode)

    [Required]
    [MaxLength(10)]
    public string BaseCurrency { get; set; } = "USDT";

    public Guid? UserExchangeDetailsId { get; set; } = null;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey(nameof(UserExchangeDetailsId))]
    public virtual UserExchangeDetailsEntity? UserExchangeDetails { get; set; } = null;

    public virtual ICollection<AlgorithmPositionEntity> AlgorithmPositions { get; set; } = [];

    public virtual ICollection<BacktestRunEntity> BacktestRuns { get; set; } = [];
}