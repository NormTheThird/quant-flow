namespace QuantFlow.Domain.Services;

/// <summary>
/// Service implementation for symbol business operations
/// </summary>
public class SymbolService : ISymbolService
{
    private readonly ILogger<SymbolService> _logger;
    private readonly ISymbolRepository _symbolRepository;

    public SymbolService(ILogger<SymbolService> logger, ISymbolRepository symbolRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _symbolRepository = symbolRepository ?? throw new ArgumentNullException(nameof(symbolRepository));
    }

    public async Task<SymbolModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Service: Getting symbol by ID {SymbolId}", id);
        return await _symbolRepository.GetByIdAsync(id);
    }

    public async Task<SymbolModel?> GetBySymbolAsync(string symbol)
    {
        _logger.LogInformation("Service: Getting symbol by name {Symbol}", symbol);
        return await _symbolRepository.GetBySymbolAsync(symbol);
    }

    public async Task<IEnumerable<SymbolModel>> GetAllAsync()
    {
        _logger.LogInformation("Service: Getting all symbols");
        return await _symbolRepository.GetAllAsync();
    }

    public async Task<IEnumerable<SymbolModel>> GetActiveAsync()
    {
        _logger.LogInformation("Service: Getting active symbols");
        return await _symbolRepository.GetActiveAsync();
    }

    public async Task<SymbolModel> CreateAsync(SymbolModel symbol)
    {
        _logger.LogInformation("Service: Creating symbol {Symbol}", symbol.Symbol);
        return await _symbolRepository.CreateAsync(symbol);
    }

    public async Task<SymbolModel> RestoreSymbolAsync(SymbolModel symbol)
    {
        _logger.LogInformation("Service: Restoring symbol {SymbolId}", symbol.Id);
        return await _symbolRepository.RestoreAsync(symbol);
    }

    public async Task<SymbolModel> UpdateAsync(SymbolModel symbol)
    {
        _logger.LogInformation("Service: Updating symbol {SymbolId}", symbol.Id);
        return await _symbolRepository.UpdateAsync(symbol);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Service: Deleting symbol {SymbolId}", id);
        return await _symbolRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SymbolModel>> GetByBaseAssetAsync(string baseAsset)
    {
        _logger.LogInformation("Service: Getting symbols by base asset {BaseAsset}", baseAsset);
        return await _symbolRepository.GetByBaseAssetAsync(baseAsset);
    }

    public async Task<IEnumerable<SymbolModel>> GetByQuoteAssetAsync(string quoteAsset)
    {
        _logger.LogInformation("Service: Getting symbols by quote asset {QuoteAsset}", quoteAsset);
        return await _symbolRepository.GetByQuoteAssetAsync(quoteAsset);
    }
}