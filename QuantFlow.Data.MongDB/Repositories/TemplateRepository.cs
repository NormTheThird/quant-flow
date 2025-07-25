namespace QuantFlow.Data.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of template repository
/// </summary>
public class TemplateRepository : ITemplateRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<TemplateRepository> _logger;
    private readonly IMongoCollection<TemplateDocument> _collection;

    public TemplateRepository(MongoDbContext context, ILogger<TemplateRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collection = _context.GetCollection<TemplateDocument>();
    }

    /// <summary>
    /// Gets a template by its unique identifier
    /// </summary>
    /// <param name="id">The template's unique identifier</param>
    /// <returns>Template business model if found, null otherwise</returns>
    public async Task<TemplateModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting template with ID: {TemplateId}", id);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Id, id),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Gets a template by its name
    /// </summary>
    /// <param name="name">The template's name</param>
    /// <returns>Template business model if found, null otherwise</returns>
    public async Task<TemplateModel?> GetByNameAsync(string name)
    {
        _logger.LogInformation("Getting template with name: {TemplateName}", name);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Name, name),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document?.ToBusinessModel();
    }

    /// <summary>
    /// Gets templates by category
    /// </summary>
    /// <param name="category">Template category</param>
    /// <param name="subcategory">Optional subcategory</param>
    /// <returns>Collection of template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetByCategoryAsync(string category, string? subcategory = null)
    {
        _logger.LogInformation("Getting templates for category: {Category}, subcategory: {Subcategory}", category, subcategory);

        var filters = new List<FilterDefinition<TemplateDocument>>
        {
            Builders<TemplateDocument>.Filter.Eq(x => x.Category, category),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        };

        if (!string.IsNullOrWhiteSpace(subcategory))
        {
            filters.Add(Builders<TemplateDocument>.Filter.Eq(x => x.Subcategory, subcategory));
        }

        var filter = Builders<TemplateDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets all public templates
    /// </summary>
    /// <returns>Collection of public template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetPublicTemplatesAsync()
    {
        _logger.LogInformation("Getting public templates");

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets featured templates
    /// </summary>
    /// <returns>Collection of featured template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetFeaturedTemplatesAsync()
    {
        _logger.LogInformation("Getting featured templates");

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.IsFeatured, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Searches templates by name, description, or tags
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Collection of matching template business models</returns>
    public async Task<IEnumerable<TemplateModel>> SearchTemplatesAsync(string searchTerm)
    {
        _logger.LogInformation("Searching templates with term: {SearchTerm}", searchTerm);

        var filters = new List<FilterDefinition<TemplateDocument>>
        {
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true)
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var textFilter = Builders<TemplateDocument>.Filter.Text(searchTerm);
            filters.Add(textFilter);
        }

        var filter = Builders<TemplateDocument>.Filter.And(filters);

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets templates by tags
    /// </summary>
    /// <param name="tags">Tags to filter by</param>
    /// <returns>Collection of template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetByTagsAsync(IEnumerable<string> tags)
    {
        _logger.LogInformation("Getting templates by tags: {Tags}", string.Join(", ", tags));

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.AnyIn(x => x.Tags, tags),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets templates by difficulty level
    /// </summary>
    /// <param name="difficultyLevel">Difficulty level (beginner, intermediate, advanced)</param>
    /// <returns>Collection of template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetByDifficultyLevelAsync(string difficultyLevel)
    {
        _logger.LogInformation("Getting templates by difficulty level: {DifficultyLevel}", difficultyLevel);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.DifficultyLevel, difficultyLevel),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Gets templates by risk level
    /// </summary>
    /// <param name="riskLevel">Risk level (low, medium, high)</param>
    /// <returns>Collection of template business models</returns>
    public async Task<IEnumerable<TemplateModel>> GetByRiskLevelAsync(string riskLevel)
    {
        _logger.LogInformation("Getting templates by risk level: {RiskLevel}", riskLevel);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.RiskLevel, riskLevel),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsPublic, true),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.Rating)
            .ThenByDescending(x => x.DownloadCount)
            .ToListAsync();

        return documents.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new template
    /// </summary>
    /// <param name="template">Template business model to create</param>
    /// <returns>Created template business model</returns>
    public async Task<TemplateModel> CreateAsync(TemplateModel template)
    {
        _logger.LogInformation("Creating template: {TemplateName}", template.Name);

        if (template.Id == Guid.Empty)
            template.Id = Guid.NewGuid();

        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = null;

        var document = template.ToDocument();
        await _collection.InsertOneAsync(document);

        _logger.LogInformation("Template created with ID: {TemplateId}", template.Id);
        return document.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing template
    /// </summary>
    /// <param name="template">Template business model with updated values</param>
    /// <returns>Updated template business model</returns>
    public async Task<TemplateModel> UpdateAsync(TemplateModel template)
    {
        _logger.LogInformation("Updating template: {TemplateId}", template.Id);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Id, template.Id),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            throw new InvalidOperationException($"Template with ID {template.Id} not found");
        }

        template.UpdatedAt = DateTime.UtcNow;
        existingDocument.UpdateFromModel(template);

        await _collection.ReplaceOneAsync(filter, existingDocument);

        _logger.LogInformation("Template updated: {TemplateId}", template.Id);
        return existingDocument.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes a template
    /// </summary>
    /// <param name="id">Template ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Soft deleting template: {TemplateId}", id);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Id, id),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<TemplateDocument>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Template soft deleted: {TemplateId}", id);
        }
        else
        {
            _logger.LogWarning("Template not found for deletion: {TemplateId}", id);
        }

        return success;
    }

    /// <summary>
    /// Increments the download count for a template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> IncrementDownloadCountAsync(Guid id)
    {
        _logger.LogInformation("Incrementing download count for template: {TemplateId}", id);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Id, id),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<TemplateDocument>.Update
            .Inc(x => x.DownloadCount, 1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Download count incremented for template: {TemplateId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to increment download count for template: {TemplateId}", id);
        }

        return success;
    }

    /// <summary>
    /// Updates the rating and review count for a template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="rating">New average rating</param>
    /// <param name="reviewCount">New review count</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateRatingAsync(Guid id, decimal rating, int reviewCount)
    {
        _logger.LogInformation("Updating rating for template: {TemplateId} to {Rating} with {ReviewCount} reviews",
            id, rating, reviewCount);

        var filter = Builders<TemplateDocument>.Filter.And(
            Builders<TemplateDocument>.Filter.Eq(x => x.Id, id),
            Builders<TemplateDocument>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<TemplateDocument>.Update
            .Set(x => x.Rating, rating)
            .Set(x => x.ReviewCount, reviewCount)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);

        var success = result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Rating updated for template: {TemplateId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to update rating for template: {TemplateId}", id);
        }

        return success;
    }
}