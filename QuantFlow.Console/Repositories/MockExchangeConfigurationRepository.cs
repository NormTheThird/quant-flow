namespace QuantFlow.Console.Repositories;

/// <summary>
/// Mock repository for testing exchange configuration functionality
/// Remove this when the actual SQL repository is available
/// </summary>
public class MockExchangeConfigurationRepository : IExchangeConfigurationRepository
{
    private readonly ILogger<MockExchangeConfigurationRepository> _logger;
    private readonly Dictionary<Exchange, ExchangeConfigurationModel> _exchanges = new();
    private readonly List<FeeTierModel> _feeTiers = new();
    private readonly List<SymbolFeeOverrideModel> _symbolOverrides = new();

    public MockExchangeConfigurationRepository(ILogger<MockExchangeConfigurationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ExchangeConfigurationModel?> GetByExchangeAsync(Exchange exchange)
    {
        _logger.LogInformation("Mock: Getting configuration for {Exchange}", exchange);
        _exchanges.TryGetValue(exchange, out var config);
        return Task.FromResult(config);
    }

    public Task<IEnumerable<ExchangeConfigurationModel>> GetAllSupportedAsync()
    {
        _logger.LogInformation("Mock: Getting all supported exchanges");
        var supported = _exchanges.Values.Where(e => e.IsSupported).ToList();
        return Task.FromResult<IEnumerable<ExchangeConfigurationModel>>(supported);
    }

    public Task<IEnumerable<ExchangeConfigurationModel>> GetAllAsync()
    {
        _logger.LogInformation("Mock: Getting all exchanges");
        return Task.FromResult<IEnumerable<ExchangeConfigurationModel>>(_exchanges.Values);
    }

    public Task<ExchangeConfigurationModel> UpsertAsync(ExchangeConfigurationModel configuration)
    {
        _logger.LogInformation("Mock: Upserting configuration for {Exchange}", configuration.Exchange);

        if (configuration.Id == Guid.Empty)
            configuration.Id = Guid.NewGuid();

        configuration.UpdatedAt = DateTime.UtcNow;
        _exchanges[configuration.Exchange] = configuration;

        return Task.FromResult(configuration);
    }

    public Task<bool> UpdateBaseFeesAsync(Exchange exchange, decimal makerFee, decimal takerFee)
    {
        _logger.LogInformation("Mock: Updating base fees for {Exchange}: Maker {Maker}%, Taker {Taker}%",
            exchange, makerFee, takerFee);

        if (_exchanges.TryGetValue(exchange, out var config))
        {
            config.BaseMakerFeePercent = makerFee;
            config.BaseTakerFeePercent = takerFee;
            config.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<IEnumerable<FeeTierModel>> GetFeeTiersAsync(Exchange exchange)
    {
        _logger.LogInformation("Mock: Getting fee tiers for {Exchange}", exchange);
        var tiers = _feeTiers.Where(t => t.Exchange == exchange && t.IsActive)
                            .OrderBy(t => t.TierLevel)
                            .ToList();
        return Task.FromResult<IEnumerable<FeeTierModel>>(tiers);
    }

    public Task<FeeTierModel> UpsertFeeTierAsync(FeeTierModel feeTier)
    {
        _logger.LogInformation("Mock: Upserting fee tier {Tier} for {Exchange}",
            feeTier.TierLevel, feeTier.Exchange);

        // Find existing tier
        var existing = _feeTiers.FirstOrDefault(t =>
            t.Exchange == feeTier.Exchange && t.TierLevel == feeTier.TierLevel);

        if (existing != null)
        {
            // Update existing
            existing.MinimumVolumeThreshold = feeTier.MinimumVolumeThreshold;
            existing.MakerFeePercent = feeTier.MakerFeePercent;
            existing.TakerFeePercent = feeTier.TakerFeePercent;
            existing.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(existing);
        }
        else
        {
            // Add new
            if (feeTier.Id == Guid.Empty)
                feeTier.Id = Guid.NewGuid();

            _feeTiers.Add(feeTier);
            return Task.FromResult(feeTier);
        }
    }

    public Task<bool> RemoveFeeTierAsync(Guid id)
    {
        _logger.LogInformation("Mock: Removing fee tier {Id}", id);
        var tier = _feeTiers.FirstOrDefault(t => t.Id == id);
        if (tier != null)
        {
            _feeTiers.Remove(tier);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<IEnumerable<SymbolFeeOverrideModel>> GetSymbolOverridesAsync(Exchange exchange, string? symbol = null)
    {
        _logger.LogInformation("Mock: Getting symbol overrides for {Exchange} {Symbol}",
            exchange, symbol ?? "all");

        var overrides = _symbolOverrides.Where(o => o.Exchange == exchange && o.IsActive);

        if (!string.IsNullOrEmpty(symbol))
        {
            overrides = overrides.Where(o => o.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<SymbolFeeOverrideModel>>(overrides.ToList());
    }

    public Task<SymbolFeeOverrideModel> UpsertSymbolOverrideAsync(SymbolFeeOverrideModel symbolOverride)
    {
        _logger.LogInformation("Mock: Upserting symbol override for {Exchange} {Symbol}",
            symbolOverride.Exchange, symbolOverride.Symbol);

        // Find existing override
        var existing = _symbolOverrides.FirstOrDefault(o =>
            o.Exchange == symbolOverride.Exchange &&
            o.Symbol.Equals(symbolOverride.Symbol, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // Update existing
            existing.MakerFeePercent = symbolOverride.MakerFeePercent;
            existing.TakerFeePercent = symbolOverride.TakerFeePercent;
            existing.Reason = symbolOverride.Reason;
            existing.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(existing);
        }
        else
        {
            // Add new
            if (symbolOverride.Id == Guid.Empty)
                symbolOverride.Id = Guid.NewGuid();

            _symbolOverrides.Add(symbolOverride);
            return Task.FromResult(symbolOverride);
        }
    }

    public Task<bool> RemoveSymbolOverrideAsync(Guid id)
    {
        _logger.LogInformation("Mock: Removing symbol override {Id}", id);
        var symbolOverride = _symbolOverrides.FirstOrDefault(o => o.Id == id);
        if (symbolOverride != null)
        {
            _symbolOverrides.Remove(symbolOverride);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<FeeTierModel?> GetEffectiveFeeTierAsync(Exchange exchange, decimal volume)
    {
        _logger.LogInformation("Mock: Getting effective fee tier for {Exchange} volume {Volume}",
            exchange, volume);

        var applicableTiers = _feeTiers
            .Where(t => t.Exchange == exchange && t.IsActive && t.MinimumVolumeThreshold <= volume)
            .OrderByDescending(t => t.MinimumVolumeThreshold)
            .ToList();

        var effectiveTier = applicableTiers.FirstOrDefault();
        return Task.FromResult(effectiveTier);
    }
}