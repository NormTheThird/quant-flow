namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of market data configuration repository
/// </summary>
public class MarketDataConfigurationRepository : IMarketDataConfigurationRepository
{
    private readonly QuantFlowDbContext _context;
    private readonly ILogger<MarketDataConfigurationRepository> _logger;

    public MarketDataConfigurationRepository(QuantFlowDbContext context, ILogger<MarketDataConfigurationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all market data configurations with symbol information
    /// </summary>
    public async Task<IEnumerable<MarketDataConfigurationModel>> GetAllAsync()
    {
        try
        {
            var entities = await _context.MarketDataConfigurations
                .Include(x => x.Symbol)
                .OrderBy(x => x.Symbol.Symbol)
                .ThenBy(x => x.Exchange)
                .ToListAsync();

            return entities.ToBusinessModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting all market data configurations");
            throw;
        }
    }

    /// <summary>
    /// Gets a configuration by ID with symbol information
    /// </summary>
    public async Task<MarketDataConfigurationModel?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.MarketDataConfigurations
                .Include(x => x.Symbol)
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity?.ToBusinessModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting market data configuration by ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new configuration
    /// </summary>
    public async Task<MarketDataConfigurationModel> CreateAsync(MarketDataConfigurationModel configuration)
    {
        try
        {
            var entity = configuration.ToEntity();
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = "System";
            entity.UpdatedBy = "System";

            await _context.MarketDataConfigurations.AddAsync(entity);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(entity.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error creating market data configuration for symbol {SymbolId}", configuration.SymbolId);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing configuration
    /// </summary>
    public async Task<MarketDataConfigurationModel?> UpdateAsync(MarketDataConfigurationModel configuration)
    {
        try
        {
            var entity = await _context.MarketDataConfigurations.FirstOrDefaultAsync(x => x.Id == configuration.Id)
                ?? throw new NotFoundException($"Configuration with ID {configuration.Id} not found");

            entity.Is1mActive = configuration.Is1mActive;
            entity.Is5mActive = configuration.Is5mActive;
            entity.Is15mActive = configuration.Is15mActive;
            entity.Is1hActive = configuration.Is1hActive;
            entity.Is4hActive = configuration.Is4hActive;
            entity.Is1dActive = configuration.Is1dActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            var updatedEntity = await GetByIdAsync(entity.Id)
                ?? throw new NotFoundException($"Configuration with ID {entity.Id} not found after update");

            return updatedEntity;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error updating market data configuration: {Id}", configuration.Id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes configurations by IDs
    /// </summary>
    public async Task<int> DeleteAsync(IEnumerable<Guid> ids)
    {
        try
        {
            var entities = await _context.MarketDataConfigurations
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error soft deleting market data configurations");
            throw;
        }
    }

    /// <summary>
    /// Checks if a configuration exists for a symbol and exchange
    /// </summary>
    public async Task<bool> ExistsAsync(Guid symbolId, string exchange)
    {
        try
        {
            return await _context.MarketDataConfigurations
                .AnyAsync(x => x.SymbolId == symbolId && x.Exchange == exchange && !x.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error checking if configuration exists for symbol {SymbolId} and exchange {Exchange}", symbolId, exchange);
            throw;
        }
    }
}