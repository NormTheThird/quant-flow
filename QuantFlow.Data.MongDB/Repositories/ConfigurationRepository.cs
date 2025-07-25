namespace QuantFlow.Data.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of configuration repository
/// </summary>
public class ConfigurationRepository : IConfigurationRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<ConfigurationRepository> _logger;
    private readonly IMongoCollection<ConfigurationDocument> _collection;

    public ConfigurationRepository(MongoDbContext context, ILogger<ConfigurationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collection = _context.GetCollection<ConfigurationDocument>();
    }

    /// <summary>
    /// Gets a configuration by key
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>Configuration business model if found, null otherwise</returns>
    public async Task<ConfigurationModel?> GetByKeyAsync(string key)
    {
        _logger.LogInformation("Getting configuration with key: {Key}", key);

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Key, key),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Gets configurations by category
    /// </summary>
    /// <param name="category">Configuration category</param>
    /// <param name="subcategory">Optional subcategory</param>
    /// <returns>Collection of configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetByCategoryAsync(string category, string? subcategory = null)
    {
        _logger.LogInformation("Getting configurations for category: {Category}, subcategory: {Subcategory}", category, subcategory);

        var filters = new List<FilterDefinition<ConfigurationDocument>>
        {
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Category, category),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        };

        if (!string.IsNullOrWhiteSpace(subcategory))
        {
            filters.Add(Builders<ConfigurationDocument>.Filter.Eq(x => x.Subcategory, subcategory));
        }

        var filter = Builders<ConfigurationDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets configurations by environment
    /// </summary>
    /// <param name="environment">Environment name</param>
    /// <returns>Collection of configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetByEnvironmentAsync(string environment)
    {
        _logger.LogInformation("Getting configurations for environment: {Environment}", environment);

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Environment, environment),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets all configurations
    /// </summary>
    /// <returns>Collection of configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all configurations");

        var filter = Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false);

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets user-configurable settings
    /// </summary>
    /// <returns>Collection of user-configurable configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetUserConfigurableAsync()
    {
        _logger.LogInformation("Getting user-configurable configurations");

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsUserConfigurable, true),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets effective configurations (considering effective and expiry dates)
    /// </summary>
    /// <param name="asOfDate">Date to check effectiveness</param>
    /// <returns>Collection of effective configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetEffectiveConfigurationsAsync(DateTime? asOfDate = null)
    {
        var checkDate = asOfDate ?? DateTime.UtcNow;
        _logger.LogInformation("Getting effective configurations as of: {AsOfDate}", checkDate);

        var filters = new List<FilterDefinition<ConfigurationDocument>>
        {
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        };

        // Effective date filter (null or <= checkDate)
        var effectiveDateFilter = Builders<ConfigurationDocument>.Filter.Or(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.EffectiveDate, null),
            Builders<ConfigurationDocument>.Filter.Lte(x => x.EffectiveDate, checkDate)
        );
        filters.Add(effectiveDateFilter);

        // Expiry date filter (null or > checkDate)
        var expiryDateFilter = Builders<ConfigurationDocument>.Filter.Or(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.ExpiryDate, null),
            Builders<ConfigurationDocument>.Filter.Gt(x => x.ExpiryDate, checkDate)
        );
        filters.Add(expiryDateFilter);

        var filter = Builders<ConfigurationDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new configuration
    /// </summary>
    /// <param name="configuration">Configuration business model to create</param>
    /// <returns>Created configuration business model</returns>
    public async Task<ConfigurationModel> CreateAsync(ConfigurationModel configuration)
    {
        _logger.LogInformation("Creating configuration: {Key}", configuration.Key);

        if (configuration.Id == Guid.Empty)
            configuration.Id = Guid.NewGuid();

        configuration.CreatedAt = DateTime.UtcNow;
        configuration.UpdatedAt = null;

        var document = configuration.ToDocument();
        await _collection.InsertOneAsync(document);

        _logger.LogInformation("Configuration created with key: {Key}", configuration.Key);
        return document.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing configuration
    /// </summary>
    /// <param name="configuration">Configuration business model with updated values</param>
    /// <returns>Updated configuration business model</returns>
    public async Task<ConfigurationModel> UpdateAsync(ConfigurationModel configuration)
    {
        _logger.LogInformation("Updating configuration: {Key}", configuration.Key);

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Key, configuration.Key),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            throw new InvalidOperationException($"Configuration with key {configuration.Key} not found");
        }

        configuration.UpdatedAt = DateTime.UtcNow;
        configuration.Version = existingDocument.Version + 1;
        existingDocument.UpdateFromModel(configuration);

        await _collection.ReplaceOneAsync(filter, existingDocument);

        _logger.LogInformation("Configuration updated: {Key}", configuration.Key);
        return existingDocument.ToBusinessModel();
    }

    /// <summary>
    /// Creates or updates a configuration (upsert operation)
    /// </summary>
    /// <param name="configuration">Configuration business model</param>
    /// <returns>Created or updated configuration business model</returns>
    public async Task<ConfigurationModel> UpsertAsync(ConfigurationModel configuration)
    {
        _logger.LogInformation("Upserting configuration: {Key}", configuration.Key);

        var existing = await GetByKeyAsync(configuration.Key);
        if (existing != null)
        {
            configuration.Id = existing.Id;
            configuration.CreatedAt = existing.CreatedAt;
            configuration.Version = existing.Version;
            return await UpdateAsync(configuration);
        }
        else
        {
            return await CreateAsync(configuration);
        }
    }

    /// <summary>
    /// Soft deletes a configuration
    /// </summary>
    /// <param name="key">Configuration key to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string key)
    {
        _logger.LogInformation("Soft deleting configuration: {Key}", key);

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Key, key),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<ConfigurationDocument>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Configuration soft deleted: {Key}", key);
        }
        else
        {
            _logger.LogWarning("Configuration not found for deletion: {Key}", key);
        }

        return success;
    }

    /// <summary>
    /// Updates configuration value by key
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">New value</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateValueAsync(string key, object value)
    {
        _logger.LogInformation("Updating configuration value for key: {Key}", key);

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.Eq(x => x.Key, key),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var bsonValue = BsonValue.Create(value);
        var update = Builders<ConfigurationDocument>.Update
            .Set(x => x.Value, bsonValue)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Inc(x => x.Version, 1);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Configuration value updated for key: {Key}", key);
        }
        else
        {
            _logger.LogWarning("Failed to update configuration value for key: {Key}", key);
        }

        return success;
    }

    /// <summary>
    /// Gets configurations by tags
    /// </summary>
    /// <param name="tags">Tags to filter by</param>
    /// <returns>Collection of configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> GetByTagsAsync(IEnumerable<string> tags)
    {
        _logger.LogInformation("Getting configurations by tags: {Tags}", string.Join(", ", tags));

        var filter = Builders<ConfigurationDocument>.Filter.And(
            Builders<ConfigurationDocument>.Filter.AnyIn(x => x.Tags, tags),
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Searches configurations by key or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Collection of matching configuration business models</returns>
    public async Task<IEnumerable<ConfigurationModel>> SearchAsync(string searchTerm)
    {
        _logger.LogInformation("Searching configurations with term: {SearchTerm}", searchTerm);

        var filters = new List<FilterDefinition<ConfigurationDocument>>
        {
            Builders<ConfigurationDocument>.Filter.Eq(x => x.IsDeleted, false)
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = Builders<ConfigurationDocument>.Filter.Or(
                Builders<ConfigurationDocument>.Filter.Regex(x => x.Key, new BsonRegularExpression(searchTerm, "i")),
                Builders<ConfigurationDocument>.Filter.Regex(x => x.Description, new BsonRegularExpression(searchTerm, "i"))
            );
            filters.Add(searchFilter);
        }

        var filter = Builders<ConfigurationDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return documents.ToBusinessModels();
    }
}