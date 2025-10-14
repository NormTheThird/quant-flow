namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing algorithm position operations
/// </summary>
public class AlgorithmPositionService : IAlgorithmPositionService
{
    private readonly ILogger<AlgorithmPositionService> _logger;
    private readonly IAlgorithmPositionRepository _algorithmPositionRepository;

    public AlgorithmPositionService(ILogger<AlgorithmPositionService> logger, IAlgorithmPositionRepository algorithmPositionRepository)
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

    /// <summary>
    /// Gets all unassigned positions (not linked to any portfolio) for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of unassigned positions</returns>
    public async Task<IEnumerable<AlgorithmPositionModel>> GetUnassignedPositionsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting unassigned positions for user: {UserId}", userId);

        var positions = await _algorithmPositionRepository.GetByUserIdAsync(userId);

        return positions.Where(_ => _.PortfolioId == null);
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