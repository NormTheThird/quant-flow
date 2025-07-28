namespace QuantFlow.Data.InfluxDB.Repositories;

/// <summary>
/// InfluxDB implementation of system metrics repository
/// </summary>
public class SystemMetricsRepository : ISystemMetricsRepository
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<SystemMetricsRepository> _logger;

    public SystemMetricsRepository(
        InfluxDbContext context,
        ILogger<SystemMetricsRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task WriteSystemMetricsAsync(SystemMetricsModel metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        try
        {
            var metricsPoint = new SystemMetricsPoint
            {
                Host = metrics.Host,
                Service = metrics.Service,
                Instance = metrics.Instance,
                Environment = metrics.Environment,
                CpuPercentage = metrics.CpuPercentage,
                MemoryUsedMb = metrics.MemoryUsedMb,
                MemoryTotalMb = metrics.MemoryTotalMb,
                MemoryPercentage = metrics.MemoryPercentage,
                DiskUsedGb = metrics.DiskUsedGb,
                DiskTotalGb = metrics.DiskTotalGb,
                DiskPercentage = metrics.DiskPercentage,
                NetworkInMbps = metrics.NetworkInMbps,
                NetworkOutMbps = metrics.NetworkOutMbps,
                ActiveConnections = metrics.ActiveConnections,
                ThreadCount = metrics.ThreadCount,
                GcCollections = metrics.GcCollections,
                GcMemoryMb = metrics.GcMemoryMb,
                RequestCount = metrics.RequestCount,
                ErrorCount = metrics.ErrorCount,
                ResponseTimeMs = metrics.ResponseTimeMs,
                Timestamp = metrics.Timestamp
            };

            await _context.WritePointAsync(metricsPoint);
            _logger.LogDebug("Written system metrics for {Host}/{Service} at {Timestamp}",
                metrics.Host, metrics.Service, metrics.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write system metrics for {Host}/{Service}",
                metrics.Host, metrics.Service);
            throw;
        }
    }

    public async Task WriteSystemMetricsBatchAsync(IEnumerable<SystemMetricsModel> metricsList)
    {
        ArgumentNullException.ThrowIfNull(metricsList);

        var dataList = metricsList.ToList();
        if (!dataList.Any())
        {
            _logger.LogDebug("No system metrics to write");
            return;
        }

        try
        {
            var metricsPoints = dataList.Select(m => new SystemMetricsPoint
            {
                Host = m.Host,
                Service = m.Service,
                Instance = m.Instance,
                Environment = m.Environment,
                CpuPercentage = m.CpuPercentage,
                MemoryUsedMb = m.MemoryUsedMb,
                MemoryTotalMb = m.MemoryTotalMb,
                MemoryPercentage = m.MemoryPercentage,
                DiskUsedGb = m.DiskUsedGb,
                DiskTotalGb = m.DiskTotalGb,
                DiskPercentage = m.DiskPercentage,
                NetworkInMbps = m.NetworkInMbps,
                NetworkOutMbps = m.NetworkOutMbps,
                ActiveConnections = m.ActiveConnections,
                ThreadCount = m.ThreadCount,
                GcCollections = m.GcCollections,
                GcMemoryMb = m.GcMemoryMb,
                RequestCount = m.RequestCount,
                ErrorCount = m.ErrorCount,
                ResponseTimeMs = m.ResponseTimeMs,
                Timestamp = m.Timestamp
            });

            await _context.WritePointsAsync(metricsPoints);
            _logger.LogDebug("Written {Count} system metrics", dataList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch of {Count} system metrics", dataList.Count);
            throw;
        }
    }

    public async Task<IEnumerable<SystemMetricsModel>> GetSystemMetricsAsync(
        string host,
        string service,
        DateTime start,
        DateTime end,
        string? environment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(service);

        try
        {
            var environmentFilter = !string.IsNullOrEmpty(environment) ?
                $"and r.environment == \"{environment}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""system_metrics"")
                |> filter(fn: (r) => r.host == ""{host}"")
                |> filter(fn: (r) => r.service == ""{service}"")
                {environmentFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""])";

            var results = await _context.QueryAsync<SystemMetricsPoint>(fluxQuery);

            var systemMetrics = results.Select(m => new SystemMetricsModel
            {
                Host = m.Host,
                Service = m.Service,
                Instance = m.Instance,
                Environment = m.Environment,
                CpuPercentage = m.CpuPercentage,
                MemoryUsedMb = m.MemoryUsedMb,
                MemoryTotalMb = m.MemoryTotalMb,
                MemoryPercentage = m.MemoryPercentage,
                DiskUsedGb = m.DiskUsedGb,
                DiskTotalGb = m.DiskTotalGb,
                DiskPercentage = m.DiskPercentage,
                NetworkInMbps = m.NetworkInMbps,
                NetworkOutMbps = m.NetworkOutMbps,
                ActiveConnections = m.ActiveConnections,
                ThreadCount = m.ThreadCount,
                GcCollections = m.GcCollections,
                GcMemoryMb = m.GcMemoryMb,
                RequestCount = m.RequestCount,
                ErrorCount = m.ErrorCount,
                ResponseTimeMs = m.ResponseTimeMs,
                Timestamp = m.Timestamp
            });

            _logger.LogDebug("Retrieved {Count} system metrics for {Host}/{Service}",
                results.Count, host, service);
            return systemMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system metrics for {Host}/{Service}", host, service);
            throw;
        }
    }

    public async Task<SystemMetricsModel?> GetLatestSystemMetricsAsync(
        string host,
        string service,
        string? environment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(service);

        try
        {
            var environmentFilter = !string.IsNullOrEmpty(environment) ?
                $"and r.environment == \"{environment}\"" : "";

            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: -1h)
                |> filter(fn: (r) => r._measurement == ""system_metrics"")
                |> filter(fn: (r) => r.host == ""{host}"")
                |> filter(fn: (r) => r.service == ""{service}"")
                {environmentFilter}
                |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""], desc: true)
                |> limit(n: 1)";

            var results = await _context.QueryAsync<SystemMetricsPoint>(fluxQuery);
            var latestMetrics = results.FirstOrDefault();

            if (latestMetrics == null)
            {
                _logger.LogDebug("No latest system metrics found for {Host}/{Service}", host, service);
                return null;
            }

            var metricsModel = new SystemMetricsModel
            {
                Host = latestMetrics.Host,
                Service = latestMetrics.Service,
                Instance = latestMetrics.Instance,
                Environment = latestMetrics.Environment,
                CpuPercentage = latestMetrics.CpuPercentage,
                MemoryUsedMb = latestMetrics.MemoryUsedMb,
                MemoryTotalMb = latestMetrics.MemoryTotalMb,
                MemoryPercentage = latestMetrics.MemoryPercentage,
                DiskUsedGb = latestMetrics.DiskUsedGb,
                DiskTotalGb = latestMetrics.DiskTotalGb,
                DiskPercentage = latestMetrics.DiskPercentage,
                NetworkInMbps = latestMetrics.NetworkInMbps,
                NetworkOutMbps = latestMetrics.NetworkOutMbps,
                ActiveConnections = latestMetrics.ActiveConnections,
                ThreadCount = latestMetrics.ThreadCount,
                GcCollections = latestMetrics.GcCollections,
                GcMemoryMb = latestMetrics.GcMemoryMb,
                RequestCount = latestMetrics.RequestCount,
                ErrorCount = latestMetrics.ErrorCount,
                ResponseTimeMs = latestMetrics.ResponseTimeMs,
                Timestamp = latestMetrics.Timestamp
            };

            _logger.LogDebug("Retrieved latest system metrics for {Host}/{Service} at {Timestamp}",
                host, service, latestMetrics.Timestamp);
            return metricsModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest system metrics for {Host}/{Service}", host, service);
            throw;
        }
    }

    public async Task<SystemMetricsSummaryModel?> GetSystemMetricsSummaryAsync(
        string host,
        string service,
        DateTime start,
        DateTime end,
        string aggregationWindow = "1h")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(service);

        try
        {
            var fluxQuery = $@"
                from(bucket: ""{_context.Bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""system_metrics"")
                |> filter(fn: (r) => r.host == ""{host}"")
                |> filter(fn: (r) => r.service == ""{service}"")
                |> filter(fn: (r) => r._field == ""cpu_percentage"" or r._field == ""memory_percentage"" or r._field == ""response_time_ms"")
                |> aggregateWindow(every: {aggregationWindow}, fn: mean)
                |> group(columns: [""_field""])
                |> mean()
                |> yield(name: ""summary"")";

            var results = await _context.QueryRawAsync(fluxQuery);

            if (!results.Any() || !results.First().Records.Any())
            {
                _logger.LogDebug("No system metrics summary found for {Host}/{Service}", host, service);
                return null;
            }

            var summary = new SystemMetricsSummaryModel
            {
                Host = host,
                Service = service,
                StartDate = start,
                EndDate = end,
                AggregationWindow = aggregationWindow
            };

            foreach (var table in results)
            {
                foreach (var record in table.Records)
                {
                    var field = record.GetValueByKey("_field")?.ToString();
                    var value = record.GetValue();

                    switch (field)
                    {
                        case "cpu_percentage":
                            if (decimal.TryParse(value?.ToString(), out var cpu))
                                summary.AvgCpuPercentage = cpu;
                            break;
                        case "memory_percentage":
                            if (decimal.TryParse(value?.ToString(), out var memory))
                                summary.AvgMemoryPercentage = memory;
                            break;
                        case "response_time_ms":
                            if (decimal.TryParse(value?.ToString(), out var responseTime))
                                summary.AvgResponseTimeMs = responseTime;
                            break;
                    }
                }
            }

            _logger.LogDebug("Retrieved system metrics summary for {Host}/{Service}", host, service);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system metrics summary for {Host}/{Service}", host, service);
            throw;
        }
    }
}