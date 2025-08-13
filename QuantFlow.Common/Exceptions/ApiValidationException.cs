namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when data validation fails
/// </summary>
public class ApiValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors that occurred
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Gets the parameter name that failed validation (if applicable)
    /// </summary>
    public string? ParameterName { get; }

    /// <summary>
    /// Gets the parameter value that failed validation (if applicable)
    /// </summary>
    public string? ParameterValue { get; }

    /// <summary>
    /// Gets the supported values for the parameter (if applicable)
    /// </summary>
    public string? SupportedValues { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    public ApiValidationException() : base("One or more validation errors occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ApiValidationException(string message) : base(message)
    {
        Errors.Add(message);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class for parameter validation
    /// </summary>
    /// <param name="parameterName">Name of the parameter that failed validation</param>
    /// <param name="parameterValue">Value that failed validation</param>
    /// <param name="supportedValues">Supported values for the parameter</param>
    public ApiValidationException(string parameterName, string parameterValue, string supportedValues)
        : base($"Invalid {parameterName} '{parameterValue}'. Supported values: {supportedValues}")
    {
        ParameterName = parameterName;
        ParameterValue = parameterValue;
        SupportedValues = supportedValues;
        Errors.Add(Message);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with validation errors
    /// </summary>
    /// <param name="errors">The validation errors that occurred</param>
    public ApiValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred.")
    {
        Errors.AddRange(errors);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ApiValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors.Add(message);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with parameter validation and inner exception
    /// </summary>
    /// <param name="parameterName">Name of the parameter that failed validation</param>
    /// <param name="parameterValue">Value that failed validation</param>
    /// <param name="supportedValues">Supported values for the parameter</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ApiValidationException(string parameterName, string parameterValue, string supportedValues, Exception innerException)
        : base($"Invalid {parameterName} '{parameterValue}'. Supported values: {supportedValues}", innerException)
    {
        ParameterName = parameterName;
        ParameterValue = parameterValue;
        SupportedValues = supportedValues;
        Errors.Add(Message);
    }
}