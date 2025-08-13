namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseController(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates and parses an exchange string to Exchange enum
    /// </summary>
    /// <param name="exchangeString">Exchange string to validate</param>
    /// <returns>Parsed Exchange enum</returns>
    /// <exception cref="ValidationException">Thrown when exchange is invalid</exception>
    protected static Exchange ValidateAndParseExchange(string exchangeString)
    {
        if (TryParseExchange(exchangeString, out var exchange))
            return exchange;

        var supportedExchanges = GetSupportedExchanges();
        throw new ApiValidationException("exchange", exchangeString, supportedExchanges);
    }

    /// <summary>
    /// Validates and parses a timeframe string to Timeframe enum
    /// </summary>
    /// <param name="timeframeString">Timeframe string to validate</param>
    /// <returns>Parsed Timeframe enum</returns>
    /// <exception cref="ValidationException">Thrown when timeframe is invalid</exception>
    protected static Timeframe ValidateAndParseTimeframe(string timeframeString)
    {
        if (TryParseTimeframe(timeframeString, out var timeframe))
            return timeframe;

        var supportedTimeframes = GetSupportedTimeframes();
        throw new ApiValidationException("timeframe", timeframeString, supportedTimeframes);
    }

    /// <summary>
    /// Creates a BadRequest response from a ValidationException
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="ex">ValidationException to convert</param>
    /// <returns>BadRequest ActionResult with ApiResponse</returns>
    protected BadRequestObjectResult CreateValidationErrorResponse<T>(ApiValidationException ex)
    {
        // If there are multiple errors, combine them
        var message = ex.Errors.Count > 1
            ? $"Validation failed:\n• {string.Join("\n• ", ex.Errors)}"
            : ex.Message;

        return BadRequest(new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default
        });
    }

    /// <summary>
    /// Creates a successful API response with data
    /// </summary>
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Request completed successfully")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        });
    }

    /// <summary>
    /// Creates a successful API response without data
    /// </summary>
    protected ActionResult<ApiResponse> Success(string message = "Request completed successfully")
    {
        return Ok(new ApiResponse
        {
            Success = true,
            Message = message
        });
    }

    /// <summary>
    /// Creates an error API response
    /// </summary>
    protected ActionResult<ApiResponse> Error(string message, List<string>? errors = null, int statusCode = 400)
    {
        var response = new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? []
        };

        return statusCode switch
        {
            400 => BadRequest(response),
            401 => Unauthorized(response),
            403 => StatusCode(403, response),
            404 => NotFound(response),
            409 => Conflict(response),
            500 => StatusCode(500, response),
            _ => BadRequest(response)
        };
    }

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    protected void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }




    private static bool TryParseExchange(string exchangeString, out Exchange exchange)
    {
        return Enum.TryParse<Exchange>(exchangeString, true, out exchange) &&
               exchange != Exchange.Unknown;
    }

    private static bool TryParseTimeframe(string timeframeString, out Timeframe timeframe)
    {
        timeframe = timeframeString.ToLowerInvariant() switch
        {
            "1m" => Timeframe.OneMinute,
            "5m" => Timeframe.FiveMinutes,
            "15m" => Timeframe.FifteenMinutes,
            "30m" => Timeframe.ThirtyMinutes,
            "1h" => Timeframe.OneHour,
            "4h" => Timeframe.FourHours,
            "1d" => Timeframe.OneDay,
            "1w" => Timeframe.OneWeek,
            _ => Timeframe.Unknown
        };

        return timeframe != Timeframe.Unknown;
    }

    private static string GetSupportedExchanges()
    {
        var exchanges = Enum.GetNames<Exchange>()
            .Where(e => e != nameof(Exchange.Unknown))
            .Select(e => e.ToLowerInvariant());

        return string.Join(", ", exchanges);
    }

    private static string GetSupportedTimeframes()
    {
        return "1m, 5m, 15m, 30m, 1h, 4h, 1d, 1w";
    }
}