namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of backtest run repository
/// </summary>
public class BacktestRunRepository : IBacktestRunRepository
{
    private readonly ILogger<BacktestRunRepository> _logger;
    private readonly QuantFlowDbContext _context;

    public BacktestRunRepository(ILogger<BacktestRunRepository> logger, QuantFlowDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<BacktestRunModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting backtest run with ID: {BacktestRunId}", id);

        var entity = await _context.BacktestRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.Id == id && !_.IsDeleted);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<BacktestRunModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting backtest runs for user: {UserId}", userId);

        var entities = await _context.BacktestRuns
            .AsNoTracking()
            .Where(_ => _.UserId == userId && !_.IsDeleted)
            .OrderByDescending(_ => _.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<BacktestRunModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all backtest runs");

        var entities = await _context.BacktestRuns
            .AsNoTracking()
            .Where(_ => !_.IsDeleted)
            .OrderByDescending(_ => _.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<BacktestRunModel> CreateAsync(BacktestRunModel backtestRun)
    {
        _logger.LogInformation("Creating backtest run: {Name}", backtestRun.Name);

        var entity = backtestRun.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.BacktestRuns.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<BacktestRunModel> UpdateAsync(BacktestRunModel backtestRun)
    {
        _logger.LogInformation("Updating backtest run: {BacktestRunId}", backtestRun.Id);

        var entity = await _context.BacktestRuns.FindAsync(backtestRun.Id);
        if (entity == null)
            throw new NotFoundException($"Backtest run with ID {backtestRun.Id} not found");

        entity.Name = backtestRun.Name;
        entity.Status = backtestRun.Status.ToString();
        entity.FinalBalance = backtestRun.FinalBalance;
        entity.TotalReturnPercent = backtestRun.TotalReturnPercent;
        entity.MaxDrawdownPercent = backtestRun.MaxDrawdownPercent;
        entity.SharpeRatio = backtestRun.SharpeRatio;
        entity.TotalTrades = backtestRun.TotalTrades;
        entity.WinningTrades = backtestRun.WinningTrades;
        entity.LosingTrades = backtestRun.LosingTrades;
        entity.WinRatePercent = backtestRun.WinRatePercent;
        entity.AverageTradeReturnPercent = backtestRun.AverageTradeReturnPercent;
        entity.ExecutionDurationTicks = backtestRun.ExecutionDuration.Ticks;
        entity.ErrorMessage = backtestRun.ErrorMessage;
        entity.CompletedAt = backtestRun.CompletedAt;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting backtest run: {BacktestRunId}", id);

        var entity = await _context.BacktestRuns.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Counting backtest runs for user: {UserId}", userId);

        return await _context.BacktestRuns
            .Where(_ => _.UserId == userId && !_.IsDeleted)
            .CountAsync();
    }

    public async Task<IEnumerable<BacktestRunModel>> GetByStatusAsync(BacktestStatus status)
    {
        _logger.LogInformation("Getting backtest runs by status: {Status}", status);

        var entities = await _context.BacktestRuns
            .AsNoTracking()
            .Where(_ => _.Status == status.ToString() && !_.IsDeleted)
            .OrderByDescending(_ => _.CreatedAt)
            .ToListAsync();

        return entities.ToBusinessModels();
    }
}