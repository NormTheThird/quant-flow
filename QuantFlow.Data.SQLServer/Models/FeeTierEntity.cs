namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing fee tiers
/// </summary>
[Table("FeeTiers")]
public class FeeTierEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid ExchangeConfigurationId { get; set; } = Guid.Empty;

    [Required]
    public int Exchange { get; set; } // Exchange enum value

    public int TierLevel { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumVolumeThreshold { get; set; }

    [Column(TypeName = "decimal(8,5)")]
    public decimal MakerFeePercent { get; set; }

    [Column(TypeName = "decimal(8,5)")]
    public decimal TakerFeePercent { get; set; }

    public bool IsActive { get; set; } = true;

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