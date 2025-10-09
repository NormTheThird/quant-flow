namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing user exchange details including credentials and configuration
/// </summary>
[Table("UserExchangeDetails")]
public class UserExchangeDetailsEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(50)]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string KeyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string KeyValue { get; set; } = string.Empty;

    public bool IsEncrypted { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;
}