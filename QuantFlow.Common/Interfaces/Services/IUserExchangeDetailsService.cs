namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for user exchange details operations
/// </summary>
public interface IUserExchangeDetailsService
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
    /// <returns>Collection of UserExchangeDetailsModel with decrypted values</returns>
    Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAndExchangeAsync(Guid userId, Exchange exchange);

    /// <summary>
    /// Gets all exchange details for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of UserExchangeDetailsModel with decrypted values</returns>
    Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new exchange detail (encrypts if specified)
    /// </summary>
    /// <param name="model">UserExchangeDetailsModel to create</param>
    /// <returns>Created UserExchangeDetailsModel</returns>
    Task<UserExchangeDetailsModel> CreateAsync(UserExchangeDetailsModel model);

    /// <summary>
    /// Updates an existing exchange detail (encrypts if specified)
    /// </summary>
    /// <param name="model">UserExchangeDetailsModel with updates</param>
    /// <returns>Updated UserExchangeDetailsModel</returns>
    Task<UserExchangeDetailsModel> UpdateAsync(UserExchangeDetailsModel model);

    /// <summary>
    /// Deletes an exchange detail
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);
}