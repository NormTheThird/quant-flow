namespace QuantFlow.Data.InfluxDB.Models;

/// <summary>
/// InfluxDB measurement for system performance metrics
/// </summary>
[Measurement("system_metrics")]
public class SystemMetricsPoint : BaseTimeSeriesPoint
{
    // Tags
    [Column("host", IsTag = true)]
    public string Host { get; set; } = string.Empty;

    [Column("service", IsTag = true)]
    public string Service { get; set; } = string.Empty;

    [Column("instance", IsTag = true)]
    public string Instance { get; set; } = string.Empty;

    [Column("environment", IsTag = true)]
    public string Environment { get; set; } = string.Empty;

    // System Fields
    [Column("cpu_percentage")]
    public decimal CpuPercentage { get; set; } = 0.0m;

    [Column("memory_used_mb")]
    public decimal MemoryUsedMb { get; set; } = 0.0m;

    [Column("memory_total_mb")]
    public decimal MemoryTotalMb { get; set; } = 0.0m;

    [Column("memory_percentage")]
    public decimal MemoryPercentage { get; set; } = 0.0m;

    [Column("disk_used_gb")]
    public decimal? DiskUsedGb { get; set; } = null;

    [Column("disk_total_gb")]
    public decimal? DiskTotalGb { get; set; } = null;

    [Column("disk_percentage")]
    public decimal? DiskPercentage { get; set; } = null;

    [Column("network_in_mbps")]
    public decimal? NetworkInMbps { get; set; } = null;

    [Column("network_out_mbps")]
    public decimal? NetworkOutMbps { get; set; } = null;

    [Column("active_connections")]
    public int? ActiveConnections { get; set; } = null;

    [Column("thread_count")]
    public int? ThreadCount { get; set; } = null;

    [Column("gc_collections")]
    public int? GcCollections { get; set; } = null;

    [Column("gc_memory_mb")]
    public decimal? GcMemoryMb { get; set; } = null;

    [Column("request_count")]
    public int? RequestCount { get; set; } = 0;

    [Column("error_count")]
    public int ErrorCount { get; set; } = 0;

    [Column("response_time_ms")]
    public decimal? ResponseTimeMs { get; set; } = null;
}