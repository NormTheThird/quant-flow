namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for user operations using business models
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
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of user business models</returns>
    Task<IEnumerable<UserModel>> GetAllAsync();

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
    /// Soft deletes a user
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

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
}