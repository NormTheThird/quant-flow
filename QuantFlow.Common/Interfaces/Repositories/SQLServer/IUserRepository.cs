namespace QuantFlow.Common.Interfaces.Repositories.SQLServer;

/// <summary>
/// Repository interface for user data access operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>User business model if found, null otherwise</returns>
    Task<UserModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <returns>User business model if found, null otherwise</returns>
    Task<UserModel?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username to search for</param>
    /// <returns>User business model if found, null otherwise</returns>
    Task<UserModel?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of user business models</returns>
    Task<IEnumerable<UserModel>> GetAllAsync();

    /// <summary>
    /// Gets a user's password hash by email for authentication validation
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Password hash if user found, null otherwise</returns>
    Task<string?> GetPasswordHashByEmailAsync(string email);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User business model to create</param>
    /// <returns>Created user business model</returns>
    Task<UserModel> CreateAsync(UserModel user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">User business model with updates</param>
    /// <returns>Updated user business model</returns>
    Task<UserModel> UpdateAsync(UserModel user);

    /// <summary>
    /// Updates a user's password hash
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="newPasswordHash">New hashed password</param>
    /// <returns>True if updated successfully, false if user not found</returns>
    Task<bool> UpdatePasswordHashAsync(Guid userId, string newPasswordHash);

    /// <summary>
    /// Soft deletes a user
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);
}