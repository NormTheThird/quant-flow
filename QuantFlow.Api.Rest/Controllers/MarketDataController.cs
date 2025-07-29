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
    [HttpGet("{symbol}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MarketDataResponse>>>> GetHistoryAsync(string symbol, [FromQuery] string? dataSource = null,
        [FromQuery] string timeframe = "1h", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? limit = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var marketData = await _marketDataService.GetMarketDataAsync(symbol, dataSource, timeframe, start, end, limit);

        var response = marketData.Select(md => new MarketDataResponse
        {
            Symbol = md.Symbol,
            Timeframe = md.Timeframe,
            DataSource = md.DataSource ?? string.Empty,
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

    /// <summary>
    /// Gets the latest market data point for a symbol
    /// </summary>
    [HttpGet("{symbol}/latest")]
    public async Task<ActionResult<ApiResponse<MarketDataResponse>>> GetLatestAsync(string symbol, [FromQuery] string? dataSource = null, [FromQuery] string timeframe = "1h")
    {
        var latestData = await _marketDataService.GetLatestMarketDataAsync(symbol, dataSource, timeframe);

        if (latestData == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "No market data found for the specified symbol"
            });
        }

        var response = new MarketDataResponse
        {
            Symbol = latestData.Symbol,
            Timeframe = latestData.Timeframe,
            DataSource = latestData.DataSource ?? string.Empty,
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

    /// <summary>
    /// Gets data availability information for a symbol
    /// </summary>
    [HttpGet("{symbol}/availability")]
    public async Task<ActionResult<ApiResponse<DataAvailabilityResponse>>> GetAvailabilityAsync(string symbol, [FromQuery] string? dataSource = null)
    {
        var availability = await _marketDataService.GetDataAvailabilityAsync(symbol, dataSource);

        var response = new DataAvailabilityResponse
        {
            Symbol = availability.Symbol,
            DataSource = availability.DataSource ?? string.Empty, // Note: availability.Exchange should be updated to availability.DataSource in the model
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

    /// <summary>
    /// Gets data gaps for a symbol within a date range
    /// </summary>
    [HttpGet("{symbol}/gaps")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GapInfoResponse>>>> GetDataGapsAsync(string symbol, [FromQuery] string? dataSource = null,
        [FromQuery] string timeframe = "1h", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var gaps = await _marketDataService.GetDataGapsAsync(symbol, dataSource, timeframe, start, end);

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

    /// <summary>
    /// Bulk imports market data
    /// </summary>
    [HttpPost("bulk-import")]
    public async Task<ActionResult<ApiResponse<int>>> BulkImportAsync([FromBody] BulkImportMarketDataRequest request)
    {
        var marketDataModels = request.Data.Select(item => new MarketDataModel
        {
            Symbol = item.Symbol,
            Timeframe = item.Timeframe,
            DataSource = item.DataSource,
            Open = item.Open,
            High = item.High,
            Low = item.Low,
            Close = item.Close,
            Volume = item.Volume,
            VWAP = item.VWAP,
            TradeCount = item.TradeCount,
            Timestamp = item.Timestamp
        });

        var recordsStored = await _marketDataService.StoreMarketDataAsync(marketDataModels);

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Message = $"Successfully stored {recordsStored} market data records",
            Data = recordsStored
        });
    }

    /// <summary>
    /// Deletes market data for testing purposes (date range)
    /// </summary>
    [HttpDelete("{symbol}/range")]
    public async Task<ActionResult<ApiResponse>> DeleteMarketDataRangeAsync(
        string symbol,
        [FromQuery] string timeframe = "1h",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? dataSource = null)
    {
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

        try
        {
            await _marketDataService.DeleteMarketDataAsync(symbol, timeframe, startDate.Value, endDate.Value, dataSource);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Successfully deleted market data for {symbol} ({timeframe}) from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            });
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete market data for {Symbol}", symbol);
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
    [HttpDelete("{symbol}/all")]
    public async Task<ActionResult<ApiResponse>> DeleteAllMarketDataAsync(
        string symbol,
        [FromQuery] string? dataSource = null,
        [FromQuery] bool confirmDelete = false)
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

        try
        {
            await _marketDataService.DeleteAllMarketDataAsync(symbol, dataSource);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"⚠️  Successfully deleted ALL market data for {symbol}"
            });
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete all market data for {Symbol}", symbol);
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
    [HttpDelete("{symbol}/criteria")]
    public async Task<ActionResult<ApiResponse>> DeleteMarketDataByCriteriaAsync(
        string symbol,
        [FromBody] DeleteMarketDataRequest request)
    {
        try
        {
            foreach (var criteria in request.DeleteCriteria)
            {
                await _marketDataService.DeleteMarketDataAsync(
                    symbol,
                    criteria.Timeframe,
                    criteria.StartDate,
                    criteria.EndDate,
                    criteria.DataSource);
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Successfully deleted market data for {symbol} using {request.DeleteCriteria.Count} criteria"
            });
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete market data by criteria for {Symbol}", symbol);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting market data by criteria"
            });
        }
    }
}