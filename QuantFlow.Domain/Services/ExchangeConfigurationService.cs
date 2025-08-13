
namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing exchange configurations and fee calculations
/// </summary>
public class ExchangeConfigurationService : IExchangeConfigurationService
{
    private readonly IExchangeConfigurationRepository _repository;
    private readonly ILogger<ExchangeConfigurationService> _logger;

    /// <summary>
    /// Supported exchanges for initial implementation
    /// </summary>
    private static readonly Exchange[] SupportedExchanges = [Exchange.Kraken, Exchange.KuCoin];

    public ExchangeConfigurationService(IExchangeConfigurationRepository repository, ILogger<ExchangeConfigurationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all supported exchanges with their configurations
    /// </summary>
    /// <returns>Collection of supported exchange configurations</returns>
    public async Task<IEnumerable<ExchangeConfigurationModel>> GetSupportedExchangesAsync()
    {
        _logger.LogDebug("Retrieving all supported exchange configurations");

        try
        {
            var configurations = await _repository.GetAllSupportedAsync();
            var supportedConfigs = configurations.Where(c => SupportedExchanges.Contains(c.Exchange)).ToList();

            _logger.LogInformation("Retrieved {Count} supported exchange configurations", supportedConfigs.Count);
            return supportedConfigs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supported exchanges");
            throw;
        }
    }

    /// <summary>
    /// Gets configuration for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Exchange configuration or null if not found</returns>
    public async Task<ExchangeConfigurationModel?> GetExchangeConfigurationAsync(Exchange exchange)
    {
        if (!await IsExchangeSupportedAsync(exchange))
        {
            _logger.LogWarning("Exchange {Exchange} is not supported", exchange);
            return null;
        }

        _logger.LogDebug("Retrieving configuration for exchange {Exchange}", exchange);

        try
        {
            return await _repository.GetByExchangeAsync(exchange);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve configuration for exchange {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Calculates trading fees for a specific trade
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="tradeValue">Value of the trade</param>
    /// <param name="volume">User's 30-day trading volume (optional for tier calculation)</param>
    /// <returns>Fee calculation result with maker and taker fees</returns>
    public async Task<FeeCalculationResult> CalculateFeesAsync(Exchange exchange, string symbol, decimal tradeValue, decimal? volume = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        if (tradeValue <= 0)
            throw new ArgumentException("Trade value must be positive", nameof(tradeValue));

        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        _logger.LogDebug("Calculating fees for {Exchange} {Symbol} trade value {TradeValue} with volume {Volume}",
            exchange, symbol, tradeValue, volume);

        try
        {
            var result = new FeeCalculationResult
            {
                Exchange = exchange,
                Symbol = symbol,
                TradeValue = tradeValue,
                VolumeUsed = volume,
                CalculatedAt = DateTime.UtcNow
            };

            // 1. Check for symbol-specific overrides first
            var symbolOverrides = await _repository.GetSymbolOverridesAsync(exchange, symbol);
            var activeOverride = symbolOverrides.FirstOrDefault(o => o.IsActive);

            if (activeOverride != null)
            {
                result.MakerFeePercent = activeOverride.MakerFeePercent;
                result.TakerFeePercent = activeOverride.TakerFeePercent;
                result.FeeSource = "Override";

                _logger.LogDebug("Using symbol override for {Exchange} {Symbol}: Maker {Maker}%, Taker {Taker}%",
                    exchange, symbol, result.MakerFeePercent, result.TakerFeePercent);
            }
            // 2. Check for volume-based tier if volume is provided
            else if (volume.HasValue && volume.Value > 0)
            {
                var effectiveTier = await _repository.GetEffectiveFeeTierAsync(exchange, volume.Value);

                if (effectiveTier != null)
                {
                    result.MakerFeePercent = effectiveTier.MakerFeePercent;
                    result.TakerFeePercent = effectiveTier.TakerFeePercent;
                    result.FeeSource = "Tier";
                    result.AppliedTierLevel = effectiveTier.TierLevel;

                    _logger.LogDebug("Using tier {Tier} for {Exchange} volume {Volume}: Maker {Maker}%, Taker {Taker}%",
                        effectiveTier.TierLevel, exchange, volume, result.MakerFeePercent, result.TakerFeePercent);
                }
                else
                {
                    // Fall back to base fees
                    await ApplyBaseFees(exchange, result);
                }
            }
            // 3. Fall back to base fees
            else
            {
                await ApplyBaseFees(exchange, result);
            }

            // Calculate fee amounts
            result.MakerFeeAmount = tradeValue * (result.MakerFeePercent / 100m);
            result.TakerFeeAmount = tradeValue * (result.TakerFeePercent / 100m);

            _logger.LogInformation("Fee calculation completed for {Exchange} {Symbol}: Maker ${MakerAmount:F2} ({MakerPercent}%), Taker ${TakerAmount:F2} ({TakerPercent}%)",
                exchange, symbol, result.MakerFeeAmount, result.MakerFeePercent, result.TakerFeeAmount, result.TakerFeePercent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate fees for {Exchange} {Symbol}", exchange, symbol);
            throw;
        }
    }

    /// <summary>
    /// Sets base fee structure for an exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="makerFeePercent">Maker fee percentage</param>
    /// <param name="takerFeePercent">Taker fee percentage</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> SetBaseFeesAsync(Exchange exchange, decimal makerFeePercent, decimal takerFeePercent)
    {
        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        ValidateFeePercentages(makerFeePercent, takerFeePercent);

        _logger.LogInformation("Setting base fees for {Exchange}: Maker {Maker}%, Taker {Taker}%",
            exchange, makerFeePercent, takerFeePercent);

        try
        {
            return await _repository.UpdateBaseFeesAsync(exchange, makerFeePercent, takerFeePercent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set base fees for {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Adds or updates a volume-based fee tier
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="tierLevel">Tier level</param>
    /// <param name="minimumVolume">Minimum volume threshold</param>
    /// <param name="makerFeePercent">Maker fee percentage for this tier</param>
    /// <param name="takerFeePercent">Taker fee percentage for this tier</param>
    /// <returns>Created or updated fee tier</returns>
    public async Task<FeeTierModel> AddOrUpdateFeeTierAsync(Exchange exchange, int tierLevel, decimal minimumVolume, decimal makerFeePercent, decimal takerFeePercent)
    {
        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        if (tierLevel <= 0)
            throw new ArgumentException("Tier level must be positive", nameof(tierLevel));

        if (minimumVolume < 0)
            throw new ArgumentException("Minimum volume cannot be negative", nameof(minimumVolume));

        ValidateFeePercentages(makerFeePercent, takerFeePercent, allowNegative: true);

        _logger.LogInformation("Adding/updating fee tier {Tier} for {Exchange}: Volume {Volume}, Maker {Maker}%, Taker {Taker}%",
            tierLevel, exchange, minimumVolume, makerFeePercent, takerFeePercent);

        try
        {
            var config = await _repository.GetByExchangeAsync(exchange);
            if (config == null)
                throw new InvalidOperationException($"Exchange configuration not found for {exchange}");

            var feeTier = new FeeTierModel
            {
                Id = Guid.NewGuid(),
                ExchangeConfigurationId = config.Id,
                Exchange = exchange,
                TierLevel = tierLevel,
                MinimumVolumeThreshold = minimumVolume,
                MakerFeePercent = makerFeePercent,
                TakerFeePercent = takerFeePercent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            return await _repository.UpsertFeeTierAsync(feeTier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add/update fee tier for {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Sets symbol-specific fee override
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="makerFeePercent">Override maker fee percentage</param>
    /// <param name="takerFeePercent">Override taker fee percentage</param>
    /// <param name="reason">Reason for override</param>
    /// <returns>Created or updated symbol override</returns>
    public async Task<SymbolFeeOverrideModel> SetSymbolFeeOverrideAsync(Exchange exchange, string symbol, decimal makerFeePercent, decimal takerFeePercent, string reason = "")
    {
        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ValidateFeePercentages(makerFeePercent, takerFeePercent);

        _logger.LogInformation("Setting symbol override for {Exchange} {Symbol}: Maker {Maker}%, Taker {Taker}% - {Reason}",
            exchange, symbol, makerFeePercent, takerFeePercent, reason);

        try
        {
            var config = await _repository.GetByExchangeAsync(exchange);
            if (config == null)
                throw new InvalidOperationException($"Exchange configuration not found for {exchange}");

            var symbolOverride = new SymbolFeeOverrideModel
            {
                Id = Guid.NewGuid(),
                ExchangeConfigurationId = config.Id,
                Exchange = exchange,
                Symbol = symbol.ToUpperInvariant(),
                MakerFeePercent = makerFeePercent,
                TakerFeePercent = takerFeePercent,
                IsActive = true,
                Reason = reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            return await _repository.UpsertSymbolOverrideAsync(symbolOverride);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set symbol override for {Exchange} {Symbol}", exchange, symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets fee tiers for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Collection of fee tiers ordered by tier level</returns>
    public async Task<IEnumerable<FeeTierModel>> GetFeeTiersAsync(Exchange exchange)
    {
        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        _logger.LogDebug("Retrieving fee tiers for {Exchange}", exchange);

        try
        {
            return await _repository.GetFeeTiersAsync(exchange);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve fee tiers for {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Gets symbol fee overrides for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <returns>Collection of symbol fee overrides</returns>
    public async Task<IEnumerable<SymbolFeeOverrideModel>> GetSymbolOverridesAsync(Exchange exchange, string? symbol = null)
    {
        if (!await IsExchangeSupportedAsync(exchange))
            throw new ArgumentException($"Exchange {exchange} is not supported", nameof(exchange));

        _logger.LogDebug("Retrieving symbol overrides for {Exchange} {Symbol}", exchange, symbol ?? "all");

        try
        {
            return await _repository.GetSymbolOverridesAsync(exchange, symbol?.ToUpperInvariant());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve symbol overrides for {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Initializes default configurations for supported exchanges
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    public async Task<bool> InitializeDefaultConfigurationsAsync()
    {
        _logger.LogInformation("Initializing default configurations for supported exchanges");

        try
        {
            var initialized = 0;

            foreach (var exchange in SupportedExchanges)
            {
                var existing = await _repository.GetByExchangeAsync(exchange);
                if (existing == null)
                {
                    var config = CreateDefaultConfiguration(exchange);
                    await _repository.UpsertAsync(config);
                    initialized++;

                    _logger.LogInformation("Initialized default configuration for {Exchange}", exchange);
                }
            }

            _logger.LogInformation("Initialized {Count} default exchange configurations", initialized);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize default configurations");
            return false;
        }
    }

    /// <summary>
    /// Validates if an exchange is supported
    /// </summary>
    /// <param name="exchange">Exchange type to validate</param>
    /// <returns>True if exchange is supported</returns>
    public async Task<bool> IsExchangeSupportedAsync(Exchange exchange)
    {
        await Task.CompletedTask;
        return SupportedExchanges.Contains(exchange);
    }



    #region Private Helper Methods

    /// <summary>
    /// Applies base fees from exchange configuration
    /// </summary>
    private async Task ApplyBaseFees(Exchange exchange, FeeCalculationResult result)
    {
        var config = await _repository.GetByExchangeAsync(exchange);
        if (config != null)
        {
            result.MakerFeePercent = config.BaseMakerFeePercent;
            result.TakerFeePercent = config.BaseTakerFeePercent;
            result.FeeSource = "Base";

            _logger.LogDebug("Using base fees for {Exchange}: Maker {Maker}%, Taker {Taker}%",
                exchange, result.MakerFeePercent, result.TakerFeePercent);
        }
        else
        {
            throw new InvalidOperationException($"Exchange configuration not found for {exchange}");
        }
    }

    /// <summary>
    /// Creates default configuration for an exchange
    /// </summary>
    private static ExchangeConfigurationModel CreateDefaultConfiguration(Exchange exchange)
    {
        return exchange switch
        {
            Exchange.Kraken => new ExchangeConfigurationModel
            {
                Id = Guid.NewGuid(),
                Exchange = Exchange.Kraken,
                Name = "Kraken",
                IsActive = true,
                IsSupported = true,
                BaseMakerFeePercent = 0.25m,
                BaseTakerFeePercent = 0.40m,
                ApiEndpoint = "https://api.kraken.com",
                MaxRequestsPerMinute = 15,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            Exchange.KuCoin => new ExchangeConfigurationModel
            {
                Id = Guid.NewGuid(),
                Exchange = Exchange.KuCoin,
                Name = "KuCoin",
                IsActive = true,
                IsSupported = true,
                BaseMakerFeePercent = 0.10m,
                BaseTakerFeePercent = 0.10m,
                ApiEndpoint = "https://api.kucoin.com",
                MaxRequestsPerMinute = 45,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            _ => throw new ArgumentException($"Default configuration not defined for {exchange}")
        };
    }

    /// <summary>
    /// Validates fee percentages are within acceptable ranges
    /// </summary>
    private static void ValidateFeePercentages(decimal makerFee, decimal takerFee, bool allowNegative = false)
    {
        var minValue = allowNegative ? -1.0m : 0.0m;
        const decimal maxValue = 10.0m; // 10% maximum fee

        if (makerFee < minValue || makerFee > maxValue)
            throw new ArgumentException($"Maker fee must be between {minValue}% and {maxValue}%", nameof(makerFee));

        if (takerFee < minValue || takerFee > maxValue)
            throw new ArgumentException($"Taker fee must be between {minValue}% and {maxValue}%", nameof(takerFee));
    }

    #endregion
}