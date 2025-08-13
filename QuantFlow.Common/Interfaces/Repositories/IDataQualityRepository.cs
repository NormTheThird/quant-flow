namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for data quality metrics using business models
/// </summary>
public interface IDataQualityRepository
{
    /// <summary>
    /// Writes data quality metrics to the time-series database
    /// </summary>
    /// <param name="qualityMetrics">Data quality metrics to store</param>
    Task WriteDataQualityMetricsAsync(DataQualityModel qualityMetrics);

    /// <summary>
    /// Writes multiple data quality metrics in a batch operation
    /// </summary>
    /// <param name="qualityMetricsList">Collection of data quality metrics to store</param>
    Task WriteDataQualityMetricsBatchAsync(IEnumerable<DataQualityModel> qualityMetricsList);

    /// <summary>
    /// Gets data quality metrics for a symbol within a time range
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <param name="qualityCheck">Optional quality check type filter</param>
    /// <returns>Collection of data quality metrics</returns>
    Task<IEnumerable<DataQualityModel>> GetDataQualityMetricsAsync(
        string symbol,
        DateTime start,
        DateTime end,
        string? exchange = null,
        string? qualityCheck = null);

    /// <summary>
    /// Gets the latest data quality metrics for a symbol
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <param name="qualityCheck">Optional quality check type filter</param>
    /// <returns>Latest data quality metrics or null</returns>
    Task<DataQualityModel?> GetLatestDataQualityAsync(
        string symbol,
        string? exchange = null,
        string? qualityCheck = null);

    /// <summary>
    /// Gets data quality summary for multiple symbols
    /// </summary>
    /// <param name="symbols">Collection of symbols to analyze</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <returns>Collection of data quality summaries</returns>
    Task<IEnumerable<DataQualitySummaryModel>> GetDataQualitySummaryAsync(
        IEnumerable<string> symbols,
        DateTime start,
        DateTime end,
        string? exchange = null);
}