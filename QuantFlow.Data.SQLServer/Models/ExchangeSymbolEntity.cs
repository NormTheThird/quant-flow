//namespace QuantFlow.Data.SQLServer.Models;

///// <summary>
///// SQL Server entity representing symbol availability on exchanges
///// </summary>
//[Table("ExchangeSymbols")]
//public class ExchangeSymbolEntity
//{
//    [Key]
//    public Guid Id { get; set; } = Guid.Empty;

//    [Required]
//    public Guid SymbolId { get; set; } = Guid.Empty;

//    [Required]
//    public int Exchange { get; set; } = 1; // Exchange enum

//    [Required]
//    [MaxLength(50)]
//    public string ExchangeSymbolName { get; set; } = string.Empty;

//    public bool IsActive { get; set; } = true;
//    public DateTime LastDataUpdate { get; set; } = new();

//    [MaxLength(200)]
//    public string ApiEndpoint { get; set; } = string.Empty;

//    public DateTime CreatedAt { get; set; } = new();
//    public DateTime? UpdatedAt { get; set; } = null;
//    public bool IsDeleted { get; set; } = false;

//    [MaxLength(100)]
//    public string CreatedBy { get; set; } = string.Empty;

//    [MaxLength(100)]
//    public string? UpdatedBy { get; set; } = null;

//    // EF Core navigation properties
//    [ForeignKey(nameof(SymbolId))]
//    public virtual SymbolEntity Symbol { get; set; } = null!;
//}