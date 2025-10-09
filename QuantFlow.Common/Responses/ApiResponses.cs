namespace QuantFlow.Common.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Generic API response wrapper with data payload
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; } = default;
}