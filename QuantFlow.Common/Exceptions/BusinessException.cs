namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule violation occurs
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the BusinessException class
    /// </summary>
    public BusinessException() : base("A business rule violation occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public BusinessException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessException class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}