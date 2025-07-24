namespace QuantFlow.Common.Models;

/// <summary>
/// Base model class that provides common properties for all business entities
/// </summary>
public abstract class BaseModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; } = new();
    public DateTime? UpdatedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; } = null;
}