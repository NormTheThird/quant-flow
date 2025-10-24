namespace QuantFlow.Common.Interfaces.Repositories.SQLServer;

/// <summary>
/// Repository interface for algorithm metadata operations
/// </summary>
public interface IAlgorithmRepository
{
    /// <summary>
    /// Gets an algorithm by its unique identifier
    /// </summary>
    Task<AlgorithmMetadataModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all algorithms
    /// </summary>
    Task<IEnumerable<AlgorithmMetadataModel>> GetAllAsync();

    /// <summary>
    /// Gets all enabled algorithms
    /// </summary>
    Task<IEnumerable<AlgorithmMetadataModel>> GetEnabledAsync();

    /// <summary>
    /// Gets algorithms by source type
    /// </summary>
    Task<IEnumerable<AlgorithmMetadataModel>> GetBySourceAsync(AlgorithmSource source);

    /// <summary>
    /// Gets algorithms by type
    /// </summary>
    Task<IEnumerable<AlgorithmMetadataModel>> GetByTypeAsync(AlgorithmType type);

    /// <summary>
    /// Creates a new algorithm
    /// </summary>
    Task<AlgorithmMetadataModel> CreateAsync(AlgorithmMetadataModel algorithm);

    /// <summary>
    /// Updates an existing algorithm
    /// </summary>
    Task<AlgorithmMetadataModel> UpdateAsync(AlgorithmMetadataModel algorithm);

    /// <summary>
    /// Deletes an algorithm (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Sets the enabled status for an algorithm
    /// </summary>
    Task<bool> SetEnabledAsync(Guid id, bool isEnabled);
}