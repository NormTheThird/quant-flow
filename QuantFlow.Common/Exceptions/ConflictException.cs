namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate key, concurrent modification)
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConflictException class
    /// </summary>
    public ConflictException() : base("A conflict occurred with the requested resource.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConflictException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConflictException class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}