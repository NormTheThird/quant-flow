namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for algorithm effectiveness operations
/// </summary>
public interface IAlgorithmEffectivenessRepository
{
    /// <summary>
    /// Gets effectiveness rating by unique identifier
    /// </summary>
    Task<AlgorithmEffectivenessModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all effectiveness ratings for an algorithm
    /// </summary>
    Task<IEnumerable<AlgorithmEffectivenessModel>> GetByAlgorithmIdAsync(Guid algorithmId);

    /// <summary>
    /// Gets effectiveness rating for specific algorithm and timeframe
    /// </summary>
    Task<AlgorithmEffectivenessModel?> GetByAlgorithmAndTimeframeAsync(Guid algorithmId, string timeframe);

    /// <summary>
    /// Gets all effectiveness ratings for a timeframe
    /// </summary>
    Task<IEnumerable<AlgorithmEffectivenessModel>> GetByTimeframeAsync(string timeframe);

    /// <summary>
    /// Creates a new effectiveness rating
    /// </summary>
    Task<AlgorithmEffectivenessModel> CreateAsync(AlgorithmEffectivenessModel effectiveness);

    /// <summary>
    /// Updates an existing effectiveness rating
    /// </summary>
    Task<AlgorithmEffectivenessModel> UpdateAsync(AlgorithmEffectivenessModel effectiveness);

    /// <summary>
    /// Deletes an effectiveness rating (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
} 