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
    public decimal InitialBalance { get; set; } = 10000.0m;

    [Required]
    [Precision(18, 2)]
    public decimal CurrentBalance { get; set; } = 10000.0m;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Inactive"; // PortfolioStatus enum as string

    [Required]
    [MaxLength(50)]
    public string Mode { get; set; } = "TestMode"; // PortfolioMode enum as string

    [MaxLength(50)]
    public string? Exchange { get; set; } = null; // Exchange enum as string (nullable)

    public Guid? UserExchangeDetailsId { get; set; } = null;

    [Precision(5, 2)]
    public decimal MaxPositionSizePercent { get; set; } = 10.0m;

    [Precision(8, 6)]
    public decimal CommissionRate { get; set; } = 0.001m;

    public bool AllowShortSelling { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // EF Core navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey(nameof(UserExchangeDetailsId))]
    public virtual UserExchangeDetailsEntity? UserExchangeDetails { get; set; } = null;
}