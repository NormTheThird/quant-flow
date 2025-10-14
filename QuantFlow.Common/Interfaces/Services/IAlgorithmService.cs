namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for algorithm operations
/// </summary>
public interface IAlgorithmService
{
    /// <summary>
    /// Gets an algorithm by its unique identifier
    /// </summary>
    /// <param name="id">The algorithm's unique identifier</param>
    /// <returns>Algorithm if found, null otherwise</returns>
    Task<AlgorithmModel?> GetAlgorithmByIdAsync(Guid id);

    /// <summary>
    /// Gets an algorithm by name for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="name">The algorithm name</param>
    /// <returns>Algorithm if found, null otherwise</returns>
    Task<AlgorithmModel?> GetAlgorithmByNameAsync(Guid userId, string name);

    /// <summary>
    /// Gets all algorithms for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all public algorithms
    /// </summary>
    /// <returns>Collection of public algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync();

    /// <summary>
    /// Searches algorithms by name or description
    /// </summary>
    /// <param name="searchTerm">Text to search for</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of matching algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null);

    /// <summary>
    /// Gets algorithms by status
    /// </summary>
    /// <param name="status">Algorithm status to filter by</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByStatusAsync(AlgorithmStatus status, Guid? userId = null);

    /// <summary>
    /// Creates a new algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm to create</param>
    /// <returns>Created algorithm</returns>
    Task<AlgorithmModel> CreateAlgorithmAsync(AlgorithmModel algorithm);

    /// <summary>
    /// Updates an existing algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm with updates</param>
    /// <returns>Updated algorithm</returns>
    Task<AlgorithmModel> UpdateAlgorithmAsync(AlgorithmModel algorithm);

    /// <summary>
    /// Deletes an algorithm
    /// </summary>
    /// <param name="id">The algorithm's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAlgorithmAsync(Guid id);

    /// <summary>
    /// Gets the count of algorithms for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Number of algorithms owned by the user</returns>
    Task<long> GetAlgorithmCountByUserIdAsync(Guid userId);
}