namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing a user in the database
/// </summary>
[Table("Users")]
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

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;
    public bool IsSystemAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
    public DateTime? UpdatedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; } = null;

    // EF Core navigation properties
    public virtual ICollection<PortfolioEntity> Portfolios { get; set; } = [];
    public virtual ICollection<SubscriptionEntity> Subscriptions { get; set; } = [];
    public virtual ICollection<BacktestRunEntity> BacktestRuns { get; set; } = [];
}