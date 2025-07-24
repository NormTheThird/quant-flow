namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of subscription repository
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionRepository> _logger;

    public SubscriptionRepository(ApplicationDbContext context, ILogger<SubscriptionRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a subscription by its unique identifier
    /// </summary>
    /// <param name="id">The subscription's unique identifier</param>
    /// <returns>Subscription business model if found, null otherwise</returns>
    public async Task<SubscriptionModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting subscription with ID: {SubscriptionId}", id);

        var entity = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all subscriptions for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of subscription business models</returns>
    public async Task<IEnumerable<SubscriptionModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting subscriptions for user: {UserId}", userId);

        var entities = await _context.Subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets the active subscription for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Active subscription business model if found, null otherwise</returns>
    public async Task<SubscriptionModel?> GetActiveByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting active subscription for user: {UserId}", userId);

        var entity = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && !s.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all active subscriptions
    /// </summary>
    /// <returns>Collection of subscription business models</returns>
    public async Task<IEnumerable<SubscriptionModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all subscriptions");

        var entities = await _context.Subscriptions
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new subscription
    /// </summary>
    /// <param name="subscription">Subscription business model to create</param>
    /// <returns>Created subscription business model</returns>
    public async Task<SubscriptionModel> CreateAsync(SubscriptionModel subscription)
    {
        _logger.LogInformation("Creating subscription for user: {UserId}", subscription.UserId);

        var entity = subscription.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;

        _context.Subscriptions.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing subscription
    /// </summary>
    /// <param name="subscription">Subscription business model with updates</param>
    /// <returns>Updated subscription business model</returns>
    public async Task<SubscriptionModel> UpdateAsync(SubscriptionModel subscription)
    {
        _logger.LogInformation("Updating subscription with ID: {SubscriptionId}", subscription.Id);

        var entity = await _context.Subscriptions.FindAsync(subscription.Id);
        if (entity == null)
            throw new NotFoundException($"Subscription with ID {subscription.Id} not found");

        entity.Type = (int)subscription.Type;
        entity.StartDate = subscription.StartDate;
        entity.EndDate = subscription.EndDate;
        entity.IsActive = subscription.IsActive;
        entity.MaxPortfolios = subscription.MaxPortfolios;
        entity.MaxAlgorithms = subscription.MaxAlgorithms;
        entity.MaxBacktestRuns = subscription.MaxBacktestRuns;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes a subscription
    /// </summary>
    /// <param name="id">The subscription's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting subscription with ID: {SubscriptionId}", id);

        var entity = await _context.Subscriptions.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets subscriptions by type
    /// </summary>
    /// <param name="subscriptionType">The subscription type to filter by</param>
    /// <returns>Collection of subscription business models</returns>
    public async Task<IEnumerable<SubscriptionModel>> GetByTypeAsync(SubscriptionType subscriptionType)
    {
        _logger.LogInformation("Getting subscriptions of type: {SubscriptionType}", subscriptionType);

        var entities = await _context.Subscriptions
            .Where(s => s.Type == (int)subscriptionType && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets expired subscriptions
    /// </summary>
    /// <returns>Collection of expired subscription business models</returns>
    public async Task<IEnumerable<SubscriptionModel>> GetExpiredAsync()
    {
        _logger.LogInformation("Getting expired subscriptions");

        var now = DateTime.UtcNow;
        var entities = await _context.Subscriptions
            .Where(s => s.EndDate < now && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.EndDate)
            .ToListAsync();

        return entities.ToBusinessModels();
    }
}