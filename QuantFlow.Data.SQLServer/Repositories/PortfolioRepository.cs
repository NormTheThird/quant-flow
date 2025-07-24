namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of portfolio repository
/// </summary>
public class PortfolioRepository : IPortfolioRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PortfolioRepository> _logger;

    public PortfolioRepository(ApplicationDbContext context, ILogger<PortfolioRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a portfolio by its unique identifier
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>Portfolio business model if found, null otherwise</returns>
    public async Task<PortfolioModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting portfolio with ID: {PortfolioId}", id);

        var entity = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all portfolios for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of portfolio business models</returns>
    public async Task<IEnumerable<PortfolioModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting portfolios for user: {UserId}", userId);

        var entities = await _context.Portfolios
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets all active portfolios
    /// </summary>
    /// <returns>Collection of portfolio business models</returns>
    public async Task<IEnumerable<PortfolioModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all portfolios");

        var entities = await _context.Portfolios
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio business model to create</param>
    /// <returns>Created portfolio business model</returns>
    public async Task<PortfolioModel> CreateAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Creating portfolio: {Name} for user: {UserId}", portfolio.Name, portfolio.UserId);

        var entity = portfolio.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;

        _context.Portfolios.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio business model with updates</param>
    /// <returns>Updated portfolio business model</returns>
    public async Task<PortfolioModel> UpdateAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Updating portfolio with ID: {PortfolioId}", portfolio.Id);

        var entity = await _context.Portfolios.FindAsync(portfolio.Id);
        if (entity == null)
            throw new NotFoundException($"Portfolio with ID {portfolio.Id} not found");

        entity.Name = portfolio.Name;
        entity.Description = portfolio.Description;
        entity.CurrentBalance = portfolio.CurrentBalance;
        entity.Status = (int)portfolio.Status;
        entity.MaxPositionSizePercent = portfolio.MaxPositionSizePercent;
        entity.CommissionRate = portfolio.CommissionRate;
        entity.AllowShortSelling = portfolio.AllowShortSelling;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes a portfolio
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
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

    /// <summary>
    /// Counts the number of active portfolios for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Number of active portfolios</returns>
    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Counting portfolios for user: {UserId}", userId);

        return await _context.Portfolios
            .CountAsync(p => p.UserId == userId && !p.IsDeleted);
    }
}