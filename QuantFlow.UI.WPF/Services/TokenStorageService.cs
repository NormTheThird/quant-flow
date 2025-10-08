namespace QuantFlow.UI.WPF.Services;

public class TokenStorageService : ITokenStorageService
{
    private string? _token;
    private string? _refreshToken;

    public void StoreToken(string token, string refreshToken)
    {
        _token = token;
        _refreshToken = refreshToken;
    }

    public string? GetToken()
    {
        return _token;
    }

    public string? GetRefreshToken()
    {
        return _refreshToken;
    }

    public void ClearTokens()
    {
        _token = null;
        _refreshToken = null;
    }
}