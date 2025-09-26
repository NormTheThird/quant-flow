//namespace QuantFlow.Data.SQLServer.Models;

///// <summary>
///// SQL Server entity representing exchange configurations
///// </summary>
//[Table("ExchangeConfigurations")]
//public class ExchangeConfigurationEntity
//{
//    [Key]
//    public Guid Id { get; set; } = Guid.Empty;

//    [Required]
//    public int Exchange { get; set; } // Exchange enum value

//    [Required]
//    [MaxLength(50)]
//    public string Name { get; set; } = string.Empty;

//    public bool IsActive { get; set; } = true;
//    public bool IsSupported { get; set; } = true;

//    [Column(TypeName = "decimal(8,5)")]
//    public decimal BaseMakerFeePercent { get; set; } = 0.0m;

//    [Column(TypeName = "decimal(8,5)")]
//    public decimal BaseTakerFeePercent { get; set; } = 0.0m;

//    [MaxLength(200)]
//    public string ApiEndpoint { get; set; } = string.Empty;

//    public int MaxRequestsPerMinute { get; set; } = 60;

//    public DateTime CreatedAt { get; set; } = new();
//    public DateTime? UpdatedAt { get; set; } = null;
//    public bool IsDeleted { get; set; } = false;

//    [MaxLength(100)]
//    public string CreatedBy { get; set; } = string.Empty;

//    [MaxLength(100)]
//    public string? UpdatedBy { get; set; } = null;

//    // Navigation properties
//    public virtual ICollection<FeeTierEntity> FeeTiers { get; set; } = [];
//    public virtual ICollection<SymbolFeeOverrideEntity> SymbolOverrides { get; set; } = [];
//}