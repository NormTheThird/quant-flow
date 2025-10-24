namespace QuantFlow.Common.Interfaces.Repositories.SQLServer;

/// <summary>
/// Repository interface for user exchange details operations
/// </summary>
public interface IUserExchangeDetailsRepository
{
    /// <summary>
    /// Gets exchange details by unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>UserExchangeDetailsModel if found, null otherwise</returns>
    Task<UserExchangeDetailsModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all exchange details for a specific user and exchange
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="exchange">The exchange name</param>
    /// <returns>Collection of UserExchangeDetailsModel</returns>
    Task<IEnumerable<UserExchangeDetailsModel>> GetByUserAndExchangeAsync(Guid userId, string exchange);

    /// <summary>
    /// Gets all exchange details for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of UserExchangeDetailsModel</returns>
    Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new exchange detail
    /// </summary>
    /// <param name="model">UserExchangeDetailsModel to create</param>
    /// <returns>Created UserExchangeDetailsModel</returns>
    Task<UserExchangeDetailsModel> CreateAsync(UserExchangeDetailsModel model);

    /// <summary>
    /// Updates an existing exchange detail
    /// </summary>
    /// <param name="model">UserExchangeDetailsModel with updates</param>
    /// <returns>Updated UserExchangeDetailsModel</returns>
    Task<UserExchangeDetailsModel> UpdateAsync(UserExchangeDetailsModel model);

    /// <summary>
    /// Soft deletes an exchange detail
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);
}