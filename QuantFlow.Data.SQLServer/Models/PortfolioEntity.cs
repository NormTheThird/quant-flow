//namespace QuantFlow.Data.SQLServer.Models;

///// <summary>
///// SQL Server entity representing a trading portfolio
///// </summary>
//[Table("Portfolios")]
//public class PortfolioEntity
//{
//    [Key]
//    public Guid Id { get; set; } = Guid.Empty;

//    [Required]
//    public Guid UserId { get; set; } = Guid.Empty;

//    [Required]
//    [MaxLength(100)]
//    public string Name { get; set; } = string.Empty;

//    [MaxLength(500)]
//    public string Description { get; set; } = string.Empty;

//    [Required]
//    [Precision(18, 2)]
//    public decimal InitialBalance { get; set; } = 10000.0m;

//    [Required]
//    [Precision(18, 2)]
//    public decimal CurrentBalance { get; set; } = 10000.0m;

//    [Required]
//    public int Status { get; set; } = 1; // PortfolioStatus enum

//    [Precision(5, 2)]
//    public decimal MaxPositionSizePercent { get; set; } = 10.0m;

//    public bool AllowShortSelling { get; set; } = false;

//    public bool IsDeleted { get; set; } = false;

//    public DateTime CreatedAt { get; set; } = new();

//    [MaxLength(100)]
//    public string CreatedBy { get; set; } = string.Empty;
//    public DateTime? UpdatedAt { get; set; } = null;

//    [MaxLength(100)]
//    public string? UpdatedBy { get; set; } = null;

//    // EF Core navigation properties
//    [ForeignKey(nameof(UserId))]
//    public virtual UserEntity User { get; set; } = null!;

//    public virtual ICollection<BacktestRunEntity> BacktestRuns { get; set; } = [];
//}