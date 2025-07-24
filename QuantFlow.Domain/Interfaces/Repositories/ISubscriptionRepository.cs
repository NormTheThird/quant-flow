namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for subscription operations using business models
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Gets a subscription by its unique identifier
    /// </summary>
    /// <param name="id">The subscription's unique identifier</param>
    /// <returns>Subscription business model if found, null otherwise</returns>
    Task<SubscriptionModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all subscriptions for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of subscription business models</returns>
    Task<IEnumerable<SubscriptionModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets the active subscription for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Active subscription business model if found, null otherwise</returns>
    Task<SubscriptionModel?> GetActiveByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all subscriptions
    /// </summary>
    /// <returns>Collection of subscription business models</returns>
    Task<IEnumerable<SubscriptionModel>> GetAllAsync();

    /// <summary>
    /// Creates a new subscription
    /// </summary>
    /// <param name="subscription">Subscription business model to create</param>
    /// <returns>Created subscription business model</returns>
    Task<SubscriptionModel> CreateAsync(SubscriptionModel subscription);

    /// <summary>
    /// Updates an existing subscription
    /// </summary>
    /// <param name="subscription">Subscription business model with updates</param>
    /// <returns>Updated subscription business model</returns>
    Task<SubscriptionModel> UpdateAsync(SubscriptionModel subscription);

    /// <summary>
    /// Soft deletes a subscription
    /// </summary>
    /// <param name="id">The subscription's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets subscriptions by type
    /// </summary>
    /// <param name="subscriptionType">The subscription type to filter by</param>
    /// <returns>Collection of subscription business models</returns>
    Task<IEnumerable<SubscriptionModel>> GetByTypeAsync(SubscriptionType subscriptionType);

    /// <summary>
    /// Gets expired subscriptions
    /// </summary>
    /// <returns>Collection of expired subscription business models</returns>
    Task<IEnumerable<SubscriptionModel>> GetExpiredAsync();
}