namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity representing user subscriptions
/// </summary>
[Table("Subscriptions")]
public class SubscriptionEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    [Required]
    public int Type { get; set; } = 1; // SubscriptionType enum

    public DateTime StartDate { get; set; } = new();
    public DateTime EndDate { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public int MaxPortfolios { get; set; } = 1;
    public int MaxAlgorithms { get; set; } = 5;
    public int MaxBacktestRuns { get; set; } = 10;

    public DateTime CreatedAt { get; set; } = new();
    public DateTime? UpdatedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; } = null;

    // EF Core navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;
}