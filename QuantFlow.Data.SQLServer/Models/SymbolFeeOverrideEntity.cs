namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing symbol fee overrides
/// </summary>
[Table("SymbolFeeOverrides")]
public class SymbolFeeOverrideEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid ExchangeConfigurationId { get; set; } = Guid.Empty;

    [Required]
    public int Exchange { get; set; } // Exchange enum value

    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Column(TypeName = "decimal(8,5)")]
    public decimal MakerFeePercent { get; set; }

    [Column(TypeName = "decimal(8,5)")]
    public decimal TakerFeePercent { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = new();
    public DateTime? UpdatedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; } = null;

    // Navigation property
    [ForeignKey(nameof(ExchangeConfigurationId))]
    public virtual ExchangeConfigurationEntity ExchangeConfiguration { get; set; } = null!;
}