namespace QuantFlow.Data.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of algorithm repository
/// </summary>
public class AlgorithmRepository : IAlgorithmRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<AlgorithmRepository> _logger;
    private readonly IMongoCollection<AlgorithmDocument> _collection;

    public AlgorithmRepository(MongoDbContext context, ILogger<AlgorithmRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collection = _context.GetCollection<AlgorithmDocument>();
    }

    /// <summary>
    /// Gets an algorithm by its unique identifier
    /// </summary>
    /// <param name="id">The algorithm's unique identifier</param>
    /// <returns>Algorithm business model if found, null otherwise</returns>
    public async Task<AlgorithmModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm with ID: {AlgorithmId}", id);

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.Id, id),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all algorithms for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of algorithm business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithms for user: {UserId}", userId);

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets all public algorithms
    /// </summary>
    /// <returns>Collection of algorithm business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync()
    {
        _logger.LogInformation("Getting public algorithms");

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Searches algorithms by name, description, or tags
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of matching algorithm business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null)
    {
        _logger.LogInformation("Searching algorithms with term: {SearchTerm}, UserId: {UserId}", searchTerm, userId);

        var filters = new List<FilterDefinition<AlgorithmDocument>>
        {
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        };

        if (userId.HasValue)
        {
            filters.Add(Builders<AlgorithmDocument>.Filter.Eq(x => x.UserId, userId.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var textFilter = Builders<AlgorithmDocument>.Filter.Text(searchTerm);
            filters.Add(textFilter);
        }

        var filter = Builders<AlgorithmDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets algorithms by tags
    /// </summary>
    /// <param name="tags">Tags to filter by</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of algorithm business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> GetByTagsAsync(IEnumerable<string> tags, Guid? userId = null)
    {
        _logger.LogInformation("Getting algorithms by tags: {Tags}, UserId: {UserId}", string.Join(", ", tags), userId);

        var filters = new List<FilterDefinition<AlgorithmDocument>>
        {
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false),
            Builders<AlgorithmDocument>.Filter.AnyIn(x => x.Tags, tags)
        };

        if (userId.HasValue)
        {
            filters.Add(Builders<AlgorithmDocument>.Filter.Eq(x => x.UserId, userId.Value));
        }

        var filter = Builders<AlgorithmDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm business model to create</param>
    /// <returns>Created algorithm business model</returns>
    public async Task<AlgorithmModel> CreateAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Creating algorithm: {AlgorithmName} for user: {UserId}", algorithm.Name, algorithm.UserId);

        if (algorithm.Id == Guid.Empty)
            algorithm.Id = Guid.NewGuid();

        algorithm.CreatedAt = DateTime.UtcNow;
        algorithm.UpdatedAt = DateTime.UtcNow;

        var document = algorithm.ToDocument();
        await _collection.InsertOneAsync(document);

        _logger.LogInformation("Algorithm created with ID: {AlgorithmId}", algorithm.Id);
        return document.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm business model with updated values</param>
    /// <returns>Updated algorithm business model</returns>
    public async Task<AlgorithmModel> UpdateAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Updating algorithm: {AlgorithmId}", algorithm.Id);

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.Id, algorithm.Id),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            throw new InvalidOperationException($"Algorithm with ID {algorithm.Id} not found");
        }

        algorithm.UpdatedAt = DateTime.UtcNow;
        existingDocument.UpdateFromModel(algorithm);

        await _collection.ReplaceOneAsync(filter, existingDocument);

        _logger.LogInformation("Algorithm updated: {AlgorithmId}", algorithm.Id);
        return existingDocument.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes an algorithm
    /// </summary>
    /// <param name="id">Algorithm ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Soft deleting algorithm: {AlgorithmId}", id);

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.Id, id),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<AlgorithmDocument>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Algorithm soft deleted: {AlgorithmId}", id);
        }
        else
        {
            _logger.LogWarning("Algorithm not found for deletion: {AlgorithmId}", id);
        }

        return success;
    }

    /// <summary>
    /// Gets algorithms by status
    /// </summary>
    /// <param name="status">Algorithm status to filter by</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of algorithm business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> GetByStatusAsync(AlgorithmStatus status, Guid? userId = null)
    {
        _logger.LogInformation("Getting algorithms by status: {Status}, UserId: {UserId}", status, userId);

        var filters = new List<FilterDefinition<AlgorithmDocument>>
        {
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.Status, (int)status)
        };

        if (userId.HasValue)
        {
            filters.Add(Builders<AlgorithmDocument>.Filter.Eq(x => x.UserId, userId.Value));
        }

        var filter = Builders<AlgorithmDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets algorithm templates
    /// </summary>
    /// <returns>Collection of algorithm template business models</returns>
    public async Task<IEnumerable<AlgorithmModel>> GetTemplatesAsync()
    {
        _logger.LogInformation("Getting algorithm templates");

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsTemplate, true),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortBy(x => x.TemplateCategory)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Counts algorithms for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of algorithms for the user</returns>
    public async Task<long> CountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Counting algorithms for user: {UserId}", userId);

        var filter = Builders<AlgorithmDocument>.Filter.And(
            Builders<AlgorithmDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<AlgorithmDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await _collection.CountDocumentsAsync(filter);
    }
}