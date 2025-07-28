namespace QuantFlow.Data.InfluxDB.Context;

/// <summary>
/// InfluxDB context for time-series operations
/// </summary>
public class InfluxDbContext : IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly ILogger<InfluxDbContext> _logger;
    private readonly string _bucket;
    private readonly string _organization;
    private bool _disposed = false;

    public InfluxDbContext(InfluxDBClient client, string bucket, string organization, ILogger<InfluxDbContext> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        _organization = organization ?? throw new ArgumentNullException(nameof(organization));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the write API for writing data to InfluxDB
    /// </summary>
    public WriteApi GetWriteApi()
    {
        ThrowIfDisposed();
        return _client.GetWriteApi();
    }

    /// <summary>
    /// Gets the query API for reading data from InfluxDB
    /// </summary>
    public QueryApi GetQueryApi()
    {
        ThrowIfDisposed();
        return _client.GetQueryApi();
    }

    /// <summary>
    /// Gets the delete API for deleting data from InfluxDB
    /// </summary>
    public DeleteApi GetDeleteApi()
    {
        ThrowIfDisposed();
        return _client.GetDeleteApi();
    }

    /// <summary>
    /// Writes a single point to InfluxDB
    /// </summary>
    public async Task WritePointAsync<T>(T point) where T : BaseTimeSeriesPoint
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(point);

        try
        {
            using var writeApi = GetWriteApi();
            writeApi.WriteMeasurement(point, WritePrecision.Ns, _bucket, _organization);
            // WriteApi is disposed here, which flushes the data

            _logger.LogDebug("Written single point of type {Type} to bucket {Bucket}",
                typeof(T).Name, _bucket);

            // Yield control to allow other async operations to proceed
            await Task.Yield();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write point of type {Type} to InfluxDB", typeof(T).Name);
            throw;
        }
    }

    ///// <summary>
    ///// Writes multiple points to InfluxDB
    ///// </summary>
    //public async Task WritePointsAsync<T>(IEnumerable<T> points) where T : BaseTimeSeriesPoint
    //{
    //    ThrowIfDisposed();
    //    ArgumentNullException.ThrowIfNull(points);

    //    var pointsList = points.ToList();
    //    if (!pointsList.Any())
    //    {
    //        _logger.LogDebug("No points to write");
    //        return;
    //    }

    //    try
    //    {
    //        using var writeApi = GetWriteApi();
    //        writeApi.WriteMeasurements(pointsList, WritePrecision.Ns, _bucket, _organization);
    //        // WriteApi is disposed here, which flushes the data

    //        _logger.LogDebug("Written {Count} points of type {Type} to bucket {Bucket}",
    //            pointsList.Count, typeof(T).Name, _bucket);

    //        // Yield control to allow other async operations to proceed
    //        await Task.Yield();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to write {Count} points of type {Type} to InfluxDB",
    //            pointsList.Count, typeof(T).Name);
    //        throw;
    //    }
    //}
    public async Task WritePointsAsync<T>(IEnumerable<T> points) where T : BaseTimeSeriesPoint
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(points);

        var pointsList = points.ToList();
        if (!pointsList.Any())
        {
            _logger.LogDebug("No points to write");
            return;
        }

        try
        {
            var writeApiAsync = _client.GetWriteApiAsync();
            await writeApiAsync.WriteMeasurementsAsync(pointsList, WritePrecision.Ns, _bucket, _organization);

            _logger.LogDebug("✅ Written {Count} points of type {Type} to bucket {Bucket}",
                pointsList.Count, typeof(T).Name, _bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to write {Count} points of type {Type} to InfluxDB",
                pointsList.Count, typeof(T).Name);
            throw;
        }
    }


    /// <summary>
    /// Executes a Flux query and returns results
    /// </summary>
    public async Task<List<T>> QueryAsync<T>(string fluxQuery) where T : BaseTimeSeriesPoint
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(fluxQuery);

        try
        {
            var queryApi = GetQueryApi();
            var results = await queryApi.QueryAsync<T>(fluxQuery, _organization);

            _logger.LogDebug("Executed query returning {Count} results of type {Type}",
                results.Count, typeof(T).Name);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query: {Query}", fluxQuery);
            throw;
        }
    }

    /// <summary>
    /// Executes a Flux query and returns raw table results
    /// </summary>
    public async Task<List<FluxTable>> QueryRawAsync(string fluxQuery)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(fluxQuery);

        try
        {
            var queryApi = GetQueryApi();
            var results = await queryApi.QueryAsync(fluxQuery, _organization);

            _logger.LogDebug("Executed raw query returning {Count} tables", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute raw query: {Query}", fluxQuery);
            throw;
        }
    }

    /// <summary>
    /// Deletes data from InfluxDB within a time range
    /// </summary>
    public async Task DeleteDataAsync(string measurement, DateTime start, DateTime stop, string? predicate = null)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(measurement);

        _logger.LogWarning("Deleting data from measurement {Measurement} from {Start} to {Stop}",
            measurement, start, stop);

        try
        {
            var deleteApi = GetDeleteApi();
            var predicateStr = string.IsNullOrEmpty(predicate) ?
                $"_measurement=\"{measurement}\"" :
                $"_measurement=\"{measurement}\" AND {predicate}";

            await deleteApi.Delete(start, stop, predicateStr, _bucket, _organization);

            _logger.LogInformation("Successfully deleted data from measurement {Measurement}", measurement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete data from measurement {Measurement}", measurement);
            throw;
        }
    }

    /// <summary>
    /// Checks if the InfluxDB connection is healthy
    /// </summary>
    /// <returns>True if connection is healthy</returns>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            ThrowIfDisposed();

            // Ping the InfluxDB server to check connectivity
            var isHealthy = await _client.PingAsync();

            _logger.LogDebug("InfluxDB ping check: {Status}", isHealthy ? "Success" : "Failed");
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InfluxDB ping check failed");
            return false;
        }
    }

    public string Bucket => _bucket;
    public string Organization => _organization;

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InfluxDbContext));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _client?.Dispose();
            _disposed = true;
            _logger.LogDebug("InfluxDbContext disposed");
        }
    }
}