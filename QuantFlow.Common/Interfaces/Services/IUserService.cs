namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for user management and authentication operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Validates user credentials during authentication
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">Plain text password to verify</param>
    /// <returns>User model if credentials are valid, null if invalid</returns>
    Task<UserModel?> ValidateUserCredentialsAsync(string email, string password);

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>User model if found, null if not found</returns>
    Task<UserModel?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>User model if found, null if not found</returns>
    Task<UserModel?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Creates a new user with hashed password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's plain text password (will be hashed)</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="username">User's username</param>
    /// <param name="isSystemAdmin">Whether user should have admin privileges</param>
    /// <returns>Created user business model</returns>
    Task<UserModel> CreateUserAsync(string email, string password, string firstName, string lastName, string username, bool isSystemAdmin = false);

    /// <summary>
    /// Updates a user's password
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="newPassword">New plain text password (will be hashed)</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdatePasswordAsync(Guid userId, string newPassword);

    /// <summary>
    /// Creates a token for a user
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="token">The token string</param>
    /// <param name="expiresAt">When the token expires</param>
    /// <param name="tokenType">Type of token (Refresh or PasswordReset)</param>
    /// <returns>Created user refresh token model</returns>
    Task<UserRefreshTokenModel> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, RefreshTokenType tokenType);

    /// <summary>
    /// Gets a token by token string and type
    /// </summary>
    /// <param name="token">The token string</param>
    /// <param name="tokenType">Type of token to retrieve</param>
    /// <returns>User refresh token model if found, null otherwise</returns>
    Task<UserRefreshTokenModel?> GetRefreshTokenAsync(string token, RefreshTokenType tokenType);

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token string to revoke</param>
    /// <returns>True if revoked successfully, false if token not found</returns>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices)
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserRefreshTokensAsync(Guid userId);
}