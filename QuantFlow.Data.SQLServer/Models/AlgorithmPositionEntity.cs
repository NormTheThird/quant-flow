namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing an algorithm position within a portfolio
/// </summary>
[Table("AlgorithmPositions")]
public class AlgorithmPositionEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid AlgorithmId { get; set; } = Guid.Empty;

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    public Guid? PortfolioId { get; set; } = null;

    [Required]
    [MaxLength(200)]
    public string PositionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    [Precision(5, 2)]
    public decimal AllocatedPercent { get; set; } = 0.0m;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Inactive";

    [Precision(5, 2)]
    public decimal MaxPositionSizePercent { get; set; } = 10.0m;

    [Precision(8, 6)]
    public decimal ExchangeFees { get; set; } = 0.001m;

    public bool AllowShortSelling { get; set; } = false;

    [Required]
    public string AlgorithmParameters { get; set; } = "{}";

    [Precision(18, 2)]
    public decimal CurrentValue { get; set; } = 0.0m;

    public DateTime? ActivatedAt { get; set; } = null;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // EF Core navigation properties
    [ForeignKey(nameof(PortfolioId))]
    public virtual PortfolioEntity Portfolio { get; set; } = null!;

    public virtual ICollection<TradeEntity> Trades { get; set; } = [];
}