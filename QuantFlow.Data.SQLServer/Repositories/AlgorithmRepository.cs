namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of algorithm repository
/// </summary>
public class AlgorithmRepository : IAlgorithmRepository
{
    private readonly ILogger<AlgorithmRepository> _logger;
    private readonly QuantFlowDbContext _context;

    public AlgorithmRepository(
        ILogger<AlgorithmRepository> logger,
        QuantFlowDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AlgorithmMetadataModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm with ID: {AlgorithmId}", id);

        var entity = await _context.Algorithms
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.Id == id);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<AlgorithmMetadataModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all algorithms");

        var entities = await _context.Algorithms
            .AsNoTracking()
            .OrderBy(_ => _.Name)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<AlgorithmMetadataModel>> GetEnabledAsync()
    {
        _logger.LogInformation("Getting enabled algorithms");

        var entities = await _context.Algorithms
            .AsNoTracking()
            .Where(_ => _.IsEnabled)
            .OrderBy(_ => _.Name)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<AlgorithmMetadataModel>> GetBySourceAsync(AlgorithmSource source)
    {
        _logger.LogInformation("Getting algorithms by source: {Source}", source);

        var entities = await _context.Algorithms
            .AsNoTracking()
            .Where(_ => _.AlgorithmSource == (int)source)
            .OrderBy(_ => _.Name)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<AlgorithmMetadataModel>> GetByTypeAsync(AlgorithmType type)
    {
        _logger.LogInformation("Getting algorithms by type: {Type}", type);

        var entities = await _context.Algorithms
            .AsNoTracking()
            .Where(_ => _.AlgorithmType == (int)type)
            .OrderBy(_ => _.Name)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<AlgorithmMetadataModel> CreateAsync(AlgorithmMetadataModel algorithm)
    {
        _logger.LogInformation("Creating algorithm: {Name}", algorithm.Name);

        var entity = algorithm.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Algorithms.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<AlgorithmMetadataModel> UpdateAsync(AlgorithmMetadataModel algorithm)
    {
        _logger.LogInformation("Updating algorithm: {AlgorithmId}", algorithm.Id);

        var entity = await _context.Algorithms.FindAsync(algorithm.Id);
        if (entity == null)
            throw new NotFoundException($"Algorithm with ID {algorithm.Id} not found");

        entity.Name = algorithm.Name;
        entity.Abbreviation = algorithm.Abbreviation;
        entity.Description = algorithm.Description;
        entity.AlgorithmType = (int)algorithm.AlgorithmType;
        entity.AlgorithmSource = (int)algorithm.AlgorithmSource;
        entity.IsEnabled = algorithm.IsEnabled;
        entity.Version = algorithm.Version;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = algorithm.UpdatedBy;

        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting algorithm: {AlgorithmId}", id);

        var entity = await _context.Algorithms.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetEnabledAsync(Guid id, bool isEnabled)
    {
        _logger.LogInformation("Setting algorithm {AlgorithmId} enabled status to: {IsEnabled}", id, isEnabled);

        var entity = await _context.Algorithms.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsEnabled = isEnabled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}