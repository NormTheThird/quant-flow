namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for algorithm position operations
/// </summary>
public interface IAlgorithmPositionRepository
{
    /// <summary>
    /// Gets an algorithm position by its unique identifier
    /// </summary>
    /// <param name="id">The position's unique identifier</param>
    /// <returns>AlgorithmPosition if found, null otherwise</returns>
    Task<AlgorithmPositionModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all algorithm positions for a specific portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio's unique identifier</param>
    /// <returns>Collection of algorithm positions</returns>
    Task<IEnumerable<AlgorithmPositionModel>> GetByPortfolioIdAsync(Guid portfolioId);

    /// <summary>
    /// Creates a new algorithm position
    /// </summary>
    /// <param name="position">AlgorithmPosition to create</param>
    /// <returns>Created algorithm position</returns>
    Task<AlgorithmPositionModel> CreateAsync(AlgorithmPositionModel position);

    /// <summary>
    /// Updates an existing algorithm position
    /// </summary>
    /// <param name="position">AlgorithmPosition with updates</param>
    /// <returns>Updated algorithm position</returns>
    Task<AlgorithmPositionModel> UpdateAsync(AlgorithmPositionModel position);

    /// <summary>
    /// Soft deletes an algorithm position
    /// </summary>
    /// <param name="id">The position's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);
}