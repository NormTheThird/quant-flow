namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing portfolio operations
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly ILogger<PortfolioService> _logger;
    private readonly IPortfolioRepository _portfolioRepository;

    public PortfolioService(ILogger<PortfolioService> logger, IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PortfolioModel?> GetPortfolioByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting portfolio with ID: {PortfolioId}", id);
        return await _portfolioRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<PortfolioModel>> GetPortfoliosByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting portfolios for user: {UserId}", userId);
        return await _portfolioRepository.GetByUserIdAsync(userId);
    }

    public async Task<PortfolioModel> CreatePortfolioAsync(PortfolioModel portfolio)
    {
        _logger.LogInformation("Creating portfolio: {Name} for user: {UserId}", portfolio.Name, portfolio.UserId);

        portfolio.Status = PortfolioStatus.Inactive;
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
}