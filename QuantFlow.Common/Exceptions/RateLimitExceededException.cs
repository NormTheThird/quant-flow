namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when rate limits are persistently exceeded
/// </summary>
public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message) { }
    public RateLimitExceededException(string message, Exception innerException) : base(message, innerException) { }
}