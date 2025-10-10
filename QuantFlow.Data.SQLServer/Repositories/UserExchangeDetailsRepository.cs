namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server repository for user exchange details operations
/// </summary>
public class UserExchangeDetailsRepository : IUserExchangeDetailsRepository
{
    private readonly QuantFlowDbContext _context;
    private readonly ILogger<UserExchangeDetailsRepository> _logger;

    public UserExchangeDetailsRepository(
        ILogger<UserExchangeDetailsRepository> logger,
        QuantFlowDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserExchangeDetailsModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting exchange details by ID: {Id}", id);
        var entity = await _context.UserExchangeDetails.FindAsync(id);
        return entity?.ToBusinessModel();
    }

    public async Task<IEnumerable<UserExchangeDetailsModel>> GetByUserAndExchangeAsync(Guid userId, string exchange)
    {
        _logger.LogInformation("Getting exchange details for user: {UserId}, exchange: {Exchange}", userId, exchange);
        var entities = await _context.UserExchangeDetails
            .Where(_ => _.UserId == userId && _.Exchange == exchange)
            .OrderBy(_ => _.KeyName)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting all exchange details for user: {UserId}", userId);
        var entities = await _context.UserExchangeDetails
            .Where(_ => _.UserId == userId)
            .OrderBy(_ => _.Exchange)
            .ThenBy(_ => _.KeyName)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    public async Task<UserExchangeDetailsModel> CreateAsync(UserExchangeDetailsModel model)
    {
        _logger.LogInformation("Creating exchange detail for user: {UserId}, exchange: {Exchange}, key: {KeyName}",
            model.UserId, model.Exchange, model.KeyName);

        var entity = model.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.UserExchangeDetails.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<UserExchangeDetailsModel> UpdateAsync(UserExchangeDetailsModel model)
    {
        _logger.LogInformation("Updating exchange detail: {Id}", model.Id);

        var entity = model.ToEntity();
        entity.UpdatedAt = DateTime.UtcNow;

        _context.UserExchangeDetails.Update(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting exchange detail: {Id}", id);

        var entity = await _context.UserExchangeDetails.FindAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("Exchange detail not found: {Id}", id);
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}