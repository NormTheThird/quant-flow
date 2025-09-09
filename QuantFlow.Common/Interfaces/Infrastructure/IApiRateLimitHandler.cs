namespace QuantFlow.Common.Interfaces.Infrastructure;

/// <summary>
/// Interface for API rate limit handling - enables testing and future flexibility
/// </summary>
public interface IApiRateLimitHandler
{
    /// <summary>
    /// Executes an async operation with rate limit handling
    /// </summary>
    Task<T> ExecuteWithRateLimitHandlingAsync<T>(Func<Task<T>> apiCall, string operationName, CancellationToken cancellationToken = default);
}