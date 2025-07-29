// QuantFlow.Data.InfluxDB/Repositories/InfluxDataQualityRepository.cs
using Microsoft.Extensions.Logging;
using QuantFlow.Domain.Interfaces.Repositories;
using QuantFlow.Common.Models;
using QuantFlow.Data.InfluxDB.Context;
using QuantFlow.Data.InfluxDB.Models;

namespace QuantFlow.Data.InfluxDB.Repositories;

/// <summary>
/// InfluxDB implementation of data quality repository
/// </summary>
public class DataQualityRepository : IDataQualityRepository
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<DataQualityRepository> _logger;

    public DataQualityRepository(
        InfluxDbContext context,
        ILogger<DataQualityRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Writes data quality metrics to InfluxDB
    /// </summary>
    public async Task WriteDataQualityMetricsAsync(DataQualityModel qualityMetrics)
    {
        ArgumentNullException.ThrowIfNull(qualityMetrics);

        try
        {
            var qualityPoint = new DataQualityPoint
            {
                Symbol = qualityMetrics.Symbol,
                Exchange = qualityMetrics.Exchange,
                Timeframe = qualityMetrics.Timeframe,
                DataSource = qualityMetrics.DataSource,
                QualityCheck = qualityMetrics.QualityCheck,
                QualityScore = qualityMetrics.QualityScore,
                ExpectedRecords = qualityMetrics.ExpectedRecords,
                ActualRecords = qualityMetrics.ActualRecords,
                MissingRecords = qualityMetrics.MissingRecords,
                DuplicateRecords = qualityMetrics.DuplicateRecords,
                InvalidRecords = qualityMetrics.InvalidRecords,
                DataGaps = qualityMetrics.DataGaps,
                MaxGapMinutes = qualityMetrics.MaxGapMinutes,
                AvgLatencyMs = qualityMetrics.AvgLatencyMs,
                MaxLatencyMs = qualityMetrics.MaxLatencyMs,
                DataFreshnessMinutes = qualityMetrics.DataFreshnessMinutes,
                ErrorCount = qualityMetrics.ErrorCount,
                WarningCount = qualityMetrics.WarningCount,
                Timestamp = qualityMetrics.Timestamp
            };

            await _context.WritePointAsync(qualityPoint);
            _logger.LogDebug("Written data quality metrics for {Symbol}/{Exchange} at {Timestamp}",
                qualityMetrics.Symbol, qualityMetrics.Exchange, qualityMetrics.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write data quality metrics for {Symbol}/{Exchange}",
                qualityMetrics.Symbol, qualityMetrics.Exchange);
            throw;
        }
    }

    /// <summary>
    /// Writes multiple data quality metrics to InfluxDB
    /// </summary>
    public async Task WriteDataQualityMetricsBatchAsync(IEnumerable<DataQualityModel> qualityMetricsList)
    {
        ArgumentNullException.ThrowIfNull(qualityMetricsList);

        var dataList = qualityMetricsList.ToList();
        if (!dataList.Any())
        {
            _logger.LogDebug("No data quality metrics to write");
            return;
        }

        try
        {
            var qualityPoints = dataList.Select(q => new DataQualityPoint
            {
                Symbol = q.Symbol,
                Exchange = q.Exchange,
                Timeframe = q.Timeframe,
                DataSource = q.DataSource,
                QualityCheck = q.QualityCheck,
                QualityScore = q.QualityScore,
                ExpectedRecords = q.ExpectedRecords,
                ActualRecords = q.ActualRecords,
                MissingRecords = q.MissingRecords,
                DuplicateRecords = q.DuplicateRecords,
                InvalidRecords = q.InvalidRecords,
                DataGaps = q.DataGaps,
                MaxGapMinutes = q.MaxGapMinutes,
                AvgLatencyMs = q.AvgLatencyMs,
                MaxLatencyMs = q.MaxLatencyMs,
                DataFreshnessMinutes = q.DataFreshnessMinutes,
                ErrorCount = q.ErrorCount,
                WarningCount = q.WarningCount,
                Timestamp = q.Timestamp
            });

            await _context.WritePointsAsync(qualityPoints);
            _logger.LogDebug("Written {Count} data quality metrics", dataList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch of {Count} data quality metrics", dataList.Count);
            throw;
        }
    }

    /// <summary>
    /// Gets data quality metrics for a symbol within a time range
    /// </summary>
    public async Task<IEnumerable<DataQualityModel>> GetDataQualityMetricsAsync(
        string symbol,
        DateTime start,
        DateTime end,
        string? exchange = null,
        string? qualityCheck = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";
            var qualityCheckFilter = !string.IsNullOrEmpty(qualityCheck) ?
                $"and r.quality_check == \"{qualityCheck}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""data_quality"")
                |> filter(fn: (r) => r.symbol == ""{symbol}"")
                {exchangeFilter}
                {qualityCheckFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""])";

            var results = await _context.QueryAsync<DataQualityPoint>(fluxQuery);

            var qualityMetrics = results.Select(q => new DataQualityModel
            {
                Symbol = q.Symbol,
                Exchange = q.Exchange,
                Timeframe = q.Timeframe,
                DataSource = q.DataSource,
                QualityCheck = q.QualityCheck,
                QualityScore = q.QualityScore,
                ExpectedRecords = q.ExpectedRecords,
                ActualRecords = q.ActualRecords,
                MissingRecords = q.MissingRecords,
                DuplicateRecords = q.DuplicateRecords,
                InvalidRecords = q.InvalidRecords,
                DataGaps = q.DataGaps,
                MaxGapMinutes = q.MaxGapMinutes,
                AvgLatencyMs = q.AvgLatencyMs,
                MaxLatencyMs = q.MaxLatencyMs,
                DataFreshnessMinutes = q.DataFreshnessMinutes,
                ErrorCount = q.ErrorCount,
                WarningCount = q.WarningCount,
                Timestamp = q.Timestamp
            });

            _logger.LogDebug("Retrieved {Count} data quality metrics for {Symbol}", results.Count(), symbol);
            return qualityMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data quality metrics for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets latest data quality metrics for a symbol
    /// </summary>
    public async Task<DataQualityModel?> GetLatestDataQualityAsync(
        string symbol,
        string? exchange = null,
        string? qualityCheck = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";
            var qualityCheckFilter = !string.IsNullOrEmpty(qualityCheck) ?
                $"and r.quality_check == \"{qualityCheck}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: -24h)
                |> filter(fn: (r) => r._measurement == ""data_quality"")
                |> filter(fn: (r) => r.symbol == ""{symbol}"")
                {exchangeFilter}
                {qualityCheckFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""], desc: true)
                |> limit(n: 1)";

            var results = await _context.QueryAsync<DataQualityPoint>(fluxQuery);
            var latestQuality = results.FirstOrDefault();

            if (latestQuality == null)
            {
                _logger.LogDebug("No latest data quality found for {Symbol}", symbol);
                return null;
            }

            var qualityModel = new DataQualityModel
            {
                Symbol = latestQuality.Symbol,
                Exchange = latestQuality.Exchange,
                Timeframe = latestQuality.Timeframe,
                DataSource = latestQuality.DataSource,
                QualityCheck = latestQuality.QualityCheck,
                QualityScore = latestQuality.QualityScore,
                ExpectedRecords = latestQuality.ExpectedRecords,
                ActualRecords = latestQuality.ActualRecords,
                MissingRecords = latestQuality.MissingRecords,
                DuplicateRecords = latestQuality.DuplicateRecords,
                InvalidRecords = latestQuality.InvalidRecords,
                DataGaps = latestQuality.DataGaps,
                MaxGapMinutes = latestQuality.MaxGapMinutes,
                AvgLatencyMs = latestQuality.AvgLatencyMs,
                MaxLatencyMs = latestQuality.MaxLatencyMs,
                DataFreshnessMinutes = latestQuality.DataFreshnessMinutes,
                ErrorCount = latestQuality.ErrorCount,
                WarningCount = latestQuality.WarningCount,
                Timestamp = latestQuality.Timestamp
            };

            _logger.LogDebug("Retrieved latest data quality for {Symbol} at {Timestamp}",                symbol, latestQuality.Timestamp);
            return qualityModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest data quality for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets data quality summary for multiple symbols
    /// </summary>
    public async Task<IEnumerable<DataQualitySummaryModel>> GetDataQualitySummaryAsync(
        IEnumerable<string> symbols,
        DateTime start,
        DateTime end,
        string? exchange = null)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        var symbolsList = symbols.ToList();
        if (!symbolsList.Any())
        {
            return Enumerable.Empty<DataQualitySummaryModel>();
        }

        try
        {
            var symbolsFilter = string.Join(" or ", symbolsList.Select(s => $"r.symbol == \"{s}\""));
            var exchangeFilter = !string.IsNullOrEmpty(exchange) ?
                $"and r.exchange == \"{exchange}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""data_quality"")
                |> filter(fn: (r) => {symbolsFilter})
                {exchangeFilter}
                |> filter(fn: (r) => r._field == ""quality_score"" or r._field == ""missing_records"" or r._field == ""error_count"")
                |> group(columns: [""symbol"", ""_field""])
                |> mean()
                |> pivot(rowKey:[""symbol""], columnKey: [""_field""], valueColumn: ""_value"")
                |> yield(name: ""summary"")";

            var results = await _context.QueryRawAsync(fluxQuery);
            var summaries = new List<DataQualitySummaryModel>();

            if (results.Any())
            {
                foreach (var table in results)
                {
                    foreach (var record in table.Records)
                    {
                        var symbol = record.GetValueByKey("symbol")?.ToString() ?? "";

                        var summary = new DataQualitySummaryModel
                        {
                            Symbol = symbol,
                            Exchange = exchange ?? "ALL",
                            StartDate = start,
                            EndDate = end
                        };

                        // Extract values from the record
                        if (record.Values.TryGetValue("quality_score", out var qualityScore) &&
                            decimal.TryParse(qualityScore?.ToString(), out var score))
                        {
                            summary.AvgQualityScore = score;
                        }

                        if (record.Values.TryGetValue("missing_records", out var missingRecords) &&
                            int.TryParse(missingRecords?.ToString(), out var missing))
                        {
                            summary.TotalMissingRecords = missing;
                        }

                        if (record.Values.TryGetValue("error_count", out var errorCount) &&
                            int.TryParse(errorCount?.ToString(), out var errors))
                        {
                            summary.TotalErrorCount = errors;
                        }

                        summaries.Add(summary);
                    }
                }
            }

            _logger.LogDebug("Retrieved data quality summary for {Count} symbols", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data quality summary for symbols");
            throw;
        }
    }
}