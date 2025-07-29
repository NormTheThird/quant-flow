namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Refresh token request model
/// </summary>
public class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}