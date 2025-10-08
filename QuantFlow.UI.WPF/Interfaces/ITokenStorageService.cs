namespace QuantFlow.UI.WPF.Interfaces;

public interface ITokenStorageService
{
    void StoreToken(string token, string refreshToken);
    string? GetToken();
    string? GetRefreshToken();
    void ClearTokens();
}