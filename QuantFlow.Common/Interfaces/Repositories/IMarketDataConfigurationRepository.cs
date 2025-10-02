namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for market data configuration operations
/// </summary>
public interface IMarketDataConfigurationRepository
{
    /// <summary>
    /// Gets all market data configurations
    /// </summary>
    Task<IEnumerable<MarketDataConfigurationModel>> GetAllAsync();

    /// <summary>
    /// Gets a configuration by ID
    /// </summary>
    Task<MarketDataConfigurationModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new configuration
    /// </summary>
    Task<MarketDataConfigurationModel> CreateAsync(MarketDataConfigurationModel configuration);

    /// <summary>
    /// Updates an existing configuration
    /// </summary>
    Task<MarketDataConfigurationModel?> UpdateAsync(MarketDataConfigurationModel configuration);

    /// <summary>
    /// Deletes configurations by IDs (soft delete)
    /// </summary>
    Task<int> DeleteAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// Checks if a configuration exists for a symbol and exchange
    /// </summary>
    Task<bool> ExistsAsync(Guid symbolId, string exchange);
}