namespace QuantFlow.Data.SQLServer.Interceptors;

/// <summary>
/// Interceptor that automatically clears the change tracker when SaveChanges fails
/// </summary>
public class ChangeTrackerClearingInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<ChangeTrackerClearingInterceptor> _logger;

    public ChangeTrackerClearingInterceptor(ILogger<ChangeTrackerClearingInterceptor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context != null)
        {
            _logger.LogWarning("SaveChanges failed, clearing change tracker");
            eventData.Context.ChangeTracker.Clear();
        }
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            _logger.LogWarning("SaveChangesAsync failed, clearing change tracker");
            eventData.Context.ChangeTracker.Clear();
        }
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}