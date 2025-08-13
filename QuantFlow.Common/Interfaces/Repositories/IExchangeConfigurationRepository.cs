namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for exchange configuration operations using business models
/// </summary>
public interface IExchangeConfigurationRepository
{
    /// <summary>
    /// Gets exchange configuration by exchange type
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Exchange configuration model if found, null otherwise</returns>
    Task<ExchangeConfigurationModel?> GetByExchangeAsync(Exchange exchange);

    /// <summary>
    /// Gets all supported exchange configurations
    /// </summary>
    /// <returns>Collection of exchange configuration models</returns>
    Task<IEnumerable<ExchangeConfigurationModel>> GetAllSupportedAsync();

    /// <summary>
    /// Gets all exchange configurations including inactive ones
    /// </summary>
    /// <returns>Collection of all exchange configuration models</returns>
    Task<IEnumerable<ExchangeConfigurationModel>> GetAllAsync();

    /// <summary>
    /// Creates or updates exchange configuration
    /// </summary>
    /// <param name="configuration">Exchange configuration to upsert</param>
    /// <returns>Created or updated exchange configuration model</returns>
    Task<ExchangeConfigurationModel> UpsertAsync(ExchangeConfigurationModel configuration);

    /// <summary>
    /// Updates base fees for an exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="makerFee">Maker fee percentage</param>
    /// <param name="takerFee">Taker fee percentage</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateBaseFeesAsync(Exchange exchange, decimal makerFee, decimal takerFee);

    /// <summary>
    /// Gets fee tiers for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Collection of fee tier models ordered by tier level</returns>
    Task<IEnumerable<FeeTierModel>> GetFeeTiersAsync(Exchange exchange);

    /// <summary>
    /// Adds or updates a fee tier for an exchange
    /// </summary>
    /// <param name="feeTier">Fee tier to add or update</param>
    /// <returns>Created or updated fee tier model</returns>
    Task<FeeTierModel> UpsertFeeTierAsync(FeeTierModel feeTier);

    /// <summary>
    /// Removes a fee tier
    /// </summary>
    /// <param name="id">Fee tier ID to remove</param>
    /// <returns>True if removal was successful</returns>
    Task<bool> RemoveFeeTierAsync(Guid id);

    /// <summary>
    /// Gets symbol fee overrides for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <returns>Collection of symbol fee override models</returns>
    Task<IEnumerable<SymbolFeeOverrideModel>> GetSymbolOverridesAsync(Exchange exchange, string? symbol = null);

    /// <summary>
    /// Adds or updates a symbol fee override
    /// </summary>
    /// <param name="symbolOverride">Symbol fee override to add or update</param>
    /// <returns>Created or updated symbol fee override model</returns>
    Task<SymbolFeeOverrideModel> UpsertSymbolOverrideAsync(SymbolFeeOverrideModel symbolOverride);

    /// <summary>
    /// Removes a symbol fee override
    /// </summary>
    /// <param name="id">Symbol override ID to remove</param>
    /// <returns>True if removal was successful</returns>
    Task<bool> RemoveSymbolOverrideAsync(Guid id);

    /// <summary>
    /// Gets the effective fee tier for a given exchange and volume
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="volume">Trading volume</param>
    /// <returns>Applicable fee tier model or null if none found</returns>
    Task<FeeTierModel?> GetEffectiveFeeTierAsync(Exchange exchange, decimal volume);
}