namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when data validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors that occurred
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    public ValidationException() : base("One or more validation errors occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ValidationException(string message) : base(message)
    {
        Errors.Add(message);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with validation errors
    /// </summary>
    /// <param name="errors">The validation errors that occurred</param>
    public ValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred.")
    {
        Errors.AddRange(errors);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors.Add(message);
    }
}