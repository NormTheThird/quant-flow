namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for algorithm performance metrics using business models
/// </summary>
public interface IAlgorithmPerformanceRepository
{
    /// <summary>
    /// Writes algorithm performance metrics to the time-series database
    /// </summary>
    /// <param name="performance">Performance metrics to store</param>
    Task WritePerformanceMetricsAsync(AlgorithmPerformanceModel performance);

    /// <summary>
    /// Writes multiple performance metrics in a batch operation
    /// </summary>
    /// <param name="performanceList">Collection of performance metrics to store</param>
    Task WritePerformanceMetricsBatchAsync(IEnumerable<AlgorithmPerformanceModel> performanceList);

    /// <summary>
    /// Gets performance metrics for an algorithm within a time range
    /// </summary>
    /// <param name="algorithmId">Algorithm unique identifier</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <param name="environment">Optional environment filter (dev, test, prod)</param>
    /// <returns>Collection of performance metrics</returns>
    Task<IEnumerable<AlgorithmPerformanceModel>> GetPerformanceMetricsAsync(
        Guid algorithmId,
        DateTime start,
        DateTime end,
        string? symbol = null,
        string? environment = null);

    /// <summary>
    /// Gets the latest performance metrics for an algorithm
    /// </summary>
    /// <param name="algorithmId">Algorithm unique identifier</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <param name="environment">Optional environment filter</param>
    /// <returns>Latest performance metrics or null</returns>
    Task<AlgorithmPerformanceModel?> GetLatestPerformanceAsync(
        Guid algorithmId,
        string? symbol = null,
        string? environment = null);

    /// <summary>
    /// Gets performance summary statistics for an algorithm
    /// </summary>
    /// <param name="algorithmId">Algorithm unique identifier</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="symbol">Optional symbol filter</param>
    /// <returns>Performance summary or null</returns>
    Task<PerformanceSummaryModel?> GetPerformanceSummaryAsync(
        Guid algorithmId,
        DateTime start,
        DateTime end,
        string? symbol = null);
}