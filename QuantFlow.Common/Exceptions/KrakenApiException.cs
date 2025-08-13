// QuantFlow.Common/Exceptions/KrakenApiException.cs
namespace QuantFlow.Common.Exceptions;

public class KrakenApiException : Exception
{
    public int? ErrorCode { get; }
    public string ApiErrorMessage { get; }

    // Basic constructor
    public KrakenApiException(string message) : base(message)
    {
        ApiErrorMessage = message; // Initialize with the main message
    }

    // Constructor with inner exception
    public KrakenApiException(string message, Exception innerException) : base(message, innerException)
    {
        ApiErrorMessage = message; // Initialize with the main message
    }

    // Constructor with additional error details
    public KrakenApiException(string message, int? errorCode, string? apiErrorMessage = null) : base(message)
    {
        ErrorCode = errorCode;
        ApiErrorMessage = apiErrorMessage ?? message; // Use message as fallback if apiErrorMessage is null
    }

    // Constructor with all parameters including inner exception
    public KrakenApiException(string message, int? errorCode, string? apiErrorMessage, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
        ApiErrorMessage = apiErrorMessage ?? message; // Use message as fallback if apiErrorMessage is null
    }

    // Override the ToString method for better logging
    public override string ToString()
    {
        var baseString = base.ToString();
        var errorCodePart = ErrorCode.HasValue ? $", ErrorCode: {ErrorCode}" : "";
        var apiMessagePart = !string.IsNullOrEmpty(ApiErrorMessage) ? $", ApiErrorMessage: {ApiErrorMessage}" : "";

        return $"{baseString}{errorCodePart}{apiMessagePart}";
    }
}