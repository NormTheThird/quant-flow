namespace QuantFlow.Api.Rest.Controllers.Admin;

/// <summary>
/// API controller for market data configuration management - Admin Only
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/admin/[controller]")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},ApiKey")]
public class MarketDataConfigurationController : BaseController<MarketDataConfigurationController>
{
    private readonly IMarketDataConfigurationService _marketDataConfigurationService;

    public MarketDataConfigurationController(ILogger<MarketDataConfigurationController> logger, IMarketDataConfigurationService marketDataConfigurationService)
        : base(logger)
    {
        _marketDataConfigurationService = marketDataConfigurationService ?? throw new ArgumentNullException(nameof(marketDataConfigurationService));
    }

    /// <summary>
    /// Gets all market data configurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MarketDataConfigurationResponse>>>> GetAllConfigurationsAsync()
    {
        try
        {
            var configurations = await _marketDataConfigurationService.GetAllConfigurationsAsync();
            return Ok(new ApiResponse<IEnumerable<MarketDataConfigurationResponse>>
            {
                Success = true,
                Message = "Configurations retrieved successfully",
                Data = configurations.ToResponses()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market data configurations");
            return StatusCode(500, new ApiResponse<IEnumerable<MarketDataConfigurationResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving configurations"
            });
        }
    }

    /// <summary>
    /// Gets a single configuration by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MarketDataConfigurationResponse>>> GetConfigurationByIdAsync(Guid id)
    {
        try
        {
            var config = await _marketDataConfigurationService.GetConfigurationByIdAsync(id);
            if (config == null)
                return NotFound(new ApiResponse<MarketDataConfigurationResponse>
                {
                    Success = false,
                    Message = $"Configuration with ID {id} not found"
                });

            return Ok(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = true,
                Message = "Configuration retrieved successfully",
                Data = config.ToResponse()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {ConfigId}", id);
            return StatusCode(500, new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the configuration"
            });
        }
    }

    /// <summary>
    /// Creates a new market data configuration
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MarketDataConfigurationResponse>>> CreateConfigurationAsync([FromBody] CreateMarketDataConfigurationRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new ApiResponse<MarketDataConfigurationResponse>
                {
                    Success = false,
                    Message = "Request body is required"
                });

            var model = request.ToModel();
            var config = await _marketDataConfigurationService.CreateAsync(model);

            return Ok(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = true,
                Message = "Configuration created successfully",
                Data = config.ToResponse()
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating market data configuration");
            return StatusCode(500, new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = "An error occurred while creating the configuration"
            });
        }
    }

    /// <summary>
    /// Toggles a specific interval on or off
    /// </summary>
    [HttpPut("{id}/interval/{interval}")]
    public async Task<ActionResult<ApiResponse<MarketDataConfigurationResponse>>> ToggleIntervalAsync(Guid id, string interval, [FromBody] ToggleIntervalRequest request)
    {
        try
        {
            var config = await _marketDataConfigurationService.ToggleIntervalAsync(id, interval, request.IsActive);
            if (config == null)
                return NotFound(new ApiResponse<MarketDataConfigurationResponse>
                {
                    Success = false,
                    Message = $"Configuration with ID {id} not found"
                });

            return Ok(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = true,
                Message = $"Interval {interval} {(request.IsActive ? "enabled" : "disabled")} successfully",
                Data = config.ToResponse()
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling interval {Interval} for configuration {ConfigId}", interval, id);
            return StatusCode(500, new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = "An error occurred while toggling the interval"
            });
        }
    }

    /// <summary>
    /// Updates an entire configuration
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MarketDataConfigurationResponse>>> UpdateConfigurationAsync(Guid id, [FromBody] UpdateMarketDataConfigurationRequest request)
    {
        try
        {
            var model = request.ToModel(id);
            var config = await _marketDataConfigurationService.UpdateAsync(model);

            if (config == null)
                return NotFound(new ApiResponse<MarketDataConfigurationResponse>
                {
                    Success = false,
                    Message = $"Configuration with ID {id} not found"
                });

            return Ok(new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = true,
                Message = "Configuration updated successfully",
                Data = config.ToResponse()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration {ConfigId}", id);
            return StatusCode(500, new ApiResponse<MarketDataConfigurationResponse>
            {
                Success = false,
                Message = "An error occurred while updating the configuration"
            });
        }
    }

    /// <summary>
    /// Deletes a single configuration (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConfigurationAsync(Guid id)
    {
        try
        {
            var result = await _marketDataConfigurationService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Configuration with ID {id} not found"
                });

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Configuration deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration {ConfigId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the configuration"
            });
        }
    }
}