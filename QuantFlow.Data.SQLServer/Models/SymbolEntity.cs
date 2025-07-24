namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing a cryptocurrency trading pair
/// </summary>
[Table("Symbols")]
public class SymbolEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string BaseAsset { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string QuoteAsset { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [Precision(18, 8)]
    public decimal MinTradeAmount { get; set; } = 0.0m;

    public int PricePrecision { get; set; } = 8;
    public int QuantityPrecision { get; set; } = 8;

    public DateTime CreatedAt { get; set; } = new();
    public DateTime? UpdatedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; } = null;

    // EF Core navigation properties
    public virtual ICollection<ExchangeSymbolEntity> ExchangeSymbols { get; set; } = [];
}