namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for symbol operations using business models
/// </summary>
public interface ISymbolRepository
{
    /// <summary>
    /// Gets a symbol by its unique identifier
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>Symbol business model if found, null otherwise</returns>
    Task<SymbolModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a symbol by its unique identifier, including soft-deleted symbols
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>Symbol business model if found (including deleted), null otherwise</returns>
    Task<SymbolModel?> GetByIdIncludingDeletedAsync(Guid id);

    /// <summary>
    /// Gets a symbol by its symbol name
    /// </summary>
    /// <param name="symbol">The symbol name</param>
    /// <returns>Symbol business model if found, null otherwise</returns>
    Task<SymbolModel?> GetBySymbolAsync(string symbol);

    /// <summary>
    /// Gets all symbols
    /// </summary>
    /// <returns>Collection of symbol business models</returns>
    Task<IEnumerable<SymbolModel>> GetAllAsync();

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    /// <returns>Collection of active symbol business models</returns>
    Task<IEnumerable<SymbolModel>> GetActiveAsync();

    /// <summary>
    /// Creates a new symbol
    /// </summary>
    /// <param name="symbol">Symbol business model to create</param>
    /// <returns>Created symbol business model</returns>
    Task<SymbolModel> CreateAsync(SymbolModel symbol);

    /// <summary>
    /// Restores a soft-deleted symbol by setting IsDeleted to false
    /// </summary>
    /// <param name="symbol">Symbol business model with updated properties</param>
    /// <returns>Restored symbol business model</returns>
    Task<SymbolModel> RestoreAsync(SymbolModel symbol);

    /// <summary>
    /// Updates an existing symbol
    /// </summary>
    /// <param name="symbol">Symbol business model with updates</param>
    /// <returns>Updated symbol business model</returns>
    Task<SymbolModel> UpdateAsync(SymbolModel symbol);

    /// <summary>
    /// Soft deletes a symbol
    /// </summary>
    /// <param name="id">The symbol's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets symbols by base asset
    /// </summary>
    /// <param name="baseAsset">The base asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    Task<IEnumerable<SymbolModel>> GetByBaseAssetAsync(string baseAsset);

    /// <summary>
    /// Gets symbols by quote asset
    /// </summary>
    /// <param name="quoteAsset">The quote asset to filter by</param>
    /// <returns>Collection of symbol business models</returns>
    Task<IEnumerable<SymbolModel>> GetByQuoteAssetAsync(string quoteAsset);

    /// <summary>
    /// Checks if a symbol exists by ID
    /// </summary>
    Task<bool> ExistsAsync(Guid symbolId);
}