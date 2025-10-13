namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of algorithm position repository
/// </summary>
public class AlgorithmPositionRepository : IAlgorithmPositionRepository
{
    private readonly ILogger<AlgorithmPositionRepository> _logger;
    private readonly QuantFlowDbContext _context;

    public AlgorithmPositionRepository(
        ILogger<AlgorithmPositionRepository> logger,
        QuantFlowDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AlgorithmPositionModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm position with ID: {PositionId}", id);

        var entity = await _context.AlgorithmPositions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<AlgorithmPositionModel>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        _logger.LogInformation("Getting algorithm positions for portfolio: {PortfolioId}", portfolioId);

        var entities = await _context.AlgorithmPositions
            .AsNoTracking()
            .Where(p => p.PortfolioId == portfolioId && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<AlgorithmPositionModel> CreateAsync(AlgorithmPositionModel position)
    {
        _logger.LogInformation("Creating algorithm position for portfolio: {PortfolioId}", position.PortfolioId);

        var entity = position.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.AlgorithmPositions.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<AlgorithmPositionModel> UpdateAsync(AlgorithmPositionModel position)
    {
        _logger.LogInformation("Updating algorithm position: {PositionId}", position.Id);

        var entity = await _context.AlgorithmPositions.FindAsync(position.Id);
        if (entity == null)
            throw new NotFoundException($"Algorithm position with ID {position.Id} not found");

        // Update with CORRECT property names from the model
        entity.PositionName = position.PositionName;
        entity.AlgorithmId = position.AlgorithmId;
        entity.Status = position.Status.ToString(); // Convert enum to string
        entity.AllocatedPercent = position.AllocatedPercent;
        entity.MaxPositionSizePercent = position.MaxPositionSizePercent;
        entity.ExchangeFees = position.ExchangeFees;
        entity.AllowShortSelling = position.AllowShortSelling;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = position.UpdatedBy;

        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting algorithm position: {PositionId}", id);

        var entity = await _context.AlgorithmPositions.FindAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("Algorithm position not found: {PositionId}", id);
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}