namespace QuantFlow.Data.InfluxDB.Context;

/// <summary>
/// InfluxDB context for time-series operations with proper async patterns
/// </summary>
public class InfluxDbContext : IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly ILogger<InfluxDbContext> _logger;
    private readonly string _bucket;
    private readonly string _organization;
    private readonly bool _ownsClient;
    private bool _disposed = false;

    public InfluxDbContext(InfluxDBClient client, string bucket, string organization, ILogger<InfluxDbContext> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        _organization = organization ?? throw new ArgumentNullException(nameof(organization));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ownsClient = false; // DI container manages the client lifetime
    }

    public string Bucket => _bucket;
    public string Organization => _organization;

    /// <summary>
    /// Gets the synchronous write API (deprecated - use WritePointAsync instead)
    /// </summary>
    public WriteApi GetWriteApi()
    {
        ThrowIfDisposed();
        return _client.GetWriteApi();
    }

    /// <summary>
    /// Gets the query API for reading data
    /// </summary>
    public QueryApi GetQueryApi()
    {
        ThrowIfDisposed();
        return _client.GetQueryApi();
    }

    /// <summary>
    /// Gets the delete API for deleting data
    /// </summary>
    public DeleteApi GetDeleteApi()
    {
        ThrowIfDisposed();
        return _client.GetDeleteApi();
    }

    /// <summary>
    /// Writes a single point to InfluxDB using proper async pattern
    /// </summary>
    public async Task WritePointAsync<T>(T point) where T : BaseTimeSeriesPoint
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(point);

        try
        {
            var writeApiAsync = _client.GetWriteApiAsync();
            await writeApiAsync.WriteMeasurementAsync(point, WritePrecision.Ns, _bucket, _organization);

            _logger.LogDebug("✅ Written single point of type {Type} to bucket {Bucket}",
                typeof(T).Name, _bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to write point of type {Type} to InfluxDB", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Writes multiple points to InfluxDB using proper async pattern
    /// </summary>
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
    /// Executes a Flux query and returns strongly-typed results
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(string fluxQuery) where T : class
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(fluxQuery);

        try
        {
            _logger.LogDebug("Executing Flux query for type {Type}", typeof(T).Name);

            var queryApi = GetQueryApi();
            var results = await queryApi.QueryAsync<T>(fluxQuery, _organization);

            _logger.LogDebug("✅ Query returned {Count} results for type {Type}",
                results?.Count() ?? 0, typeof(T).Name);

            return results ?? Enumerable.Empty<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to execute query for type {Type}: {Query}",
                typeof(T).Name, fluxQuery);
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
            _logger.LogDebug("Executing raw Flux query");

            var queryApi = GetQueryApi();
            var results = await queryApi.QueryAsync(fluxQuery, _organization);

            _logger.LogDebug("✅ Raw query returned {Count} tables", results?.Count ?? 0);

            return results ?? new List<FluxTable>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to execute raw query: {Query}", fluxQuery);
            throw;
        }
    }

    /// <summary>
    /// Deletes data from InfluxDB based on predicate with retry logic
    /// </summary>
    public async Task DeleteDataAsync(DateTime start, DateTime stop, string predicate = "")
    {
        ThrowIfDisposed();

        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogDebug("Attempt {Attempt}: Deleting data from {Start} to {Stop} with predicate: {Predicate}",
                    retryCount + 1, start, stop, predicate);

                var deleteApi = _client.GetDeleteApi();
                await deleteApi.Delete(start, stop, predicate, _bucket, _organization);

                _logger.LogDebug("✅ Successfully deleted data on attempt {Attempt}", retryCount + 1);
                return; // Success!
            }
            catch (ObjectDisposedException ex) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                _logger.LogWarning(ex, "⚠️  Client disposed on attempt {Attempt}, retrying...", retryCount);
                await Task.Delay(TimeSpan.FromMilliseconds(500 * retryCount)); // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to delete data from {Start} to {Stop} on attempt {Attempt}",
                    start, stop, retryCount + 1);
                throw;
            }
        }

        // If we get here, all retries failed
        throw new InvalidOperationException($"Failed to delete data after {maxRetries} attempts due to client disposal");
    }



    /// <summary>
    /// Checks if the InfluxDB connection is healthy
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        ThrowIfDisposed();

        try
        {
            var pingResult = await _client.PingAsync();
            var isHealthy = pingResult;

            _logger.LogDebug("InfluxDB health check: {Status}",
                isHealthy ? "Healthy" : "Unhealthy");

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Health check failed");
            return false;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InfluxDbContext));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Only dispose the client if we own it
            if (_ownsClient)
            {
                _client?.Dispose();
            }

            _disposed = true;
            _logger.LogDebug("InfluxDbContext disposed (client owned: {ClientOwned})", _ownsClient);
        }
    }
}