namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for symbol business operations
/// </summary>
public interface ISymbolService
{
    /// <summary>
    /// Gets a symbol by its unique identifier
    /// </summary>
    Task<SymbolModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a symbol by its symbol name
    /// </summary>
    Task<SymbolModel?> GetBySymbolAsync(string symbol);

    /// <summary>
    /// Gets all symbols
    /// </summary>
    Task<IEnumerable<SymbolModel>> GetAllAsync();

    /// <summary>
    /// Gets all active symbols
    /// </summary>
    Task<IEnumerable<SymbolModel>> GetActiveAsync();

    /// <summary>
    /// Creates a new symbol
    /// </summary>
    Task<SymbolModel> CreateAsync(SymbolModel symbol);

    /// <summary>
    /// Restores a deleted symbol with updated properties
    /// </summary>
    Task<SymbolModel> RestoreSymbolAsync(SymbolModel symbol);

    /// <summary>
    /// Updates an existing symbol
    /// </summary>
    Task<SymbolModel> UpdateAsync(SymbolModel symbol);

    /// <summary>
    /// Soft deletes a symbol
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets symbols by base asset
    /// </summary>
    Task<IEnumerable<SymbolModel>> GetByBaseAssetAsync(string baseAsset);

    /// <summary>
    /// Gets symbols by quote asset
    /// </summary>
    Task<IEnumerable<SymbolModel>> GetByQuoteAssetAsync(string quoteAsset);
}