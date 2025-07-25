namespace QuantFlow.Data.InfluxDB.HealthChecks;

/// <summary>
/// Health check for InfluxDB connectivity
/// </summary>
public class InfluxDbHealthCheck : IHealthCheck
{
    private readonly InfluxDbContext _context;
    private readonly ILogger<InfluxDbHealthCheck> _logger;

    public InfluxDbHealthCheck(InfluxDbContext context, ILogger<InfluxDbHealthCheck> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _context.IsHealthyAsync();

            if (isHealthy)
            {
                _logger.LogDebug("InfluxDB health check passed");
                return HealthCheckResult.Healthy("InfluxDB is healthy", new Dictionary<string, object>
                {
                    ["bucket"] = _context.Bucket,
                    ["organization"] = _context.Organization,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("InfluxDB health check failed - service unavailable");
                return HealthCheckResult.Unhealthy("InfluxDB is not healthy", null, new Dictionary<string, object>
                {
                    ["bucket"] = _context.Bucket,
                    ["organization"] = _context.Organization,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InfluxDB health check failed with exception");
            return HealthCheckResult.Unhealthy("InfluxDB health check failed", ex, new Dictionary<string, object>
            {
                ["bucket"] = _context.Bucket,
                ["organization"] = _context.Organization,
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });
        }
    }
}