//namespace QuantFlow.Data.SQLServer.Models;

//// <summary>
//// SQL Server entity representing user subscriptions
//// </summary>
//[Table("Subscriptions")]
//public class SubscriptionEntity
//{
//    [Key]
//    public Guid Id { get; set; } = Guid.Empty;

//    [Required]
//    public Guid UserId { get; set; } = Guid.Empty;

//    [Required]
//    public int Type { get; set; } = 1;

//    public DateTime StartDate { get; set; } = new();

//    public DateTime EndDate { get; set; } = new();

//    public bool IsActive { get; set; } = true;

//    public bool IsDeleted { get; set; } = false;
//    public DateTime CreatedAt { get; set; } = new();

//    [MaxLength(100)]
//    public string CreatedBy { get; set; } = string.Empty;

//    public DateTime? UpdatedAt { get; set; } = null;

//    [MaxLength(100)]
//    public string? UpdatedBy { get; set; } = null;

//    EF Core navigation properties
//   [ForeignKey(nameof(UserId))]
//    public virtual UserEntity User { get; set; } = null!;
//}