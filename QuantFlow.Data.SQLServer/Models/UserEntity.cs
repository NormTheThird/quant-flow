namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing a user in the database
/// </summary>
[Table("User")]
public class UserEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsSystemAdmin { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    public virtual ICollection<BacktestRunEntity> BacktestRuns { get; set; } = [];
}