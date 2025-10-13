namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing algorithm position operations
/// </summary>
public class AlgorithmPositionService : IAlgorithmPositionService
{
    private readonly ILogger<AlgorithmPositionService> _logger;
    private readonly IAlgorithmPositionRepository _algorithmPositionRepository;

    public AlgorithmPositionService(
        ILogger<AlgorithmPositionService> logger,
        IAlgorithmPositionRepository algorithmPositionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmPositionRepository = algorithmPositionRepository ?? throw new ArgumentNullException(nameof(algorithmPositionRepository));
    }

    public async Task<AlgorithmPositionModel?> GetPositionByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm position with ID: {PositionId}", id);
        return await _algorithmPositionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<AlgorithmPositionModel>> GetPositionsByPortfolioIdAsync(Guid portfolioId)
    {
        _logger.LogInformation("Getting algorithm positions for portfolio: {PortfolioId}", portfolioId);
        return await _algorithmPositionRepository.GetByPortfolioIdAsync(portfolioId);
    }

    public async Task<AlgorithmPositionModel> CreatePositionAsync(AlgorithmPositionModel position)
    {
        _logger.LogInformation("Creating algorithm position for portfolio: {PortfolioId}", position.PortfolioId);

        position.Status = Status.Inactive;
        position.CreatedAt = DateTime.UtcNow;
        position.UpdatedAt = DateTime.UtcNow;

        return await _algorithmPositionRepository.CreateAsync(position);
    }

    public async Task<AlgorithmPositionModel> UpdatePositionAsync(AlgorithmPositionModel position)
    {
        _logger.LogInformation("Updating algorithm position: {PositionId}", position.Id);

        position.UpdatedAt = DateTime.UtcNow;

        return await _algorithmPositionRepository.UpdateAsync(position);
    }

    public async Task<bool> DeletePositionAsync(Guid id)
    {
        _logger.LogInformation("Deleting algorithm position: {PositionId}", id);
        return await _algorithmPositionRepository.DeleteAsync(id);
    }
}