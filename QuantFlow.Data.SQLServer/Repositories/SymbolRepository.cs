namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of symbol repository
/// </summary>
public class SymbolRepository : ISymbolRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SymbolRepository> _logger;

    public SymbolRepository(ApplicationDbContext context, ILogger<SymbolRepository> logger)
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
        _logger.LogInformation("Getting symbol with ID: {SymbolId}", id);

        var entity = await _context.Symbols
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets a symbol by its symbol name
    /// </summary>
    /// <param name="symbol">The symbol name</param>
    /// <returns>Symbol business model if found, null otherwise</returns>
    public async Task<SymbolModel?> GetBySymbolAsync(string symbol)
    {
        _logger.LogInformation("Getting symbol: {Symbol}", symbol);

        var entity = await _context.Symbols
            .FirstOrDefaultAsync(s => s.Symbol == symbol && !s.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all symbols");

        var entities = await _context.Symbols
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    /// <returns>Collection of active symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetActiveAsync()
    {
        _logger.LogInformation("Getting active symbols");

        var entities = await _context.Symbols
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new symbol
    /// </summary>
    /// <param name="symbol">Symbol business model to create</param>
    /// <returns>Created symbol business model</returns>
    public async Task<SymbolModel> CreateAsync(SymbolModel symbol)
    {
        _logger.LogInformation("Creating symbol: {Symbol}", symbol.Symbol);

        var entity = symbol.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;

        _context.Symbols.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing symbol
    /// </summary>
    /// <param name="symbol">Symbol business model with updates</param>
    /// <returns>Updated symbol business model</returns>
    public async Task<SymbolModel> UpdateAsync(SymbolModel symbol)
    {
        _logger.LogInformation("Updating symbol with ID: {SymbolId}", symbol.Id);

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

    /// <summary>
    /// Soft deletes a symbol
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting symbol with ID: {SymbolId}", id);

        var entity = await _context.Symbols.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets symbols by base asset
    /// </summary>
    /// <param name="baseAsset">The base asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetByBaseAssetAsync(string baseAsset)
    {
        _logger.LogInformation("Getting symbols with base asset: {BaseAsset}", baseAsset);

        var entities = await _context.Symbols
            .Where(s => s.BaseAsset == baseAsset && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Gets symbols by quote asset
    /// </summary>
    /// <param name="quoteAsset">The quote asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    public async Task<IEnumerable<SymbolModel>> GetByQuoteAssetAsync(string quoteAsset)
    {
        _logger.LogInformation("Getting symbols with quote asset: {QuoteAsset}", quoteAsset);

        var entities = await _context.Symbols
            .Where(s => s.QuoteAsset == quoteAsset && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        return entities.ToBusinessModels();
    }
}