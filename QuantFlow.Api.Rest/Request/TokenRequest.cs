namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Token request model
/// </summary>
public class TokenRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}