namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for system metrics operations using business models
/// </summary>
public interface ISystemMetricsRepository
{
    /// <summary>
    /// Writes system metrics to the time-series database
    /// </summary>
    /// <param name="metrics">System metrics to store</param>
    Task WriteSystemMetricsAsync(SystemMetricsModel metrics);

    /// <summary>
    /// Writes multiple system metrics in a batch operation
    /// </summary>
    /// <param name="metricsList">Collection of system metrics to store</param>
    Task WriteSystemMetricsBatchAsync(IEnumerable<SystemMetricsModel> metricsList);

    /// <summary>
    /// Gets system metrics for a host/service within a time range
    /// </summary>
    /// <param name="host">Host name</param>
    /// <param name="service">Service name</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="environment">Optional environment filter</param>
    /// <returns>Collection of system metrics</returns>
    Task<IEnumerable<SystemMetricsModel>> GetSystemMetricsAsync(
        string host,
        string service,
        DateTime start,
        DateTime end,
        string? environment = null);

    /// <summary>
    /// Gets the latest system metrics for a host/service
    /// </summary>
    /// <param name="host">Host name</param>
    /// <param name="service">Service name</param>
    /// <param name="environment">Optional environment filter</param>
    /// <returns>Latest system metrics or null</returns>
    Task<SystemMetricsModel?> GetLatestSystemMetricsAsync(
        string host,
        string service,
        string? environment = null);

    /// <summary>
    /// Gets aggregated system metrics summary over a time period
    /// </summary>
    /// <param name="host">Host name</param>
    /// <param name="service">Service name</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="aggregationWindow">Aggregation window (e.g., "1h", "1d")</param>
    /// <returns>System metrics summary or null</returns>
    Task<SystemMetricsSummaryModel?> GetSystemMetricsSummaryAsync(
        string host,
        string service,
        DateTime start,
        DateTime end,
        string aggregationWindow = "1h");
}