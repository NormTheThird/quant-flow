namespace QuantFlow.Api.Rest.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        protected readonly ILogger<T> _logger;

        protected BaseController(ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Validation Helpers

        protected static Exchange ValidateAndParseExchange(string exchangeString)
        {
            if (TryParseExchange(exchangeString, out var exchange))
                return exchange;

            var supportedExchanges = GetSupportedExchanges();
            throw new ApiValidationException("exchange", exchangeString, supportedExchanges);
        }

        protected static Timeframe ValidateAndParseTimeframe(string timeframeString)
        {
            if (TryParseTimeframe(timeframeString, out var timeframe))
                return timeframe;

            var supportedTimeframes = GetSupportedTimeframes();
            throw new ApiValidationException("timeframe", timeframeString, supportedTimeframes);
        }

        #endregion

        #region API Response Helpers

        /// <summary>
        /// Creates a BadRequest response from a ValidationException with typed data
        /// </summary>
        protected BadRequestObjectResult CreateValidationErrorResponse<TData>(ApiValidationException ex)
        {
            var message = ex.Errors.Count > 1
                ? $"Validation failed:\n• {string.Join("\n• ", ex.Errors)}"
                : ex.Message;

            return BadRequest(new ApiResponse<TData>
            {
                Success = false,
                Message = message,
                Data = default
            });
        }

        /// <summary>
        /// Creates a successful API response with typed data
        /// </summary>
        protected ActionResult<ApiResponse<TData>> Success<TData>(TData data, string message = "Request completed successfully")
        {
            return Ok(new ApiResponse<TData>
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
        /// Creates a general error API response
        /// </summary>
        protected ActionResult<ApiResponse> Error(string message, List<string>? errors = null, int statusCode = 400)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
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

        #endregion

        #region Logging Helpers

        protected void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        protected void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        protected void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        #endregion

        #region Internal Parsing Helpers

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

        #endregion
    }
}
