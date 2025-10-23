namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service for managing and retrieving all algorithms (hard-coded and custom)
/// </summary>
public interface IAlgorithmRegistryService
{
    /// <summary>
    /// Gets all available algorithms for a user (both hard-coded and custom)
    /// </summary>
    Task<IEnumerable<AlgorithmMetadataModel>> GetAllAvailableAlgorithmsAsync(Guid userId);

    /// <summary>
    /// Gets a hard-coded algorithm instance by its ID
    /// </summary>
    Task<ITradingAlgorithm?> GetHardCodedAlgorithmAsync(Guid algorithmId);

    /// <summary>
    /// Checks if an algorithm is enabled
    /// </summary>
    Task<bool> IsAlgorithmEnabledAsync(Guid algorithmId);

    /// <summary>
    /// Gets effectiveness ratings for an algorithm
    /// </summary>
    Task<IEnumerable<AlgorithmEffectivenessModel>> GetEffectivenessAsync(Guid algorithmId);

    /// <summary>
    /// Gets effectiveness rating for specific algorithm and timeframe
    /// </summary>
    Task<AlgorithmEffectivenessModel?> GetEffectivenessAsync(Guid algorithmId, string timeframe);

    /// <summary>
    /// Gets parameter definitions for a hard-coded algorithm
    /// </summary>
    /// <param name="algorithmId">Algorithm unique identifier</param>
    /// <returns>List of parameter definitions, or empty list if not a hard-coded algorithm</returns>
    Task<List<ParameterDefinition>> GetParameterDefinitionsAsync(Guid algorithmId);
}