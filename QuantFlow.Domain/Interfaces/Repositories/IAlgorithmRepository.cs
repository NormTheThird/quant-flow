namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for algorithm operations using business models
/// </summary>
public interface IAlgorithmRepository
{
    Task<AlgorithmModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<AlgorithmModel>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync();
    Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null);
    Task<IEnumerable<AlgorithmModel>> GetByTagsAsync(IEnumerable<string> tags, Guid? userId = null);
    Task<AlgorithmModel> CreateAsync(AlgorithmModel algorithm);
    Task<AlgorithmModel> UpdateAsync(AlgorithmModel algorithm);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<AlgorithmModel>> GetByStatusAsync(AlgorithmStatus status, Guid? userId = null);
    Task<IEnumerable<AlgorithmModel>> GetTemplatesAsync();
    Task<long> CountByUserIdAsync(Guid userId);
}