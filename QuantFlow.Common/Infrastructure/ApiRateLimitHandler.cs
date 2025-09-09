namespace QuantFlow.Common.Infrastructure;

/// <summary>
/// Simple rate limit handler - just the essentials
/// </summary>
public class ApiRateLimitHandler : IApiRateLimitHandler
{
    private readonly ILogger<ApiRateLimitHandler> _logger;
    private readonly RateLimitSettings _settings;
    private DateTime _lastApiCall = DateTime.MinValue;
    private readonly object _lock = new object();

    public ApiRateLimitHandler(ILogger<ApiRateLimitHandler> logger, IOptions<RateLimitSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Executes an API call with rate limit handling
    /// </summary>
    public async Task<T> ExecuteWithRateLimitHandlingAsync<T>(Func<Task<T>> apiCall, string operationName, CancellationToken cancellationToken = default)
    {
        var attempt = 0;

        while (attempt < _settings.MaxRetries)
        {
            try
            {
                // Enforce minimum delay between calls
                await EnforceMinimumInterval(cancellationToken);

                _logger.LogDebug("Executing {Operation} (attempt {Attempt})", operationName, attempt + 1);

                var result = await apiCall();

                // Update last call time on success
                lock (_lock) { _lastApiCall = DateTime.UtcNow; }

                if (attempt > 0)
                {
                    _logger.LogInformation("✅ {Operation} succeeded after {Attempts} attempts", operationName, attempt + 1);
                }

                return result;
            }
            catch (Exception ex) when (IsRateLimitException(ex))
            {
                attempt++;
                if (attempt >= _settings.MaxRetries)
                {
                    _logger.LogError("❌ {Operation} failed after {Attempts} attempts due to rate limiting", operationName, attempt);
                    throw new RateLimitExceededException($"Rate limit exceeded for {operationName}", ex);
                }

                // Simple exponential backoff for rate limits
                var delay = TimeSpan.FromMilliseconds(_settings.RateLimitDelayMs * Math.Pow(2, attempt - 1));
                var maxDelay = TimeSpan.FromMilliseconds(_settings.MaxDelayMs);
                if (delay > maxDelay) delay = maxDelay;

                _logger.LogWarning("⚠️ Rate limit hit for {Operation}. Waiting {Delay} before retry", operationName, delay);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex) when (IsRetryableException(ex))
            {
                attempt++;
                if (attempt >= _settings.MaxRetries)
                {
                    _logger.LogError(ex, "❌ {Operation} failed after {Attempts} attempts", operationName, attempt);
                    throw;
                }

                // Simple exponential backoff for other errors
                var delay = TimeSpan.FromMilliseconds(_settings.BaseDelayMs * Math.Pow(2, attempt - 1));
                _logger.LogWarning("⚠️ Retryable error for {Operation}. Waiting {Delay} before retry", operationName, delay);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                // Non-retryable - fail immediately
                _logger.LogError(ex, "❌ Non-retryable error for {Operation}", operationName);
                throw;
            }
        }

        throw new InvalidOperationException($"Should not reach here - {operationName}");
    }

    private async Task EnforceMinimumInterval(CancellationToken cancellationToken)
    {
        TimeSpan? delayNeeded = null;

        // Calculate delay inside lock, but don't await inside lock
        lock (_lock)
        {
            if (_lastApiCall != DateTime.MinValue)
            {
                var timeSinceLastCall = DateTime.UtcNow - _lastApiCall;
                var minimumInterval = TimeSpan.FromMilliseconds(_settings.MinIntervalBetweenCallsMs);

                if (timeSinceLastCall < minimumInterval)
                {
                    delayNeeded = minimumInterval - timeSinceLastCall;
                }
            }
        }

        // Await outside the lock
        if (delayNeeded.HasValue)
        {
            _logger.LogDebug("Enforcing {Delay}ms delay between API calls", delayNeeded.Value.TotalMilliseconds);
            await Task.Delay(delayNeeded.Value, cancellationToken);
        }
    }

    private static bool IsRateLimitException(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("rate limit") ||
               message.Contains("too many requests") ||
               message.Contains("quota exceeded") ||
               message.Contains("429");
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is TaskCanceledException ||
               ex is SocketException ||
               ex is TimeoutException;
    }
}