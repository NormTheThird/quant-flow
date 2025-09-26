namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing a refresh token in the database
/// </summary>
[Table("UserRefreshToken")]
public class UserRefreshTokenEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TokenType { get; set; } = string.Empty; 

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // EF Core navigation properties
    [ForeignKey("UserId")]
    public virtual UserEntity User { get; set; } = null!;
}