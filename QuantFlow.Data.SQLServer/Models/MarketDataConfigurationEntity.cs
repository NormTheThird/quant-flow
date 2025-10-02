namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing market data collection configuration
/// </summary>
[Table("MarketDataConfiguration")]
public class MarketDataConfigurationEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid SymbolId { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(50)]
    public string Exchange { get; set; } = string.Empty;

    // Interval toggles
    public bool Is1mActive { get; set; } = false;

    public bool Is5mActive { get; set; } = false;

    public bool Is15mActive { get; set; } = false;

    public bool Is1hActive { get; set; } = false;

    public bool Is4hActive { get; set; } = false;

    public bool Is1dActive { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(SymbolId))]
    public virtual SymbolEntity Symbol { get; set; } = null!;
}