namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing aggregated system metrics summary
/// </summary>
public class SystemMetricsSummaryModel
{
    public string Host { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public string AggregationWindow { get; set; } = string.Empty;
    public decimal AvgCpuPercentage { get; set; } = 0.0m;
    public decimal MaxCpuPercentage { get; set; } = 0.0m;
    public decimal AvgMemoryPercentage { get; set; } = 0.0m;
    public decimal MaxMemoryPercentage { get; set; } = 0.0m;
    public decimal? AvgResponseTimeMs { get; set; } = null;
    public decimal? MaxResponseTimeMs { get; set; } = null;
    public int TotalRequests { get; set; } = 0;
    public int TotalErrors { get; set; } = 0;
    public decimal ErrorRate { get; set; } = 0.0m;
    public int? AvgActiveConnections { get; set; } = null;
    public int? MaxActiveConnections { get; set; } = null;
    public decimal UptimePercentage { get; set; } = 100.0m;
}