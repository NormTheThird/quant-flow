namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for exchange symbol operations using business models
/// </summary>
public interface IExchangeSymbolRepository
{
    /// <summary>
    /// Gets an exchange symbol by its unique identifier
    /// </summary>
    /// <param name="id">The exchange symbol's unique identifier</param>
    /// <returns>ExchangeSymbol business model if found, null otherwise</returns>
    Task<ExchangeSymbolModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all exchange symbols for a specific symbol
    /// </summary>
    /// <param name="symbolId">The symbol's unique identifier</param>
    /// <returns>Collection of exchange symbol business models</returns>
    Task<IEnumerable<ExchangeSymbolModel>> GetBySymbolIdAsync(Guid symbolId);

    /// <summary>
    /// Gets all exchange symbols for a specific exchange
    /// </summary>
    /// <param name="exchange">The exchange to filter by</param>
    /// <returns>Collection of exchange symbol business models</returns>
    Task<IEnumerable<ExchangeSymbolModel>> GetByExchangeAsync(Exchange exchange);

    /// <summary>
    /// Gets an exchange symbol by symbol and exchange
    /// </summary>
    /// <param name="symbolId">The symbol's unique identifier</param>
    /// <param name="exchange">The exchange</param>
    /// <returns>ExchangeSymbol business model if found, null otherwise</returns>
    Task<ExchangeSymbolModel?> GetBySymbolAndExchangeAsync(Guid symbolId, Exchange exchange);

    /// <summary>
    /// Gets all exchange symbols
    /// </summary>
    /// <returns>Collection of exchange symbol business models</returns>
    Task<IEnumerable<ExchangeSymbolModel>> GetAllAsync();

    /// <summary>
    /// Gets all active exchange symbols
    /// </summary>
    /// <returns>Collection of active exchange symbol business models</returns>
    Task<IEnumerable<ExchangeSymbolModel>> GetActiveAsync();

    /// <summary>
    /// Creates a new exchange symbol
    /// </summary>
    /// <param name="exchangeSymbol">ExchangeSymbol business model to create</param>
    /// <returns>Created exchange symbol business model</returns>
    Task<ExchangeSymbolModel> CreateAsync(ExchangeSymbolModel exchangeSymbol);

    /// <summary>
    /// Updates an existing exchange symbol
    /// </summary>
    /// <param name="exchangeSymbol">ExchangeSymbol business model with updates</param>
    /// <returns>Updated exchange symbol business model</returns>
    Task<ExchangeSymbolModel> UpdateAsync(ExchangeSymbolModel exchangeSymbol);

    /// <summary>
    /// Soft deletes an exchange symbol
    /// </summary>
    /// <param name="id">The exchange symbol's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Updates the last data update timestamp for an exchange symbol
    /// </summary>
    /// <param name="id">The exchange symbol's unique identifier</param>
    /// <param name="timestamp">The new timestamp</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateLastDataUpdateAsync(Guid id, DateTime timestamp);
}