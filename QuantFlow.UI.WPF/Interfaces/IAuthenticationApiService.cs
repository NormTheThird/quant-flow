namespace QuantFlow.UI.WPF.Interfaces;

public interface IAuthenticationApiService
{
    Task<AuthenticateResponse?> AuthenticateAsync(string email, string password);
}