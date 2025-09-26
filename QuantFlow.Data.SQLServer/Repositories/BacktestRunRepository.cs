//namespace QuantFlow.Data.SQLServer.Repositories;

///// <summary>
///// SQL Server implementation of backtest run repository
///// </summary>
//public class BacktestRunRepository : IBacktestRunRepository
//{
//    private readonly ApplicationDbContext _context;
//    private readonly ILogger<BacktestRunRepository> _logger;

//    public BacktestRunRepository(ApplicationDbContext context, ILogger<BacktestRunRepository> logger)
//    {
//        _context = context ?? throw new ArgumentNullException(nameof(context));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    /// <summary>
//    /// Gets a backtest run by its unique identifier
//    /// </summary>
//    /// <param name="id">The backtest run's unique identifier</param>
//    /// <returns>BacktestRun business model if found, null otherwise</returns>
//    public async Task<BacktestRunModel?> GetByIdAsync(Guid id)
//    {
//        _logger.LogInformation("Getting backtest run with ID: {BacktestRunId}", id);

//        var entity = await _context.BacktestRuns
//            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

//        return entity?.ToBusinessModel();
//    }

//    /// <summary>
//    /// Gets all backtest runs for a specific user
//    /// </summary>
//    /// <param name="userId">The user's unique identifier</param>
//    /// <returns>Collection of backtest run business models</returns>
//    public async Task<IEnumerable<BacktestRunModel>> GetByUserIdAsync(Guid userId)
//    {
//        _logger.LogInformation("Getting backtest runs for user: {UserId}", userId);

//        var entities = await _context.BacktestRuns
//            .Where(b => b.UserId == userId && !b.IsDeleted)
//            .OrderByDescending(b => b.CreatedAt)
//            .ToListAsync();

//        return entities.ToBusinessModels();
//    }

//    /// <summary>
//    /// Gets all backtest runs for a specific portfolio
//    /// </summary>
//    /// <param name="portfolioId">The portfolio's unique identifier</param>
//    /// <returns>Collection of backtest run business models</returns>
//    public async Task<IEnumerable<BacktestRunModel>> GetByPortfolioIdAsync(Guid portfolioId)
//    {
//        _logger.LogInformation("Getting backtest runs for portfolio: {PortfolioId}", portfolioId);

//        var entities = await _context.BacktestRuns
//            .Where(b => b.PortfolioId == portfolioId && !b.IsDeleted)
//            .OrderByDescending(b => b.CreatedAt)
//            .ToListAsync();

//        return entities.ToBusinessModels();
//    }

//    /// <summary>
//    /// Gets all active backtest runs
//    /// </summary>
//    /// <returns>Collection of backtest run business models</returns>
//    public async Task<IEnumerable<BacktestRunModel>> GetAllAsync()
//    {
//        _logger.LogInformation("Getting all backtest runs");

//        var entities = await _context.BacktestRuns
//            .Where(b => !b.IsDeleted)
//            .OrderByDescending(b => b.CreatedAt)
//            .ToListAsync();

//        return entities.ToBusinessModels();
//    }

//    /// <summary>
//    /// Creates a new backtest run
//    /// </summary>
//    /// <param name="backtestRun">BacktestRun business model to create</param>
//    /// <returns>Created backtest run business model</returns>
//    public async Task<BacktestRunModel> CreateAsync(BacktestRunModel backtestRun)
//    {
//        _logger.LogInformation("Creating backtest run: {Name}", backtestRun.Name);

//        var entity = backtestRun.ToEntity();
//        entity.CreatedAt = DateTime.UtcNow;

//        _context.BacktestRuns.Add(entity);
//        await _context.SaveChangesAsync();

//        return entity.ToBusinessModel();
//    }

//    /// <summary>
//    /// Updates an existing backtest run
//    /// </summary>
//    /// <param name="backtestRun">BacktestRun business model with updates</param>
//    /// <returns>Updated backtest run business model</returns>
//    public async Task<BacktestRunModel> UpdateAsync(BacktestRunModel backtestRun)
//    {
//        _logger.LogInformation("Updating backtest run with ID: {BacktestRunId}", backtestRun.Id);

//        var entity = await _context.BacktestRuns.FindAsync(backtestRun.Id);
//        if (entity == null)
//            throw new NotFoundException($"Backtest run with ID {backtestRun.Id} not found");

//        entity.Name = backtestRun.Name;
//        entity.Status = (int)backtestRun.Status;
//        entity.FinalBalance = backtestRun.FinalBalance;
//        entity.TotalReturnPercent = backtestRun.TotalReturnPercent;
//        entity.MaxDrawdownPercent = backtestRun.MaxDrawdownPercent;
//        entity.SharpeRatio = backtestRun.SharpeRatio;
//        entity.TotalTrades = backtestRun.TotalTrades;
//        entity.WinningTrades = backtestRun.WinningTrades;
//        entity.LosingTrades = backtestRun.LosingTrades;
//        entity.WinRatePercent = backtestRun.WinRatePercent;
//        entity.AverageTradeReturnPercent = backtestRun.AverageTradeReturnPercent;
//        entity.ExecutionDurationTicks = backtestRun.ExecutionDuration.Ticks;
//        entity.ErrorMessage = backtestRun.ErrorMessage;
//        entity.CompletedAt = backtestRun.CompletedAt;
//        entity.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();
//        return entity.ToBusinessModel();
//    }

//    /// <summary>
//    /// Soft deletes a backtest run
//    /// </summary>
//    /// <param name="id">The backtest run's unique identifier</param>
//    /// <returns>True if deletion was successful</returns>
//    public async Task<bool> DeleteAsync(Guid id)
//    {
//        _logger.LogInformation("Deleting backtest run with ID: {BacktestRunId}", id);

//        var entity = await _context.BacktestRuns.FindAsync(id);
//        if (entity == null)
//            return false;

//        entity.IsDeleted = true;
//        entity.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();
//        return true;
//    }

//    /// <summary>
//    /// Counts the number of backtest runs for a user
//    /// </summary>
//    /// <param name="userId">The user's unique identifier</param>
//    /// <returns>Number of backtest runs</returns>
//    public async Task<int> CountByUserIdAsync(Guid userId)
//    {
//        _logger.LogInformation("Counting backtest runs for user: {UserId}", userId);

//        return await _context.BacktestRuns
//            .CountAsync(b => b.UserId == userId && !b.IsDeleted);
//    }

//    /// <summary>
//    /// Gets backtest runs by status
//    /// </summary>
//    /// <param name="status">The backtest status to filter by</param>
//    /// <returns>Collection of backtest run business models</returns>
//    public async Task<IEnumerable<BacktestRunModel>> GetByStatusAsync(BacktestStatus status)
//    {
//        _logger.LogInformation("Getting backtest runs with status: {Status}", status);

//        var entities = await _context.BacktestRuns
//            .Where(b => b.Status == (int)status && !b.IsDeleted)
//            .OrderByDescending(b => b.CreatedAt)
//            .ToListAsync();

//        return entities.ToBusinessModels();
//    }
//}