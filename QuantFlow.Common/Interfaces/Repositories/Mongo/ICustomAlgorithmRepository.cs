namespace QuantFlow.Common.Interfaces.Repositories.Mongo;

/// <summary>
/// Repository interface for algorithm operations using business models
/// </summary>
public interface ICustomAlgorithmRepository
{
    /// <summary>
    /// Gets an algorithm by its unique identifier
    /// </summary>
    /// <param name="id">The algorithm's unique identifier</param>
    /// <returns>Algorithm if found, null otherwise</returns>
    Task<AlgorithmModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets an algorithm by name for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="name">The algorithm name</param>
    /// <returns>Algorithm if found, null otherwise</returns>
    Task<AlgorithmModel?> GetByNameAsync(Guid userId, string name);

    /// <summary>
    /// Gets all algorithms for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all public algorithms available to all users
    /// </summary>
    /// <returns>Collection of public algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync();

    /// <summary>
    /// Searches algorithms by name or description
    /// </summary>
    /// <param name="searchTerm">Text to search for in algorithm name and description</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of matching algorithms</returns>
    Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null);

    /// <summary>
    /// Gets algorithms by tags
    /// </summary>
    /// <param name="tags">Collection of tags to search for</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of algorithms matching the tags</returns>
    Task<IEnumerable<AlgorithmModel>> GetByTagsAsync(IEnumerable<string> tags, Guid? userId = null);

    /// <summary>
    /// Creates a new algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm to create</param>
    /// <returns>Created algorithm</returns>
    Task<AlgorithmModel> CreateAsync(AlgorithmModel algorithm);

    /// <summary>
    /// Updates an existing algorithm
    /// </summary>
    /// <param name="algorithm">Algorithm with updates</param>
    /// <returns>Updated algorithm</returns>
    Task<AlgorithmModel> UpdateAsync(AlgorithmModel algorithm);

    /// <summary>
    /// Soft deletes an algorithm
    /// </summary>
    /// <param name="id">The algorithm's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets algorithms by status
    /// </summary>
    /// <param name="status">Algorithm status to filter by</param>
    /// <param name="userId">Optional user ID to filter by user's algorithms</param>
    /// <returns>Collection of algorithms with the specified status</returns>
    Task<IEnumerable<AlgorithmModel>> GetByStatusAsync(AlgorithmStatus status, Guid? userId = null);

    /// <summary>
    /// Gets all algorithm templates
    /// </summary>
    /// <returns>Collection of algorithm templates</returns>
    Task<IEnumerable<AlgorithmModel>> GetTemplatesAsync();

    /// <summary>
    /// Gets the count of algorithms for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Number of algorithms owned by the user</returns>
    Task<long> CountByUserIdAsync(Guid userId);
}