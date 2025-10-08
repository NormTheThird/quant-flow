namespace QuantFlow.UI.WPF.Responses;

public class AuthenticateResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public BaseUserModel User { get; set; } = new();
    public int ExpiresIn { get; set; }
}