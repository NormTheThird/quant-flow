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

    [Precision(18, 8)]
    public decimal MinTradeAmount { get; set; } = 0.0m;

    public int PricePrecision { get; set; } = 8;

    public int QuantityPrecision { get; set; } = 8;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    //// EF Core navigation properties
    //public virtual ICollection<ExchangeSymbolEntity> ExchangeSymbols { get; set; } = [];
}