namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing backtest operations
/// </summary>
public class BacktestService : IBacktestService
{
    private readonly ILogger<BacktestService> _logger;
    private readonly IBacktestRunRepository _backtestRunRepository;
    private readonly IBacktestExecutionService _backtestExecutionService;
    private readonly IUserService _userService;

    public BacktestService(ILogger<BacktestService> logger, IBacktestRunRepository backtestRunRepository, IBacktestExecutionService backtestExecutionService,
                           IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _backtestRunRepository = backtestRunRepository ?? throw new ArgumentNullException(nameof(backtestRunRepository));
        _backtestExecutionService = backtestExecutionService ?? throw new ArgumentNullException(nameof(backtestExecutionService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<BacktestRunModel?> GetBacktestRunByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting backtest run with ID: {BacktestRunId}", id);
        return await _backtestRunRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<BacktestRunModel>> GetBacktestRunsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting backtest runs for user: {UserId}", userId);
        return await _backtestRunRepository.GetByUserIdAsync(userId);
    }

    public async Task<BacktestRunModel> CreateBacktestRunAsync(BacktestRunModel backtestRun)
    {
        _logger.LogInformation("Creating backtest run: {Name} for user: {UserId}", backtestRun.Name, backtestRun.UserId);

        var user = await _userService.GetUserByIdAsync(backtestRun.UserId);
        backtestRun.CreatedBy = user?.Username ?? "System";
        backtestRun.UpdatedBy = user?.Username ?? "System";
        backtestRun.CreatedAt = DateTime.UtcNow;
        backtestRun.UpdatedAt = DateTime.UtcNow;
        backtestRun.Status = BacktestStatus.Pending;

        var createdBacktest = await _backtestRunRepository.CreateAsync(backtestRun);

        // TODO: Queue backtest execution (for now, execute immediately in background)
        _ = Task.Run(async () =>
        {
            try
            {
                await _backtestExecutionService.ExecuteBacktestAsync(createdBacktest.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background backtest execution failed: {BacktestRunId}", createdBacktest.Id);
            }
        });

        return createdBacktest;
    }

    public async Task<BacktestRunModel> UpdateBacktestRunAsync(BacktestRunModel backtestRun)
    {
        _logger.LogInformation("Updating backtest run: {BacktestRunId}", backtestRun.Id);

        var user = await _userService.GetUserByIdAsync(backtestRun.UserId);
        backtestRun.UpdatedBy = user?.Username ?? "System";
        backtestRun.UpdatedAt = DateTime.UtcNow;

        return await _backtestRunRepository.UpdateAsync(backtestRun);
    }

    public async Task<bool> DeleteBacktestRunAsync(Guid id)
    {
        _logger.LogInformation("Deleting backtest run: {BacktestRunId}", id);
        return await _backtestRunRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<BacktestRunModel>> GetBacktestRunsByStatusAsync(BacktestStatus status)
    {
        _logger.LogInformation("Getting backtest runs by status: {Status}", status);
        return await _backtestRunRepository.GetByStatusAsync(status);
    }
}