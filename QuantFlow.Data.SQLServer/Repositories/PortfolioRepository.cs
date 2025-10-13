namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of portfolio repository
/// </summary>
public class PortfolioRepository : IPortfolioRepository
{
    private readonly QuantFlowDbContext _context;
    private readonly ILogger<PortfolioRepository> _logger;

    public PortfolioRepository(QuantFlowDbContext context, ILogger<PortfolioRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PortfolioModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting portfolio with ID: {PortfolioId}", id);

        var entity = await _context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<PortfolioModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting portfolios for user: {UserId}", userId);

        var entities = await _context.Portfolios
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<PortfolioModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all portfolios");

        var entities = await _context.Portfolios
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<PortfolioModel> CreateAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Creating portfolio: {Name} for user: {UserId}", portfolio.Name, portfolio.UserId);

        var entity = portfolio.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Portfolios.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<PortfolioModel> UpdateAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Updating portfolio with ID: {PortfolioId}", portfolio.Id);

        var entity = await _context.Portfolios.FindAsync(portfolio.Id);
        if (entity == null)
            throw new NotFoundException($"Portfolio with ID {portfolio.Id} not found");

        entity.Name = portfolio.Name;
        entity.Description = portfolio.Description;
        entity.CurrentBalance = portfolio.CurrentBalance;
        entity.Status = portfolio.Status.ToString();
        entity.Mode = portfolio.Mode.ToString();
        entity.Exchange = portfolio.Exchange.ToString();
        entity.UserExchangeDetailsId = portfolio.UserExchangeDetailsId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = portfolio.UpdatedBy;

        await _context.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting portfolio with ID: {PortfolioId}", id);

        var entity = await _context.Portfolios.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Counting portfolios for user: {UserId}", userId);

        return await _context.Portfolios
            .CountAsync(p => p.UserId == userId && !p.IsDeleted);
    }
}