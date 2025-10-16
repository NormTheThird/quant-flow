namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for algorithm position operations
/// </summary>
public interface IAlgorithmPositionService
{
    /// <summary>
    /// Gets an algorithm position by its unique identifier
    /// </summary>
    /// <param name="id">The position's unique identifier</param>
    /// <returns>AlgorithmPosition if found, null otherwise</returns>
    Task<AlgorithmPositionModel?> GetPositionByIdAsync(Guid id);

    /// <summary>
    /// Gets all algorithm positions for a specific portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio's unique identifier</param>
    /// <returns>Collection of algorithm positions</returns>
    Task<IEnumerable<AlgorithmPositionModel>> GetPositionsByPortfolioIdAsync(Guid portfolioId);

    /// <summary>
    /// Gets all unassigned positions (not linked to any portfolio) for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of unassigned positions</returns>
    Task<IEnumerable<AlgorithmPositionModel>> GetPositionsByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new algorithm position
    /// </summary>
    /// <param name="position">AlgorithmPosition to create</param>
    /// <returns>Created algorithm position</returns>
    Task<AlgorithmPositionModel> CreatePositionAsync(AlgorithmPositionModel position);

    /// <summary>
    /// Updates an existing algorithm position
    /// </summary>
    /// <param name="position">AlgorithmPosition with updates</param>
    /// <returns>Updated algorithm position</returns>
    Task<AlgorithmPositionModel> UpdatePositionAsync(AlgorithmPositionModel position);

    /// <summary>
    /// Deletes an algorithm position
    /// </summary>
    /// <param name="id">The position's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeletePositionAsync(Guid id);
}