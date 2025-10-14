namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing algorithm operations
/// </summary>
public class AlgorithmService : IAlgorithmService
{
    private readonly ILogger<AlgorithmService> _logger;
    private readonly IAlgorithmRepository _algorithmRepository;

    public AlgorithmService(ILogger<AlgorithmService> logger, IAlgorithmRepository algorithmRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmRepository = algorithmRepository ?? throw new ArgumentNullException(nameof(algorithmRepository));
    }

    public async Task<AlgorithmModel?> GetAlgorithmByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm with ID: {AlgorithmId}", id);
        return await _algorithmRepository.GetByIdAsync(id);
    }

    public async Task<AlgorithmModel?> GetAlgorithmByNameAsync(Guid userId, string name)
    {
        _logger.LogInformation("Getting algorithm by name: {Name} for user: {UserId}", name, userId);
        return await _algorithmRepository.GetByNameAsync(userId, name);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithms for user: {UserId}", userId);
        return await _algorithmRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync()
    {
        _logger.LogInformation("Getting public algorithms");
        return await _algorithmRepository.GetPublicAlgorithmsAsync();
    }

    public async Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null)
    {
        _logger.LogInformation("Searching algorithms with term: {SearchTerm}", searchTerm);
        return await _algorithmRepository.SearchAlgorithmsAsync(searchTerm, userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByStatusAsync(AlgorithmStatus status, Guid? userId = null)
    {
        _logger.LogInformation("Getting algorithms by status: {Status}", status);
        return await _algorithmRepository.GetByStatusAsync(status, userId);
    }

    public async Task<AlgorithmModel> CreateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Creating algorithm: {Name} for user: {UserId}", algorithm.Name, algorithm.UserId);

        // Check if algorithm name already exists for this user
        var existingAlgorithm = await _algorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null)
        {
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");
        }

        algorithm.Status = AlgorithmStatus.Draft;
        algorithm.CreatedAt = DateTime.UtcNow;
        algorithm.UpdatedAt = DateTime.UtcNow;

        return await _algorithmRepository.CreateAsync(algorithm);
    }

    public async Task<AlgorithmModel> UpdateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Updating algorithm: {AlgorithmId}", algorithm.Id);

        // Check if algorithm name already exists for this user (excluding current algorithm)
        var existingAlgorithm = await _algorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null && existingAlgorithm.Id != algorithm.Id)
        {
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");
        }

        algorithm.UpdatedAt = DateTime.UtcNow;

        return await _algorithmRepository.UpdateAsync(algorithm);
    }

    public async Task<bool> DeleteAlgorithmAsync(Guid id)
    {
        _logger.LogInformation("Deleting algorithm: {AlgorithmId}", id);
        return await _algorithmRepository.DeleteAsync(id);
    }

    public async Task<long> GetAlgorithmCountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithm count for user: {UserId}", userId);
        return await _algorithmRepository.CountByUserIdAsync(userId);
    }
}