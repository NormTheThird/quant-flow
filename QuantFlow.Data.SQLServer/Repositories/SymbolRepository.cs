namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of symbol repository
/// </summary>
public class SymbolRepository : ISymbolRepository
{
    private readonly QuantFlowDbContext _context;
    private readonly ILogger<SymbolRepository> _logger;

    public SymbolRepository(QuantFlowDbContext context, ILogger<SymbolRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a symbol by its unique identifier
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>Symbol business model if found, null otherwise</returns>
    public async Task<SymbolModel?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Symbols
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            return entity?.ToBusinessModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting symbol by ID: {SymbolId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets a symbol by its unique identifier, including soft-deleted symbols
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>Symbol business model if found (including deleted), null otherwise</returns>
    public async Task<SymbolModel?> GetByIdIncludingDeletedAsync(Guid id)
    {
        try
        {
            var entity = await _context.Symbols
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);

            return entity?.ToBusinessModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting symbol by ID including deleted: {SymbolId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets a symbol by its symbol name
    /// </summary>
    /// <param name="symbol">The symbol name</param>
    /// <returns>Symbol business model if found, null otherwise</returns>
    public async Task<SymbolModel?> GetBySymbolAsync(string symbol)
    {
        try
        {
            var entity = await _context.Symbols.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Symbol == symbol);

            return entity?.ToBusinessModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting symbol by name: {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetAllAsync()
    {
        try
        {
            var entities = await _context.Symbols
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Symbol)
                .ToListAsync();

            return entities.ToBusinessModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting all symbols");
            throw;
        }
    }

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    /// <returns>Collection of active symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetActiveAsync()
    {
        try
        {
            var entities = await _context.Symbols
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Symbol)
                .ToListAsync();

            return entities.ToBusinessModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting active symbols");
            throw;
        }
    }

    /// <summary>
    /// Creates a new symbol
    /// </summary>
    /// <param name="symbol">Symbol business model to create</param>
    /// <returns>Created symbol business model</returns>
    public async Task<SymbolModel> CreateAsync(SymbolModel symbol)
    {
        try
        {
            var entity = symbol.ToEntity();
            entity.Id = Guid.NewGuid();

            var utcNow = DateTime.UtcNow;
            entity.CreatedAt = utcNow;
            entity.UpdatedAt = utcNow;

            _context.Symbols.Add(entity);
            await _context.SaveChangesAsync();

            return entity.ToBusinessModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error creating symbol: {Symbol}", symbol.Symbol);
            throw;
        }
    }

    /// <summary>
    /// Restores a soft-deleted symbol by setting IsDeleted to false
    /// </summary>
    /// <param name="symbol">Symbol business model with updated properties</param>
    /// <returns>Restored symbol business model</returns>
    public async Task<SymbolModel> RestoreAsync(SymbolModel symbol)
    {
        try
        {
            var entity = await _context.Symbols
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == symbol.Id);

            if (entity == null)
                throw new NotFoundException($"Symbol with ID {symbol.Id} not found");

            if (!entity.IsDeleted)
                throw new InvalidOperationException("Symbol is not deleted and cannot be restored");

            // Restore and update properties
            entity.IsDeleted = false;
            entity.Symbol = symbol.Symbol;
            entity.BaseAsset = symbol.BaseAsset;
            entity.QuoteAsset = symbol.QuoteAsset;
            entity.IsActive = symbol.IsActive;
            entity.MinTradeAmount = symbol.MinTradeAmount;
            entity.PricePrecision = symbol.PricePrecision;
            entity.QuantityPrecision = symbol.QuantityPrecision;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return entity.ToBusinessModel();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error restoring symbol: {SymbolId}", symbol.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing symbol
    /// </summary>
    /// <param name="symbol">Symbol business model with updates</param>
    /// <returns>Updated symbol business model</returns>
    public async Task<SymbolModel> UpdateAsync(SymbolModel symbol)
    {
        try
        {
            var entity = await _context.Symbols.FindAsync(symbol.Id);
            if (entity == null)
                throw new NotFoundException($"Symbol with ID {symbol.Id} not found");

            entity.Symbol = symbol.Symbol;
            entity.BaseAsset = symbol.BaseAsset;
            entity.QuoteAsset = symbol.QuoteAsset;
            entity.IsActive = symbol.IsActive;
            entity.MinTradeAmount = symbol.MinTradeAmount;
            entity.PricePrecision = symbol.PricePrecision;
            entity.QuantityPrecision = symbol.QuantityPrecision;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return entity.ToBusinessModel();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error updating symbol: {SymbolId}", symbol.Id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes a symbol
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Symbols.FindAsync(id);
            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error deleting symbol: {SymbolId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets symbols by base asset
    /// </summary>
    /// <param name="baseAsset">The base asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetByBaseAssetAsync(string baseAsset)
    {
        try
        {
            var entities = await _context.Symbols
                .Where(s => s.BaseAsset == baseAsset && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Symbol)
                .ToListAsync();

            return entities.ToBusinessModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting symbols by base asset: {BaseAsset}", baseAsset);
            throw;
        }
    }

    /// <summary>
    /// Gets symbols by quote asset
    /// </summary>
    /// <param name="quoteAsset">The quote asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetByQuoteAssetAsync(string quoteAsset)
    {
        try
        {
            var entities = await _context.Symbols
                .Where(s => s.QuoteAsset == quoteAsset && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Symbol)
                .ToListAsync();

            return entities.ToBusinessModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error getting symbols by quote asset: {QuoteAsset}", quoteAsset);
            throw;
        }
    }

    /// <summary>
    /// Checks if a symbol exists by ID
    /// </summary>
    public async Task<bool> ExistsAsync(Guid symbolId)
    {
        try
        {
            return await _context.Symbols
                .AnyAsync(s => s.Id == symbolId && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error checking if symbol exists: {SymbolId}", symbolId);
            throw;
        }
    }
}