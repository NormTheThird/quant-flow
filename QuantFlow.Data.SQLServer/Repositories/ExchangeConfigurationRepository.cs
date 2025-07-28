namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of exchange configuration repository
/// </summary>
public class ExchangeConfigurationRepository : IExchangeConfigurationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExchangeConfigurationRepository> _logger;

    public ExchangeConfigurationRepository(
        ApplicationDbContext context,
        ILogger<ExchangeConfigurationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets exchange configuration by exchange type
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Exchange configuration model if found, null otherwise</returns>
    public async Task<ExchangeConfigurationModel?> GetByExchangeAsync(Exchange exchange)
    {
        _logger.LogDebug("Getting configuration for exchange {Exchange}", exchange);

        try
        {
            var entity = await _context.ExchangeConfigurations
                .Include(e => e.FeeTiers.Where(ft => ft.IsActive))
                .Include(e => e.SymbolOverrides.Where(so => so.IsActive))
                .FirstOrDefaultAsync(e => e.Exchange == (int)exchange);

            var result = entity?.ToBusinessModel();

            _logger.LogDebug("Found configuration for {Exchange}: {Found}",
                exchange, result != null ? "Yes" : "No");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration for exchange {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Gets all supported exchange configurations
    /// </summary>
    /// <returns>Collection of exchange configuration models</returns>
    public async Task<IEnumerable<ExchangeConfigurationModel>> GetAllSupportedAsync()
    {
        _logger.LogDebug("Getting all supported exchange configurations");

        try
        {
            var entities = await _context.ExchangeConfigurations
                .Include(e => e.FeeTiers.Where(ft => ft.IsActive))
                .Include(e => e.SymbolOverrides.Where(so => so.IsActive))
                .Where(e => e.IsSupported && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var results = entities.ToBusinessModels().ToList();

            _logger.LogInformation("Retrieved {Count} supported exchange configurations", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get supported exchange configurations");
            throw;
        }
    }

    /// <summary>
    /// Gets all exchange configurations including inactive ones
    /// </summary>
    /// <returns>Collection of all exchange configuration models</returns>
    public async Task<IEnumerable<ExchangeConfigurationModel>> GetAllAsync()
    {
        _logger.LogDebug("Getting all exchange configurations");

        try
        {
            var entities = await _context.ExchangeConfigurations
                .Include(e => e.FeeTiers)
                .Include(e => e.SymbolOverrides)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var results = entities.ToBusinessModels().ToList();

            _logger.LogDebug("Retrieved {Count} total exchange configurations", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all exchange configurations");
            throw;
        }
    }

    /// <summary>
    /// Creates or updates exchange configuration
    /// </summary>
    /// <param name="configuration">Exchange configuration to upsert</param>
    /// <returns>Created or updated exchange configuration model</returns>
    public async Task<ExchangeConfigurationModel> UpsertAsync(ExchangeConfigurationModel configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _logger.LogInformation("Upserting configuration for exchange {Exchange}", configuration.Exchange);

        try
        {
            var existing = await _context.ExchangeConfigurations
                .FirstOrDefaultAsync(e => e.Exchange == (int)configuration.Exchange);

            if (existing != null)
            {
                // Update existing
                existing.Name = configuration.Name;
                existing.IsActive = configuration.IsActive;
                existing.IsSupported = configuration.IsSupported;
                existing.BaseMakerFeePercent = configuration.BaseMakerFeePercent;
                existing.BaseTakerFeePercent = configuration.BaseTakerFeePercent;
                existing.ApiEndpoint = configuration.ApiEndpoint;
                existing.MaxRequestsPerMinute = configuration.MaxRequestsPerMinute;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";

                _context.ExchangeConfigurations.Update(existing);
            }
            else
            {
                // Create new
                var entity = configuration.ToEntity();
                if (entity.Id == Guid.Empty)
                    entity.Id = Guid.NewGuid();

                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = "System";

                await _context.ExchangeConfigurations.AddAsync(entity);
            }

            await _context.SaveChangesAsync();

            // Return updated/created entity
            var result = await GetByExchangeAsync(configuration.Exchange);
            _logger.LogInformation("Successfully upserted configuration for {Exchange}", configuration.Exchange);

            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert configuration for exchange {Exchange}", configuration.Exchange);
            throw;
        }
    }

    /// <summary>
    /// Updates base fees for an exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="makerFee">Maker fee percentage</param>
    /// <param name="takerFee">Taker fee percentage</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateBaseFeesAsync(Exchange exchange, decimal makerFee, decimal takerFee)
    {
        _logger.LogInformation("Updating base fees for {Exchange}: Maker {Maker}%, Taker {Taker}%",
            exchange, makerFee, takerFee);

        try
        {
            var entity = await _context.ExchangeConfigurations
                .FirstOrDefaultAsync(e => e.Exchange == (int)exchange);

            if (entity == null)
            {
                _logger.LogWarning("Exchange configuration not found for {Exchange}", exchange);
                return false;
            }

            entity.BaseMakerFeePercent = makerFee;
            entity.BaseTakerFeePercent = takerFee;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";

            _context.ExchangeConfigurations.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated base fees for {Exchange}", exchange);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update base fees for {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Gets fee tiers for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <returns>Collection of fee tier models ordered by tier level</returns>
    public async Task<IEnumerable<FeeTierModel>> GetFeeTiersAsync(Exchange exchange)
    {
        _logger.LogDebug("Getting fee tiers for exchange {Exchange}", exchange);

        try
        {
            var entities = await _context.FeeTiers
                .Where(ft => ft.Exchange == (int)exchange && ft.IsActive)
                .OrderBy(ft => ft.TierLevel)
                .ToListAsync();

            var results = entities.ToBusinessModels().ToList();

            _logger.LogDebug("Retrieved {Count} fee tiers for {Exchange}", results.Count, exchange);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fee tiers for exchange {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Adds or updates a fee tier for an exchange
    /// </summary>
    /// <param name="feeTier">Fee tier to add or update</param>
    /// <returns>Created or updated fee tier model</returns>
    public async Task<FeeTierModel> UpsertFeeTierAsync(FeeTierModel feeTier)
    {
        ArgumentNullException.ThrowIfNull(feeTier);

        _logger.LogInformation("Upserting fee tier {Tier} for exchange {Exchange}",
            feeTier.TierLevel, feeTier.Exchange);

        try
        {
            var existing = await _context.FeeTiers
                .FirstOrDefaultAsync(ft => ft.Exchange == (int)feeTier.Exchange && ft.TierLevel == feeTier.TierLevel);

            if (existing != null)
            {
                // Update existing
                existing.MinimumVolumeThreshold = feeTier.MinimumVolumeThreshold;
                existing.MakerFeePercent = feeTier.MakerFeePercent;
                existing.TakerFeePercent = feeTier.TakerFeePercent;
                existing.IsActive = feeTier.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";

                _context.FeeTiers.Update(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated existing fee tier {Tier} for {Exchange}", feeTier.TierLevel, feeTier.Exchange);
                return existing.ToBusinessModel();
            }
            else
            {
                // Create new
                var entity = feeTier.ToEntity();
                if (entity.Id == Guid.Empty)
                    entity.Id = Guid.NewGuid();

                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = "System";

                await _context.FeeTiers.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new fee tier {Tier} for {Exchange}", feeTier.TierLevel, feeTier.Exchange);
                return entity.ToBusinessModel();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert fee tier {Tier} for exchange {Exchange}",
                feeTier.TierLevel, feeTier.Exchange);
            throw;
        }
    }

    /// <summary>
    /// Removes a fee tier
    /// </summary>
    /// <param name="id">Fee tier ID to remove</param>
    /// <returns>True if removal was successful</returns>
    public async Task<bool> RemoveFeeTierAsync(Guid id)
    {
        _logger.LogInformation("Removing fee tier {Id}", id);

        try
        {
            var entity = await _context.FeeTiers.FirstOrDefaultAsync(ft => ft.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Fee tier {Id} not found", id);
                return false;
            }

            // Soft delete
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";

            _context.FeeTiers.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully removed fee tier {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove fee tier {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets symbol fee overrides for a specific exchange
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <returns>Collection of symbol fee override models</returns>
    public async Task<IEnumerable<SymbolFeeOverrideModel>> GetSymbolOverridesAsync(Exchange exchange, string? symbol = null)
    {
        _logger.LogDebug("Getting symbol overrides for exchange {Exchange}, symbol: {Symbol}",
            exchange, symbol ?? "all");

        try
        {
            var query = _context.SymbolFeeOverrides
                .Where(so => so.Exchange == (int)exchange && so.IsActive);

            if (!string.IsNullOrEmpty(symbol))
            {
                query = query.Where(so => so.Symbol == symbol.ToUpperInvariant());
            }

            var entities = await query.OrderBy(so => so.Symbol).ToListAsync();
            var results = entities.ToBusinessModels().ToList();

            _logger.LogDebug("Retrieved {Count} symbol overrides for {Exchange}",
                results.Count, exchange);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbol overrides for exchange {Exchange}", exchange);
            throw;
        }
    }

    /// <summary>
    /// Adds or updates a symbol fee override
    /// </summary>
    /// <param name="symbolOverride">Symbol fee override to add or update</param>
    /// <returns>Created or updated symbol fee override model</returns>
    public async Task<SymbolFeeOverrideModel> UpsertSymbolOverrideAsync(SymbolFeeOverrideModel symbolOverride)
    {
        ArgumentNullException.ThrowIfNull(symbolOverride);

        _logger.LogInformation("Upserting symbol override for {Exchange} {Symbol}",
            symbolOverride.Exchange, symbolOverride.Symbol);

        try
        {
            var existing = await _context.SymbolFeeOverrides
                .FirstOrDefaultAsync(so => so.Exchange == (int)symbolOverride.Exchange &&
                                         so.Symbol == symbolOverride.Symbol.ToUpperInvariant());

            if (existing != null)
            {
                // Update existing
                existing.MakerFeePercent = symbolOverride.MakerFeePercent;
                existing.TakerFeePercent = symbolOverride.TakerFeePercent;
                existing.IsActive = symbolOverride.IsActive;
                existing.Reason = symbolOverride.Reason;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";

                _context.SymbolFeeOverrides.Update(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated existing symbol override for {Exchange} {Symbol}",
                    symbolOverride.Exchange, symbolOverride.Symbol);
                return existing.ToBusinessModel();
            }
            else
            {
                // Create new
                var entity = symbolOverride.ToEntity();
                if (entity.Id == Guid.Empty)
                    entity.Id = Guid.NewGuid();

                entity.Symbol = entity.Symbol.ToUpperInvariant();
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = "System";

                await _context.SymbolFeeOverrides.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new symbol override for {Exchange} {Symbol}",
                    symbolOverride.Exchange, symbolOverride.Symbol);
                return entity.ToBusinessModel();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert symbol override for {Exchange} {Symbol}",
                symbolOverride.Exchange, symbolOverride.Symbol);
            throw;
        }
    }

    /// <summary>
    /// Removes a symbol fee override
    /// </summary>
    /// <param name="id">Symbol override ID to remove</param>
    /// <returns>True if removal was successful</returns>
    public async Task<bool> RemoveSymbolOverrideAsync(Guid id)
    {
        _logger.LogInformation("Removing symbol override {Id}", id);

        try
        {
            var entity = await _context.SymbolFeeOverrides.FirstOrDefaultAsync(so => so.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Symbol override {Id} not found", id);
                return false;
            }

            // Soft delete
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";

            _context.SymbolFeeOverrides.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully removed symbol override {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove symbol override {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets the effective fee tier for a given exchange and volume
    /// </summary>
    /// <param name="exchange">Exchange type</param>
    /// <param name="volume">Trading volume</param>
    /// <returns>Applicable fee tier model or null if none found</returns>
    public async Task<FeeTierModel?> GetEffectiveFeeTierAsync(Exchange exchange, decimal volume)
    {
        _logger.LogDebug("Getting effective fee tier for {Exchange} with volume {Volume}",
            exchange, volume);

        try
        {
            var entity = await _context.FeeTiers
                .Where(ft => ft.Exchange == (int)exchange &&
                           ft.IsActive &&
                           ft.MinimumVolumeThreshold <= volume)
                .OrderByDescending(ft => ft.MinimumVolumeThreshold)
                .FirstOrDefaultAsync();

            var result = entity?.ToBusinessModel();

            _logger.LogDebug("Found effective tier for {Exchange} volume {Volume}: {TierLevel}",
                exchange, volume, result?.TierLevel.ToString() ?? "None");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get effective fee tier for {Exchange}", exchange);
            throw;
        }
    }
}