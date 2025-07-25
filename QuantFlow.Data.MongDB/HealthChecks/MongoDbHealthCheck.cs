namespace QuantFlow.Data.MongoDB.HealthChecks;

/// <summary>
/// Custom health check for MongoDB connectivity
/// </summary>
public class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoDbHealthCheck> _logger;

    public MongoDbHealthCheck(MongoDbContext context, ILogger<MongoDbHealthCheck> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing MongoDB health check");

            // Use the IsHealthyAsync method from MongoDbContext
            var isHealthy = await _context.IsHealthyAsync();

            if (isHealthy)
            {
                _logger.LogDebug("MongoDB health check passed");
                return HealthCheckResult.Healthy("MongoDB connection is healthy");
            }
            else
            {
                _logger.LogWarning("MongoDB health check failed - connection unhealthy");
                return HealthCheckResult.Unhealthy("MongoDB connection is not healthy");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("MongoDB health check was cancelled");
            return HealthCheckResult.Unhealthy("MongoDB health check was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed with exception");
            return HealthCheckResult.Unhealthy($"MongoDB health check failed: {ex.Message}", ex);
        }
    }
}