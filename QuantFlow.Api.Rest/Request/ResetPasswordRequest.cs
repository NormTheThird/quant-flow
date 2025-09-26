namespace QuantFlow.Api.Rest.Request;

public class ResetPasswordRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}