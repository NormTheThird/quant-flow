namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing system performance metrics
/// </summary>
public class SystemMetricsModel
{
    public string Host { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public decimal CpuPercentage { get; set; } = 0.0m;
    public decimal MemoryUsedMb { get; set; } = 0.0m;
    public decimal MemoryTotalMb { get; set; } = 0.0m;
    public decimal MemoryPercentage { get; set; } = 0.0m;
    public decimal? DiskUsedGb { get; set; } = null;
    public decimal? DiskTotalGb { get; set; } = null;
    public decimal? DiskPercentage { get; set; } = null;
    public decimal? NetworkInMbps { get; set; } = null;
    public decimal? NetworkOutMbps { get; set; } = null;
    public int? ActiveConnections { get; set; } = null;
    public int? ThreadCount { get; set; } = null;
    public int? GcCollections { get; set; } = null;
    public decimal? GcMemoryMb { get; set; } = null;
    public int? RequestCount { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    public decimal? ResponseTimeMs { get; set; } = null;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}