namespace QuantFlow.Data.MongoDB.Context;

/// <summary>
/// MongoDB context for database operations and collection management
/// </summary>
public class MongoDbContext : IDisposable
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbContext> _logger;
    private bool _disposed = false;

    public MongoDbContext(IMongoClient mongoClient, string databaseName, ILogger<MongoDbContext> logger)
    {
        _database = mongoClient?.GetDatabase(databaseName) ?? throw new ArgumentNullException(nameof(mongoClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeIndexes();
    }

    /// <summary>
    /// Gets a MongoDB collection for the specified document type
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    /// <returns>MongoDB collection</returns>
    public IMongoCollection<T> GetCollection<T>() where T : class
    {
        ThrowIfDisposed();
        var collectionName = GetCollectionName<T>();
        _logger.LogDebug("Getting collection: {CollectionName} for type: {TypeName}", collectionName, typeof(T).Name);
        return _database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Gets the collection name for a document type
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    /// <returns>Collection name</returns>
    private static string GetCollectionName<T>()
    {
        var attribute = typeof(T).GetCustomAttribute<BsonCollectionAttribute>();
        return attribute?.CollectionName ?? typeof(T).Name.ToLowerInvariant();
    }

    /// <summary>
    /// Initializes database indexes for optimal performance
    /// </summary>
    private void InitializeIndexes()
    {
        try
        {
            CreateAlgorithmIndexes();
            CreateUserPreferencesIndexes();
            CreateConfigurationIndexes();
            CreateTemplateIndexes();
            _logger.LogInformation("MongoDB indexes initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB indexes");
        }
    }

    /// <summary>
    /// Creates indexes for algorithm collection
    /// </summary>
    private void CreateAlgorithmIndexes()
    {
        var collection = GetCollection<AlgorithmDocument>();

        // User ID index for user's algorithms
        var userIdIndex = Builders<AlgorithmDocument>.IndexKeys.Ascending(x => x.UserId);
        collection.Indexes.CreateOne(new CreateIndexModel<AlgorithmDocument>(userIdIndex,
            new CreateIndexOptions { Name = "idx_algorithms_user_id" }));

        // Status index for filtering
        var statusIndex = Builders<AlgorithmDocument>.IndexKeys.Ascending(x => x.Status);
        collection.Indexes.CreateOne(new CreateIndexModel<AlgorithmDocument>(statusIndex,
            new CreateIndexOptions { Name = "idx_algorithms_status" }));

        // Name text index for searching
        var nameIndex = Builders<AlgorithmDocument>.IndexKeys.Text(x => x.Name).Text(x => x.Description);
        collection.Indexes.CreateOne(new CreateIndexModel<AlgorithmDocument>(nameIndex,
            new CreateIndexOptions { Name = "idx_algorithms_text_search" }));

        // Tags index for filtering by algorithm type
        var tagsIndex = Builders<AlgorithmDocument>.IndexKeys.Ascending(x => x.Tags);
        collection.Indexes.CreateOne(new CreateIndexModel<AlgorithmDocument>(tagsIndex,
            new CreateIndexOptions { Name = "idx_algorithms_tags" }));

        // Public algorithms index
        var publicIndex = Builders<AlgorithmDocument>.IndexKeys
            .Ascending(x => x.IsPublic)
            .Ascending(x => x.IsDeleted);
        collection.Indexes.CreateOne(new CreateIndexModel<AlgorithmDocument>(publicIndex,
            new CreateIndexOptions { Name = "idx_algorithms_public" }));
    }

    /// <summary>
    /// Creates indexes for user preferences collection
    /// </summary>
    private void CreateUserPreferencesIndexes()
    {
        var collection = GetCollection<UserPreferencesDocument>();

        // Unique user ID index
        var userIdIndex = Builders<UserPreferencesDocument>.IndexKeys.Ascending(x => x.UserId);
        collection.Indexes.CreateOne(new CreateIndexModel<UserPreferencesDocument>(userIdIndex,
            new CreateIndexOptions { Name = "idx_user_preferences_user_id", Unique = true }));
    }

    /// <summary>
    /// Creates indexes for configuration collection
    /// </summary>
    private void CreateConfigurationIndexes()
    {
        var collection = GetCollection<ConfigurationDocument>();

        // Unique key index
        var keyIndex = Builders<ConfigurationDocument>.IndexKeys.Ascending(x => x.Key);
        collection.Indexes.CreateOne(new CreateIndexModel<ConfigurationDocument>(keyIndex,
            new CreateIndexOptions { Name = "idx_configurations_key", Unique = true }));

        // Category index
        var categoryIndex = Builders<ConfigurationDocument>.IndexKeys
            .Ascending(x => x.Category)
            .Ascending(x => x.Subcategory);
        collection.Indexes.CreateOne(new CreateIndexModel<ConfigurationDocument>(categoryIndex,
            new CreateIndexOptions { Name = "idx_configurations_category" }));

        // Environment index
        var envIndex = Builders<ConfigurationDocument>.IndexKeys.Ascending(x => x.Environment);
        collection.Indexes.CreateOne(new CreateIndexModel<ConfigurationDocument>(envIndex,
            new CreateIndexOptions { Name = "idx_configurations_environment" }));

        // Effective date index for time-based configurations
        var effectiveDateIndex = Builders<ConfigurationDocument>.IndexKeys
            .Ascending(x => x.EffectiveDate)
            .Ascending(x => x.ExpiryDate);
        collection.Indexes.CreateOne(new CreateIndexModel<ConfigurationDocument>(effectiveDateIndex,
            new CreateIndexOptions { Name = "idx_configurations_effective_date" }));
    }

    /// <summary>
    /// Creates indexes for template collection
    /// </summary>
    private void CreateTemplateIndexes()
    {
        var collection = GetCollection<TemplateDocument>();

        // Category index
        var categoryIndex = Builders<TemplateDocument>.IndexKeys
            .Ascending(x => x.Category)
            .Ascending(x => x.Subcategory);
        collection.Indexes.CreateOne(new CreateIndexModel<TemplateDocument>(categoryIndex,
            new CreateIndexOptions { Name = "idx_templates_category" }));

        // Public templates index
        var publicIndex = Builders<TemplateDocument>.IndexKeys
            .Ascending(x => x.IsPublic)
            .Descending(x => x.Rating)
            .Descending(x => x.DownloadCount);
        collection.Indexes.CreateOne(new CreateIndexModel<TemplateDocument>(publicIndex,
            new CreateIndexOptions { Name = "idx_templates_public_rating" }));

        // Featured templates index
        var featuredIndex = Builders<TemplateDocument>.IndexKeys.Ascending(x => x.IsFeatured);
        collection.Indexes.CreateOne(new CreateIndexModel<TemplateDocument>(featuredIndex,
            new CreateIndexOptions { Name = "idx_templates_featured" }));

        // Text search index
        var textIndex = Builders<TemplateDocument>.IndexKeys
            .Text(x => x.Name)
            .Text(x => x.DisplayName)
            .Text(x => x.Description);
        collection.Indexes.CreateOne(new CreateIndexModel<TemplateDocument>(textIndex,
            new CreateIndexOptions { Name = "idx_templates_text_search" }));

        // Tags index
        var tagsIndex = Builders<TemplateDocument>.IndexKeys.Ascending(x => x.Tags);
        collection.Indexes.CreateOne(new CreateIndexModel<TemplateDocument>(tagsIndex,
            new CreateIndexOptions { Name = "idx_templates_tags" }));
    }

    /// <summary>
    /// Checks if the MongoDB connection is healthy
    /// </summary>
    /// <returns>True if connection is healthy</returns>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            ThrowIfDisposed();
            var pingCommand = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(pingCommand);
            _logger.LogDebug("MongoDB health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            return false;
        }
    }

    /// <summary>
    /// Gets database statistics
    /// </summary>
    /// <returns>Database statistics</returns>
    public async Task<BsonDocument> GetDatabaseStatsAsync()
    {
        ThrowIfDisposed();
        var statsCommand = new BsonDocument("dbStats", 1);
        return await _database.RunCommandAsync<BsonDocument>(statsCommand);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MongoDbContext));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _logger.LogDebug("MongoDbContext disposed");
        }
    }
}