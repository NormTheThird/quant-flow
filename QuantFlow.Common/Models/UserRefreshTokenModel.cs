namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing a refresh token
/// </summary>
public class UserRefreshTokenModel : BaseModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; }
}