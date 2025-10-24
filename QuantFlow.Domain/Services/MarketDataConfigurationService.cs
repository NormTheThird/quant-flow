using QuantFlow.Common.Interfaces.Repositories.SQLServer;

namespace QuantFlow.Domain.Services;

/// <summary>
/// Service implementation for market data configuration operations
/// </summary>
public class MarketDataConfigurationService : IMarketDataConfigurationService
{
    private readonly ILogger<MarketDataConfigurationService> _logger;
    private readonly IMarketDataConfigurationRepository _repository;
    private readonly ISymbolRepository _symbolRepository;

    public MarketDataConfigurationService(ILogger<MarketDataConfigurationService> logger, IMarketDataConfigurationRepository repository,
                                          ISymbolRepository symbolRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _symbolRepository = symbolRepository ?? throw new ArgumentNullException(nameof(symbolRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<MarketDataConfigurationModel>> GetAllConfigurationsAsync()
    {
        _logger.LogInformation("Service: Getting all market data configurations");
        return await _repository.GetAllAsync();
    }

    public async Task<MarketDataConfigurationModel?> GetConfigurationByIdAsync(Guid id)
    {
        _logger.LogInformation("Service: Getting configuration by ID {Id}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<MarketDataConfigurationModel> CreateAsync(MarketDataConfigurationModel model)
    {
        _logger.LogInformation("Service: Creating configuration for symbol {SymbolId} on {Exchange}", model.SymbolId, model.Exchange);

        var symbolExists = await _symbolRepository.ExistsAsync(model.SymbolId);
        if (!symbolExists)
            throw new InvalidOperationException($"Symbol with ID {model.SymbolId} does not exist");

        var configExists = await _repository.ExistsAsync(model.SymbolId, model.Exchange);
        if (configExists)
            throw new InvalidOperationException($"Configuration already exists for symbol {model.SymbolId} on exchange {model.Exchange}");

        return await _repository.CreateAsync(model);
    }

    public async Task<MarketDataConfigurationModel?> ToggleIntervalAsync(Guid id, string interval, bool isActive)
    {
        _logger.LogInformation("Service: Toggling interval {Interval} to {IsActive} for configuration {Id}", interval, isActive, id);

        var configuration = await _repository.GetByIdAsync(id);
        if (configuration == null)
            return null;

        switch (interval.ToLower())
        {
            case "1m":
                configuration.Is1mActive = isActive;
                break;
            case "5m":
                configuration.Is5mActive = isActive;
                break;
            case "15m":
                configuration.Is15mActive = isActive;
                break;
            case "1h":
                configuration.Is1hActive = isActive;
                break;
            case "4h":
                configuration.Is4hActive = isActive;
                break;
            case "1d":
                configuration.Is1dActive = isActive;
                break;
            default:
                throw new ArgumentException($"Invalid interval: {interval}");
        }

        return await _repository.UpdateAsync(configuration);
    }

    public async Task<MarketDataConfigurationModel?> UpdateAsync(MarketDataConfigurationModel model)
    {
        _logger.LogInformation("Service: Updating configuration {Id}", model.Id);
        return await _repository.UpdateAsync(model);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Service: Deleting configuration {Id}", id);

        var configuration = await _repository.GetByIdAsync(id);
        if (configuration == null)
            return false;

        await _repository.DeleteAsync(new[] { id });
        return true;
    }
}