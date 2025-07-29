namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Token validation response model
/// </summary>
public class TokenValidationResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsValid { get; set; } = false;
}