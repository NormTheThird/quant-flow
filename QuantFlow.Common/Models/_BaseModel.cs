namespace QuantFlow.Common.Models;

/// <summary>
/// Base model class that provides common properties for all business entities
/// </summary>
public abstract class BaseModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = new();
    public string UpdatedBy { get; set; } = string.Empty;
}