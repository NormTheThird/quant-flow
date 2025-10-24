namespace QuantFlow.Common.Interfaces.Repositories.SQLServer;

/// <summary>
/// Repository interface for user refresh token data access operations
/// </summary>
public interface IUserRefreshTokenRepository
{
    /// <summary>
    /// Creates a new refresh token for a user
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="token">The refresh token string</param>
    /// <param name="expiresAt">When the token expires</param>
    /// <returns>Created user refresh token model</returns>
    Task<UserRefreshTokenModel> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt);

    /// <summary>
    /// Creates a password reset token
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="token">The reset token string</param>
    /// <param name="expiresAt">When the token expires</param>
    /// <returns>Created password reset token model</returns>
    Task<UserRefreshTokenModel> CreatePasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt);

    /// <summary>
    /// Gets a refresh token by token string
    /// </summary>
    /// <param name="token">The refresh token string</param>
    /// <returns>User refresh token model if found, null otherwise</returns>
    Task<UserRefreshTokenModel?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets a password reset token by token string
    /// </summary>
    /// <param name="token">The reset token string</param>
    /// <returns>Password reset token model if found and valid, null otherwise</returns>
    Task<UserRefreshTokenModel?> GetPasswordResetTokenAsync(string token);

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="token">The refresh token string to revoke</param>
    /// <returns>True if revoked successfully, false if token not found</returns>
    Task<bool> RevokeTokenAsync(string token);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserTokensAsync(Guid userId);

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>Collection of active user refresh token models</returns>
    Task<IEnumerable<UserRefreshTokenModel>> GetActiveTokensByUserAsync(Guid userId);

    /// <summary>
    /// Deletes expired tokens (cleanup)
    /// </summary>
    /// <returns>Number of tokens deleted</returns>
    Task<int> DeleteExpiredTokensAsync();
}