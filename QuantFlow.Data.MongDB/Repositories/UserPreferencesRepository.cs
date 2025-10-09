namespace QuantFlow.Data.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of user preferences repository
/// </summary>
public class UserPreferencesRepository : IUserPreferencesRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<UserPreferencesRepository> _logger;
    private readonly IMongoCollection<UserPreferencesDocument> _collection;

    public UserPreferencesRepository(MongoDbContext context, ILogger<UserPreferencesRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collection = _context.GetCollection<UserPreferencesDocument>();
    }

    /// <summary>
    /// Gets user preferences by user ID
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>User preferences business model if found, null otherwise</returns>
    public async Task<UserPreferencesModel?> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting preferences for user: {UserId}", userId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Gets user preferences by ID
    /// </summary>
    /// <param name="id">Preferences unique identifier</param>
    /// <returns>User preferences business model if found, null otherwise</returns>
    public async Task<UserPreferencesModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting preferences with ID: {PreferencesId}", id);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.Id, id),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Creates new user preferences
    /// </summary>
    /// <param name="preferences">User preferences business model to create</param>
    /// <returns>Created user preferences business model</returns>
    public async Task<UserPreferencesModel> CreateAsync(UserPreferencesModel preferences)
    {
        _logger.LogInformation("Creating preferences for user: {UserId}", preferences.UserId);

        if (preferences.Id == Guid.Empty)
            preferences.Id = Guid.NewGuid();

        preferences.CreatedAt = DateTime.UtcNow;
        preferences.UpdatedAt = DateTime.UtcNow;

        var document = preferences.ToDocument();
        await _collection.InsertOneAsync(document);

        _logger.LogInformation("Preferences created for user: {UserId}", preferences.UserId);
        return document.ToBusinessModel();
    }

    /// <summary>
    /// Updates existing user preferences
    /// </summary>
    /// <param name="preferences">User preferences business model with updated values</param>
    /// <returns>Updated user preferences business model</returns>
    public async Task<UserPreferencesModel> UpdateAsync(UserPreferencesModel preferences)
    {
        _logger.LogInformation("Updating preferences for user: {UserId}", preferences.UserId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, preferences.UserId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            throw new InvalidOperationException($"User preferences for user {preferences.UserId} not found");
        }

        preferences.UpdatedAt = DateTime.UtcNow;
        existingDocument.UpdateFromModel(preferences);

        await _collection.ReplaceOneAsync(filter, existingDocument);

        _logger.LogInformation("Preferences updated for user: {UserId}", preferences.UserId);
        return existingDocument.ToBusinessModel();
    }

    /// <summary>
    /// Creates or updates user preferences (upsert operation)
    /// </summary>
    /// <param name="preferences">User preferences business model</param>
    /// <returns>Created or updated user preferences business model</returns>
    public async Task<UserPreferencesModel> UpsertAsync(UserPreferencesModel preferences)
    {
        _logger.LogInformation("Upserting preferences for user: {UserId}", preferences.UserId);

        var existing = await GetByUserIdAsync(preferences.UserId);
        if (existing != null)
        {
            preferences.Id = existing.Id;
            preferences.CreatedAt = existing.CreatedAt;
            return await UpdateAsync(preferences);
        }
        else
        {
            return await CreateAsync(preferences);
        }
    }

    /// <summary>
    /// Soft deletes user preferences
    /// </summary>
    /// <param name="userId">User ID whose preferences to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Soft deleting preferences for user: {UserId}", userId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<UserPreferencesDocument>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Preferences soft deleted for user: {UserId}", userId);
        }
        else
        {
            _logger.LogWarning("Preferences not found for deletion, user: {UserId}", userId);
        }

        return success;
    }

    /// <summary>
    /// Updates a specific preference section for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="section">Section name (e.g., "DashboardLayout", "ChartSettings")</param>
    /// <param name="value">New value for the section</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateSectionAsync(Guid userId, string section, object value)
    {
        _logger.LogInformation("Updating preference section {Section} for user: {UserId}", section, userId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var bsonValue = BsonValue.Create(value);
        var update = Builders<UserPreferencesDocument>.Update
            .Set(section, bsonValue)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Preference section {Section} updated for user: {UserId}", section, userId);
        }
        else
        {
            _logger.LogWarning("Failed to update preference section {Section} for user: {UserId}", section, userId);
        }

        return success;
    }

    /// <summary>
    /// Adds a symbol to user's favorites
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="symbol">Symbol to add</param>
    /// <returns>True if added successfully</returns>
    public async Task<bool> AddFavoriteSymbolAsync(Guid userId, string symbol)
    {
        _logger.LogInformation("Adding favorite symbol {Symbol} for user: {UserId}", symbol, userId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<UserPreferencesDocument>.Update
            .AddToSet(x => x.FavoriteSymbols, symbol)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Removes a symbol from user's favorites
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="symbol">Symbol to remove</param>
    /// <returns>True if removed successfully</returns>
    public async Task<bool> RemoveFavoriteSymbolAsync(Guid userId, string symbol)
    {
        _logger.LogInformation("Removing favorite symbol {Symbol} for user: {UserId}", symbol, userId);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<UserPreferencesDocument>.Update
            .Pull(x => x.FavoriteSymbols, symbol)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Gets users by theme preference
    /// </summary>
    /// <param name="theme">Theme to filter by</param>
    /// <returns>Collection of user preferences business models</returns>
    public async Task<IEnumerable<UserPreferencesModel>> GetByThemeAsync(string theme)
    {
        _logger.LogInformation("Getting users with theme: {Theme}", theme);

        var filter = Builders<UserPreferencesDocument>.Filter.And(
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.Theme, theme),
            Builders<UserPreferencesDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter).ToListAsync();
        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets default preferences for a new user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Default user preferences business model</returns>
    public Task<UserPreferencesModel> GetDefaultPreferencesAsync(Guid userId)
    {
        _logger.LogInformation("Creating default preferences for user: {UserId}", userId);

        var defaultPreferences = new UserPreferencesModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Theme = "Light",
            Language = "en-US",
            Timezone = "UTC",
            CurrencyDisplay = "USD",
            DashboardLayout = new Dictionary<string, object>
            {
                ["layout"] = "default",
                ["panels"] = new List<object>()
            },
            ChartSettings = new Dictionary<string, object>
            {
                ["defaultTimeframe"] = "1h",
                ["candlestickStyle"] = "candles",
                ["showVolume"] = true
            },
            NotificationSettings = new Dictionary<string, object>
            {
                ["emailEnabled"] = true,
                ["pushEnabled"] = false,
                ["tradingAlerts"] = true
            },
            TradingSettings = new Dictionary<string, object>
            {
                ["confirmOrders"] = true,
                ["defaultOrderType"] = "limit",
                ["maxOrderSize"] = 1000
            },
            RiskPreferences = new Dictionary<string, object>
            {
                ["riskLevel"] = "medium",
                ["maxDrawdown"] = 0.10,
                ["stopLossDefault"] = 0.02
            },
            MarketOverviewCards = new Dictionary<string, object>
            {
                ["Kraken"] = new List<string> { "XBTUSD", "ETHUSD", "ADAUSD", "SOLUSD", "DOTUSD" },
                ["Kucoin"] = new List<string>()
            },
            FavoriteSymbols = [],
            FavoriteExchanges = [],
            CustomAlerts = [],
            QuickActions = ["buy", "sell", "cancel_all"],
            WorkspaceSettings = new Dictionary<string, object>
            {
                ["autoSave"] = true,
                ["showGrid"] = true
            },
            ApiSettings = new Dictionary<string, object>
            {
                ["rateLimitWarnings"] = true
            },
            PrivacySettings = new Dictionary<string, object>
            {
                ["sharePerformance"] = false,
                ["publicProfile"] = false
            }
        };

        return Task.FromResult(defaultPreferences);
    }
}