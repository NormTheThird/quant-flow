namespace QuantFlow.Domain.Interfaces.Services;

/// <summary>
/// Service interface for exchange configuration and fee calculation operations
/// </summary>
public interface IExchangeConfigurationService
{
    /// <summary>
    /// Gets all supported exchanges with their configurations
    /// </summary>
    /// <returns>Collection of supported exchange configurations</returns>
    Task<IEnumerable<ExchangeConfigurationModel>> GetSupportedExchangesAsync();

    /// <summary>
    /// Gets configuration for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Exchange configuration or null if not found</returns>
    Task<ExchangeConfigurationModel?> GetExchangeConfigurationAsync(Exchange exchange);

    /// <summary>
    /// Calculates trading fees for a specific trade
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="tradeValue">Value of the trade</param>
    /// <param name="volume">User's 30-day trading volume (optional for tier calculation)</param>
    /// <returns>Fee calculation result with maker and taker fees</returns>
    Task<FeeCalculationResult> CalculateFeesAsync(Exchange exchange, string symbol, decimal tradeValue, decimal? volume = null);

    /// <summary>
    /// Sets base fee structure for an exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="makerFeePercent">Maker fee percentage</param>
    /// <param name="takerFeePercent">Taker fee percentage</param>
    /// <returns>True if update was successful</returns>
    Task<bool> SetBaseFeesAsync(Exchange exchange, decimal makerFeePercent, decimal takerFeePercent);

    /// <summary>
    /// Adds or updates a volume-based fee tier
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="tierLevel">Tier level</param>
    /// <param name="minimumVolume">Minimum volume threshold</param>
    /// <param name="makerFeePercent">Maker fee percentage for this tier</param>
    /// <param name="takerFeePercent">Taker fee percentage for this tier</param>
    /// <returns>Created or updated fee tier</returns>
    Task<FeeTierModel> AddOrUpdateFeeTierAsync(Exchange exchange, int tierLevel, decimal minimumVolume, decimal makerFeePercent, decimal takerFeePercent);

    /// <summary>
    /// Sets symbol-specific fee override
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="makerFeePercent">Override maker fee percentage</param>
    /// <param name="takerFeePercent">Override taker fee percentage</param>
    /// <param name="reason">Reason for override</param>
    /// <returns>Created or updated symbol override</returns>
    Task<SymbolFeeOverrideModel> SetSymbolFeeOverrideAsync(Exchange exchange, string symbol, decimal makerFeePercent, decimal takerFeePercent, string reason = "");

    /// <summary>
    /// Gets fee tiers for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Collection of fee tiers ordered by tier level</returns>
    Task<IEnumerable<FeeTierModel>> GetFeeTiersAsync(Exchange exchange);

    /// <summary>
    /// Gets symbol fee overrides for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <returns>Collection of symbol fee overrides</returns>
    Task<IEnumerable<SymbolFeeOverrideModel>> GetSymbolOverridesAsync(Exchange exchange, string? symbol = null);

    /// <summary>
    /// Initializes default configurations for supported exchanges
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeDefaultConfigurationsAsync();

    /// <summary>
    /// Validates if an exchange is supported
    /// </summary>
    /// <param name="exchange">Exchange type to validate</param>
    /// <returns>True if exchange is supported</returns>
    Task<bool> IsExchangeSupportedAsync(Exchange exchange);
}