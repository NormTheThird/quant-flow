namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for market data configuration operations
/// </summary>
public interface IMarketDataConfigurationService
{
    /// <summary>
    /// Gets all market data configurations
    /// </summary>
    Task<IEnumerable<MarketDataConfigurationModel>> GetAllConfigurationsAsync();

    /// <summary>
    /// Gets a configuration by ID
    /// </summary>
    Task<MarketDataConfigurationModel?> GetConfigurationByIdAsync(Guid id);

    /// <summary>
    /// Creates a new market data configuration
    /// </summary>
    Task<MarketDataConfigurationModel> CreateAsync(MarketDataConfigurationModel model);

    /// <summary>
    /// Toggles a specific interval on or off
    /// </summary>
    Task<MarketDataConfigurationModel?> ToggleIntervalAsync(Guid id, string interval, bool isActive);

    /// <summary>
    /// Updates an entire configuration
    /// </summary>
    Task<MarketDataConfigurationModel?> UpdateAsync(MarketDataConfigurationModel model);

    /// <summary>
    /// Deletes a single configuration (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}