namespace QuantFlow.Common.Models;

/// <summary>
/// Represents user exchange API credentials and configuration
/// </summary>
public class UserExchangeDetailsModel : BaseModel
{
    public required Guid UserId { get; set; }
    public required string Exchange { get; set; } = string.Empty;
    public required string KeyName { get; set; } = string.Empty;
    public required string KeyValue { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public UserModel User { get; set; } = null!;
}