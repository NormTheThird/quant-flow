namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// API controller for market data operations - DataSource Only Version
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "ApiKeyPolicy")]
public class MarketDataController : BaseController
{
    private readonly IMarketDataService _marketDataService;

    public MarketDataController(IMarketDataService marketDataService, ILogger<MarketDataController> logger) : base(logger)
    {
        _marketDataService = marketDataService;
    }

    /// <summary>
    /// Gets historical market data for a symbol
    /// </summary>
    [HttpGet("{exchange}/{symbol}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MarketDataResponse>>>> GetHistoryAsync(string exchange, string symbol,
        [FromQuery] string timeframe = "1h", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? limit = null)
    {
        try
        {
            var exchangeEnum = ValidateAndParseExchange(exchange);
            var timeframeEnum = ValidateAndParseTimeframe(timeframe);
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var marketData = await _marketDataService.GetMarketDataAsync(exchangeEnum, symbol, timeframeEnum, start, end, limit);

            var response = marketData.Select(md => new MarketDataResponse
            {
                Symbol = md.Symbol,
                Timeframe = md.Timeframe,
                Exchange = md.Exchange,
                Open = md.Open,
                High = md.High,
                Low = md.Low,
                Close = md.Close,
                Volume = md.Volume,
                VWAP = md.VWAP,
                TradeCount = md.TradeCount,
                Timestamp = md.Timestamp
            });

            return Ok(new ApiResponse<IEnumerable<MarketDataResponse>>
            {
                Success = true,
                Message = "Market data retrieved successfully",
                Data = response
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<IEnumerable<MarketDataResponse>>(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<IEnumerable<MarketDataResponse>>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market data for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse<IEnumerable<MarketDataResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving market data",
                Data = null
            });
        }
    }

    /// <summary>
    /// Gets the latest market data point for a symbol
    /// </summary>
    [HttpGet("{exchange}/{symbol}/latest")]
    public async Task<ActionResult<ApiResponse<MarketDataResponse>>> GetLatestAsync(string exchange, string symbol, [FromQuery] string timeframe = "1h")
    {
        try
        {
            // Validate and parse parameters using base controller methods
            var exchangeEnum = ValidateAndParseExchange(exchange);
            var timeframeEnum = ValidateAndParseTimeframe(timeframe);

            var latestData = await _marketDataService.GetLatestMarketDataAsync(exchangeEnum, symbol, timeframeEnum);

            if (latestData == null)
            {
                return NotFound(new ApiResponse<MarketDataResponse>
                {
                    Success = false,
                    Message = $"No market data found for {symbol} on {exchange}",
                    Data = null
                });
            }

            var response = new MarketDataResponse
            {
                Symbol = latestData.Symbol,
                Timeframe = latestData.Timeframe,
                Exchange = latestData.Exchange,
                Open = latestData.Open,
                High = latestData.High,
                Low = latestData.Low,
                Close = latestData.Close,
                Volume = latestData.Volume,
                VWAP = latestData.VWAP,
                TradeCount = latestData.TradeCount,
                Timestamp = latestData.Timestamp
            };

            return Ok(new ApiResponse<MarketDataResponse>
            {
                Success = true,
                Message = "Latest market data retrieved successfully",
                Data = response
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<MarketDataResponse>(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest market data for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse<MarketDataResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving latest market data",
                Data = null
            });
        }
    }

    /// <summary>
    /// Gets data availability information for a symbol
    /// </summary>
    [HttpGet("{exchange}/{symbol}/availability")]
    public async Task<ActionResult<ApiResponse<DataAvailabilityResponse>>> GetAvailabilityAsync(string exchange, string symbol)
    {
        try
        {
            // Validate and parse exchange parameter
            var exchangeEnum = ValidateAndParseExchange(exchange);

            var availability = await _marketDataService.GetDataAvailabilityAsync(exchangeEnum, symbol);

            var response = new DataAvailabilityResponse
            {
                Symbol = availability.Symbol,
                Exchange = availability.Exchange,
                EarliestData = availability.EarliestDataPoint,
                LatestData = availability.LatestDataPoint,
                TotalRecords = (int)availability.TotalDataPoints,
                AvailableTimeframes = availability.TimeframeAvailability.Keys.ToList(),
                DataGaps = [] // Gaps are calculated per timeframe, would need separate endpoint for specific timeframe gaps
            };

            return Ok(new ApiResponse<DataAvailabilityResponse>
            {
                Success = true,
                Message = "Data availability retrieved successfully",
                Data = response
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<DataAvailabilityResponse>(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data availability for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse<DataAvailabilityResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving data availability",
                Data = null
            });
        }
    }

    /// <summary>
    /// Gets data gaps for a symbol within a date range
    /// </summary>
    [HttpGet("{exchange}/{symbol}/gaps")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GapInfoResponse>>>> GetDataGapsAsync(string exchange, string symbol,
        [FromQuery] string timeframe = "1h", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var exchangeEnum = ValidateAndParseExchange(exchange);
            var timeframeEnum = ValidateAndParseTimeframe(timeframe);

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var gaps = await _marketDataService.GetDataGapsAsync(exchangeEnum, symbol, timeframeEnum, start, end);

            var response = gaps.Select(gap => new GapInfoResponse
            {
                StartTime = gap.StartTime,
                EndTime = gap.EndTime,
                Timeframe = gap.TimeframeInterval,
                MissingPoints = gap.MissingDataPoints
            });

            return Ok(new ApiResponse<IEnumerable<GapInfoResponse>>
            {
                Success = true,
                Message = "Data gaps retrieved successfully",
                Data = response
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<IEnumerable<GapInfoResponse>>(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<IEnumerable<GapInfoResponse>>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data gaps for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse<IEnumerable<GapInfoResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving data gaps",
                Data = null
            });
        }
    }

    /// <summary>
    /// Bulk imports market data
    /// </summary>
    [HttpPost("bulk-import")]
    public async Task<ActionResult<ApiResponse<int>>> BulkImportAsync([FromBody] BulkImportMarketDataRequest request)
    {
        try
        {
            if (request?.Data == null || !request.Data.Any())
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Message = "No market data provided for import",
                    Data = 0
                });
            }

            var validationErrors = new List<string>();
            var marketDataModels = new List<MarketDataModel>();

            // Process each item with validation
            for (int i = 0; i < request.Data.Count; i++)
            {
                var item = request.Data[i];

                try
                {
                    // Validate symbol
                    if (string.IsNullOrWhiteSpace(item.Symbol))
                    {
                        validationErrors.Add($"Item {i + 1}: Symbol is required");
                        continue;
                    }

                    // Use ValidateAndParse methods - will throw ValidationException if invalid
                    var exchange = ValidateAndParseExchange(item.Exchange);
                    var timeframe = ValidateAndParseTimeframe(item.Timeframe);

                    marketDataModels.Add(new MarketDataModel
                    {
                        Symbol = item.Symbol,
                        Timeframe = timeframe,
                        Exchange = exchange,
                        Open = item.Open,
                        High = item.High,
                        Low = item.Low,
                        Close = item.Close,
                        Volume = item.Volume,
                        VWAP = item.VWAP,
                        TradeCount = item.TradeCount,
                        Timestamp = item.Timestamp
                    });
                }
                catch (ValidationException ex)
                {
                    // Catch validation exceptions and add to error list with item context
                    validationErrors.Add($"Item {i + 1}: {ex.Message}");
                }
            }

            // If there are validation errors, return them
            if (validationErrors.Any())
            {
                throw new ApiValidationException((IEnumerable<string>)validationErrors);
            }

            // If no valid data to import
            if (!marketDataModels.Any())
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Message = "No valid market data items to import",
                    Data = 0
                });
            }

            var recordsStored = await _marketDataService.StoreMarketDataAsync(marketDataModels);

            return Ok(new ApiResponse<int>
            {
                Success = true,
                Message = $"Successfully stored {recordsStored} market data records",
                Data = recordsStored
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<int>(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<int>
            {
                Success = false,
                Message = ex.Message,
                Data = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import of market data");
            return StatusCode(500, new ApiResponse<int>
            {
                Success = false,
                Message = "An error occurred while importing market data",
                Data = 0
            });
        }
    }

    /// <summary>
    /// Deletes market data for testing purposes (date range)
    /// </summary>
    [HttpDelete("{exchange}/{symbol}/range")]
    public async Task<ActionResult<ApiResponse>> DeleteMarketDataRangeAsync(string exchange, string symbol,
        [FromQuery] string timeframe = "1h", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            // Validate required parameters
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Both startDate and endDate are required for range deletion"
                });
            }

            if (startDate.Value >= endDate.Value)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Start date must be before end date"
                });
            }

            // Validate and parse parameters using base controller methods
            var exchangeEnum = ValidateAndParseExchange(exchange);
            var timeframeEnum = ValidateAndParseTimeframe(timeframe);

            await _marketDataService.DeleteMarketDataAsync(exchangeEnum, symbol, timeframeEnum, startDate.Value, endDate.Value);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Successfully deleted market data for {symbol} ({timeframeEnum}) on {exchangeEnum} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<object>(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete market data for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting market data"
            });
        }
    }

    /// <summary>
    /// Deletes ALL market data for a symbol (DANGEROUS - testing only)
    /// </summary>
    [HttpDelete("{exchange}/{symbol}/all")]
    public async Task<ActionResult<ApiResponse>> DeleteAllMarketDataAsync(string exchange, string symbol, [FromQuery] bool confirmDelete = false)
    {
        try
        {
            if (!confirmDelete)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "This operation will delete ALL data for the symbol. Add ?confirmDelete=true to proceed.",
                    Errors = ["⚠️  WARNING: This will permanently delete all market data for the symbol"]
                });
            }

            // Validate and parse exchange parameter
            var exchangeEnum = ValidateAndParseExchange(exchange);

            await _marketDataService.DeleteAllMarketDataAsync(exchangeEnum, symbol);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"⚠️  Successfully deleted ALL market data for {symbol} on {exchangeEnum}"
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<object>(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all market data for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting all market data"
            });
        }
    }

    /// <summary>
    /// Deletes market data by specific criteria (advanced testing)
    /// </summary>
    [HttpDelete("{exchange}/{symbol}/criteria")]
    public async Task<ActionResult<ApiResponse>> DeleteMarketDataByCriteriaAsync(string exchange, string symbol, [FromBody] DeleteMarketDataRequest request)
    {
        try
        {
            if (request?.DeleteCriteria == null || !request.DeleteCriteria.Any())
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "No delete criteria provided"
                });
            }

            // Validate and parse exchange parameter once
            var exchangeEnum = ValidateAndParseExchange(exchange);

            var validationErrors = new List<string>();
            var processedCriteria = 0;

            foreach (var criteria in request.DeleteCriteria)
            {
                try
                {
                    // Validate and parse timeframe for each criteria
                    var timeframeEnum = ValidateAndParseTimeframe(criteria.Timeframe);

                    // Validate date range
                    if (criteria.StartDate >= criteria.EndDate)
                    {
                        validationErrors.Add($"Criteria {processedCriteria + 1}: Start date must be before end date");
                        continue;
                    }

                    await _marketDataService.DeleteMarketDataAsync(
                        exchangeEnum,
                        symbol,
                        timeframeEnum,
                        criteria.StartDate,
                        criteria.EndDate);

                    processedCriteria++;
                }
                catch (ApiValidationException ex)
                {
                    validationErrors.Add($"Criteria {processedCriteria + 1}: {ex.Message}");
                }
            }

            // If there were validation errors, report them
            if (validationErrors.Any())
            {
                var successMessage = processedCriteria > 0
                    ? $"Processed {processedCriteria} criteria successfully. "
                    : "";

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"{successMessage}Validation errors occurred:",
                    Errors = validationErrors
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Successfully deleted market data for {symbol} on {exchangeEnum} using {processedCriteria} criteria"
            });
        }
        catch (ApiValidationException ex)
        {
            return CreateValidationErrorResponse<object>(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete market data by criteria for {Exchange}/{Symbol}", exchange, symbol);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting market data by criteria"
            });
        }
    }
}