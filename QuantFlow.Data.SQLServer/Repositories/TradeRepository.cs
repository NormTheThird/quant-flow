namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of trade repository
/// </summary>
public class TradeRepository : ITradeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TradeRepository> _logger;

    public TradeRepository(ApplicationDbContext context, ILogger<TradeRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a trade by its unique identifier
    /// </summary>
    /// <param name="id">The trade's unique identifier</param>
    /// <returns>Trade business model if found, null otherwise</returns>
    public async Task<TradeModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting trade with ID: {TradeId}", id);

        var entity = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all trades for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <returns>Collection of trade business models</returns>
    public async Task<IEnumerable<TradeModel>> GetByBacktestRunIdAsync(Guid backtestRunId)
    {
        _logger.LogInformation("Getting trades for backtest run: {BacktestRunId}", backtestRunId);

        var entities = await _context.Trades
            .Where(t => t.BacktestRunId == backtestRunId && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets all active trades
    /// </summary>
    /// <returns>Collection of trade business models</returns>
    public async Task<IEnumerable<TradeModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all trades");

        var entities = await _context.Trades
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new trade
    /// </summary>
    /// <param name="trade">Trade business model to create</param>
    /// <returns>Created trade business model</returns>
    public async Task<TradeModel> CreateAsync(TradeModel trade)
    {
        _logger.LogInformation("Creating trade for symbol: {Symbol}", trade.Symbol);

        var entity = trade.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;

        _context.Trades.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Creates multiple trades
    /// </summary>
    /// <param name="trades">Collection of trade business models to create</param>
    /// <returns>Collection of created trade business models</returns>
    public async Task<IEnumerable<TradeModel>> CreateBatchAsync(IEnumerable<TradeModel> trades)
    {
        _logger.LogInformation("Creating batch of {TradeCount} trades", trades.Count());

        var entities = trades.Select(t => t.ToEntity()).ToList();
        var createdAt = DateTime.UtcNow;

        foreach (var entity in entities)
            entity.CreatedAt = createdAt;

        _context.Trades.AddRange(entities);
        await _context.SaveChangesAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Updates an existing trade
    /// </summary>
    /// <param name="trade">Trade business model with updates</param>
    /// <returns>Updated trade business model</returns>
    public async Task<TradeModel> UpdateAsync(TradeModel trade)
    {
        _logger.LogInformation("Updating trade with ID: {TradeId}", trade.Id);

        var entity = await _context.Trades.FindAsync(trade.Id);
        if (entity == null)
            throw new NotFoundException($"Trade with ID {trade.Id} not found");

        entity.Symbol = trade.Symbol;
        entity.Type = (int)trade.Type;
        entity.ExecutionTimestamp = trade.ExecutionTimestamp;
        entity.Quantity = trade.Quantity;
        entity.Price = trade.Price;
        entity.Value = trade.Value;
        entity.Commission = trade.Commission;
        entity.NetValue = trade.NetValue;
        entity.PortfolioBalanceBefore = trade.PortfolioBalanceBefore;
        entity.PortfolioBalanceAfter = trade.PortfolioBalanceAfter;
        entity.AlgorithmReason = trade.AlgorithmReason;
        entity.AlgorithmConfidence = trade.AlgorithmConfidence;
        entity.RealizedProfitLoss = trade.RealizedProfitLoss;
        entity.RealizedProfitLossPercent = trade.RealizedProfitLossPercent;
        entity.EntryTradeId = trade.EntryTradeId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes a trade
    /// </summary>
    /// <param name="id">The trade's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting trade with ID: {TradeId}", id);

        var entity = await _context.Trades.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets trades by symbol for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <param name="symbol">The trading symbol</param>
    /// <returns>Collection of trade business models</returns>
    public async Task<IEnumerable<TradeModel>> GetBySymbolAsync(Guid backtestRunId, string symbol)
    {
        _logger.LogInformation("Getting trades for symbol: {Symbol} in backtest run: {BacktestRunId}", symbol, backtestRunId);

        var entities = await _context.Trades
            .Where(t => t.BacktestRunId == backtestRunId && t.Symbol == symbol && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets trades by type for a specific backtest run
    /// </summary>
    /// <param name="backtestRunId">The backtest run's unique identifier</param>
    /// <param name="tradeType">The trade type</param>
    /// <returns>Collection of trade business models</returns>
    public async Task<IEnumerable<TradeModel>> GetByTypeAsync(Guid backtestRunId, TradeType tradeType)
    {
        _logger.LogInformation("Getting trades of type: {TradeType} for backtest run: {BacktestRunId}", tradeType, backtestRunId);

        var entities = await _context.Trades
            .Where(t => t.BacktestRunId == backtestRunId && t.Type == (int)tradeType && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp)
            .ToListAsync();

        return entities.ToBusinessModels();
    }
}