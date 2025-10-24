using QuantFlow.Common.Interfaces.Repositories.SQLServer;

namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing portfolio operations
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly ILogger<PortfolioService> _logger;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IKrakenApiService _krakenApiService;
    private readonly IEncryptionService _encryptionService;

    public PortfolioService(
        ILogger<PortfolioService> logger,
        IPortfolioRepository portfolioRepository,
        IServiceScopeFactory serviceScopeFactory,
        IKrakenApiService krakenApiService,
        IEncryptionService encryptionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _krakenApiService = krakenApiService ?? throw new ArgumentNullException(nameof(krakenApiService));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    public async Task<PortfolioModel?> GetPortfolioByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting portfolio with ID: {PortfolioId}", id);
        var portfolio = await _portfolioRepository.GetByIdAsync(id);

        if (portfolio != null && portfolio.Mode == PortfolioMode.ExchangeConnected)
        {
            await UpdatePortfolioBalanceAsync(portfolio);
        }

        return portfolio;
    }

    public async Task<IEnumerable<PortfolioModel>> GetPortfoliosByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting portfolios for user: {UserId}", userId);
        var portfolios = (await _portfolioRepository.GetByUserIdAsync(userId)).ToList();

        // Update balances for all Exchange Connected portfolios
        foreach (var portfolio in portfolios.Where(p => p.Mode == PortfolioMode.ExchangeConnected))
        {
            await UpdatePortfolioBalanceAsync(portfolio);
        }

        return portfolios;
    }

    public async Task<PortfolioModel> CreatePortfolioAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Creating portfolio: {Name} for user: {UserId}", portfolio.Name, portfolio.UserId);

        // If Exchange Connected mode, validate API credentials and get initial balance
        if (portfolio.Mode == PortfolioMode.ExchangeConnected)
        {
            await ValidateExchangeCredentialsAsync(portfolio);

            // Get current balance from exchange as the initial balance
            var currentBalance = await GetBalanceFromExchangeAsync(portfolio);
            portfolio.InitialBalance = currentBalance;
            portfolio.CurrentBalance = currentBalance;

            _logger.LogInformation("Set initial balance from exchange: {Balance} {Currency}",
                currentBalance, portfolio.BaseCurrency);
        }

        portfolio.Status = Status.Inactive;
        portfolio.CreatedAt = DateTime.UtcNow;
        portfolio.UpdatedAt = DateTime.UtcNow;

        return await _portfolioRepository.CreateAsync(portfolio);
    }

    public async Task<PortfolioModel> UpdatePortfolioAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Updating portfolio: {PortfolioId}", portfolio.Id);
        portfolio.UpdatedAt = DateTime.UtcNow;
        return await _portfolioRepository.UpdateAsync(portfolio);
    }

    public async Task<bool> DeletePortfolioAsync(Guid id)
    {
        _logger.LogInformation("Deleting portfolio: {PortfolioId}", id);
        return await _portfolioRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Validates exchange API credentials for the portfolio
    /// </summary>
    private async Task ValidateExchangeCredentialsAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Validating API credentials for exchange: {Exchange}", portfolio.Exchange);

        // Create a new scope to avoid DbContext conflicts
        using var scope = _serviceScopeFactory.CreateScope();
        var userExchangeDetailsRepository = scope.ServiceProvider.GetRequiredService<IUserExchangeDetailsRepository>();

        // Get user's exchange API credentials
        var exchangeDetails = await userExchangeDetailsRepository
            .GetByUserAndExchangeAsync(portfolio.UserId, portfolio.Exchange.ToString());

        var apiKeyDetail = exchangeDetails.FirstOrDefault(d => d.KeyName == "ApiKey");
        var apiSecretDetail = exchangeDetails.FirstOrDefault(d => d.KeyName == "ApiSecret");

        if (apiKeyDetail == null || apiSecretDetail == null)
        {
            throw new InvalidOperationException($"No API credentials configured for {portfolio.Exchange}. Please add your API keys in Settings.");
        }

        // Decrypt the keys
        var apiKey = apiKeyDetail.IsEncrypted
            ? _encryptionService.Decrypt(apiKeyDetail.KeyValue)
            : apiKeyDetail.KeyValue;

        var apiSecret = apiSecretDetail.IsEncrypted
            ? _encryptionService.Decrypt(apiSecretDetail.KeyValue)
            : apiSecretDetail.KeyValue;

        // Validate credentials based on exchange
        bool isValid = portfolio.Exchange switch
        {
            Exchange.Kraken => await _krakenApiService.ValidateCredentialsAsync(apiKey, apiSecret),
            Exchange.KuCoin => throw new NotImplementedException("KuCoin validation not yet implemented"),
            _ => throw new NotSupportedException($"Exchange {portfolio.Exchange} is not supported")
        };

        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid API credentials for {portfolio.Exchange}. Please check your API keys in Settings.");
        }

        _logger.LogInformation("API credentials validated successfully for {Exchange}", portfolio.Exchange);
    }

    /// <summary>
    /// Updates portfolio balance from exchange
    /// </summary>
    private async Task UpdatePortfolioBalanceAsync(PortfolioModel portfolio)
    {
        try
        {
            _logger.LogInformation("Updating balance for portfolio: {PortfolioId} from {Exchange}", portfolio.Id, portfolio.Exchange);

            var balance = await GetBalanceFromExchangeAsync(portfolio);
            portfolio.CurrentBalance = balance;

            _logger.LogInformation("Updated portfolio {PortfolioId} balance to {Balance} {Currency}",
                portfolio.Id, balance, portfolio.BaseCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update balance for portfolio {PortfolioId}", portfolio.Id);
            // Don't throw - just log and continue with stale balance
        }
    }

    /// <summary>
    /// Gets balance from exchange for a portfolio
    /// </summary>
    private async Task<decimal> GetBalanceFromExchangeAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Getting balance from {Exchange} for {Currency}", portfolio.Exchange, portfolio.BaseCurrency);

        // Create a new scope to avoid DbContext conflicts
        using var scope = _serviceScopeFactory.CreateScope();
        var userExchangeDetailsRepository = scope.ServiceProvider.GetRequiredService<IUserExchangeDetailsRepository>();

        // Get user's exchange API credentials
        var exchangeDetails = await userExchangeDetailsRepository
            .GetByUserAndExchangeAsync(portfolio.UserId, portfolio.Exchange.ToString());

        var apiKeyDetail = exchangeDetails.FirstOrDefault(d => d.KeyName == "ApiKey");
        var apiSecretDetail = exchangeDetails.FirstOrDefault(d => d.KeyName == "ApiSecret");

        if (apiKeyDetail == null || apiSecretDetail == null)
            throw new InvalidOperationException($"No API credentials found for {portfolio.Exchange}");

        // Decrypt the keys
        var apiKey = apiKeyDetail.IsEncrypted
            ? _encryptionService.Decrypt(apiKeyDetail.KeyValue)
            : apiKeyDetail.KeyValue;

        var apiSecret = apiSecretDetail.IsEncrypted
            ? _encryptionService.Decrypt(apiSecretDetail.KeyValue)
            : apiSecretDetail.KeyValue;

        // Get balance based on exchange and base currency
        return portfolio.Exchange switch
        {
            Exchange.Kraken => await _krakenApiService.GetCurrencyBalanceAsync(apiKey, apiSecret, portfolio.BaseCurrency.ToString()),
            Exchange.KuCoin => throw new NotImplementedException("KuCoin balance retrieval not yet implemented"),
            _ => throw new NotSupportedException($"Exchange {portfolio.Exchange} is not supported")
        };
    }
}