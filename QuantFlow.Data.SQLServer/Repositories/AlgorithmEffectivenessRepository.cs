namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of algorithm effectiveness repository
/// </summary>
public class AlgorithmEffectivenessRepository : IAlgorithmEffectivenessRepository
{
    private readonly ILogger<AlgorithmEffectivenessRepository> _logger;
    private readonly QuantFlowDbContext _context;

    public AlgorithmEffectivenessRepository(
        ILogger<AlgorithmEffectivenessRepository> logger,
        QuantFlowDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AlgorithmEffectivenessModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting effectiveness rating with ID: {Id}", id);

        var entity = await _context.AlgorithmEffectiveness
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.Id == id);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<AlgorithmEffectivenessModel>> GetByAlgorithmIdAsync(Guid algorithmId)
    {
        _logger.LogInformation("Getting effectiveness ratings for algorithm: {AlgorithmId}", algorithmId);

        var entities = await _context.AlgorithmEffectiveness
            .AsNoTracking()
            .Where(_ => _.AlgorithmId == algorithmId)
            .OrderBy(_ => _.Timeframe)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<AlgorithmEffectivenessModel?> GetByAlgorithmAndTimeframeAsync(Guid algorithmId, string timeframe)
    {
        _logger.LogInformation("Getting effectiveness rating for algorithm: {AlgorithmId}, timeframe: {Timeframe}", algorithmId, timeframe);

        var entity = await _context.AlgorithmEffectiveness
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.AlgorithmId == algorithmId && _.Timeframe == timeframe);

        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<AlgorithmEffectivenessModel>> GetByTimeframeAsync(string timeframe)
    {
        _logger.LogInformation("Getting effectiveness ratings for timeframe: {Timeframe}", timeframe);

        var entities = await _context.AlgorithmEffectiveness
            .AsNoTracking()
            .Where(_ => _.Timeframe == timeframe)
            .OrderByDescending(_ => _.RecommendedStars)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<AlgorithmEffectivenessModel> CreateAsync(AlgorithmEffectivenessModel effectiveness)
    {
        _logger.LogInformation("Creating effectiveness rating for algorithm: {AlgorithmId}, timeframe: {Timeframe}", effectiveness.AlgorithmId, effectiveness.Timeframe);

        var entity = effectiveness.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.AlgorithmEffectiveness.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<AlgorithmEffectivenessModel> UpdateAsync(AlgorithmEffectivenessModel effectiveness)
    {
        _logger.LogInformation("Updating effectiveness rating: {Id}", effectiveness.Id);

        var entity = await _context.AlgorithmEffectiveness.FindAsync(effectiveness.Id);
        if (entity == null)
            throw new NotFoundException($"Effectiveness rating with ID {effectiveness.Id} not found");

        entity.ReliabilityStars = effectiveness.ReliabilityStars;
        entity.OpportunityStars = effectiveness.OpportunityStars;
        entity.RecommendedStars = effectiveness.RecommendedStars;
        entity.ReliabilityReason = effectiveness.ReliabilityReason;
        entity.OpportunityReason = effectiveness.OpportunityReason;
        entity.AverageWinRate = effectiveness.AverageWinRate;
        entity.AverageReturnPerTrade = effectiveness.AverageReturnPerTrade;
        entity.AverageTradesPerMonth = effectiveness.AverageTradesPerMonth;
        entity.AverageSharpeRatio = effectiveness.AverageSharpeRatio;
        entity.AverageMaxDrawdown = effectiveness.AverageMaxDrawdown;
        entity.AverageStopLossPercent = effectiveness.AverageStopLossPercent;
        entity.BestFor = effectiveness.BestFor;
        entity.AvoidWhen = effectiveness.AvoidWhen;
        entity.TotalBacktestsRun = effectiveness.TotalBacktestsRun;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = effectiveness.UpdatedBy;

        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting effectiveness rating: {Id}", id);

        var entity = await _context.AlgorithmEffectiveness.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}