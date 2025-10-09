namespace QuantFlow.UI.WPF.Interfaces;

/// <summary>
/// Authentication API service interface
/// </summary>
public interface IAuthenticationApiService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Authentication response with token and user info</returns>
    Task<AuthenticateResponse?> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Ensures user preferences exist for the specified user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Authentication token</param>
    /// <returns>True if successful</returns>
    Task<bool> ValidateUserPreferencesAsync(Guid userId, string token);
}