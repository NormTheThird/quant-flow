namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// API controller for exchange configuration operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "ApiKeyPolicy")]
public class ExchangeConfigurationController : BaseController<ExchangeConfigurationController>
{
    private readonly IExchangeConfigurationService _exchangeConfigurationService;

    public ExchangeConfigurationController(
        IExchangeConfigurationService exchangeConfigurationService,
        ILogger<ExchangeConfigurationController> logger) : base(logger)
    {
        _exchangeConfigurationService = exchangeConfigurationService;
    }

    /// <summary>
    /// Gets all supported exchange configurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExchangeConfigurationResponse>>>> GetAllAsync()
    {
        var exchanges = await _exchangeConfigurationService.GetSupportedExchangesAsync();

        var response = exchanges.Select(exchange => new ExchangeConfigurationResponse
        {
            Name = exchange.Name,
            Exchange = exchange.Exchange,
            MakerFee = exchange.BaseMakerFeePercent,
            TakerFee = exchange.BaseTakerFeePercent,
            IsSupported = exchange.IsSupported,
            FeeTiers = exchange.FeeTiers?.Select(ft => new FeeTierResponse
            {
                TierLevel = ft.TierLevel,
                MinimumVolume = ft.MinimumVolumeThreshold,
                MakerFeePercentage = ft.MakerFeePercent,
                TakerFeePercentage = ft.TakerFeePercent
            }).ToList() ?? [],
            SymbolOverrides = exchange.SymbolOverrides?.Select(so => new SymbolFeeOverrideResponse
            {
                Symbol = so.Symbol,
                MakerFeeOverride = so.MakerFeePercent,
                TakerFeeOverride = so.TakerFeePercent,
                Reason = so.Reason
            }).ToList() ?? []
        });

        return Ok(new ApiResponse<IEnumerable<ExchangeConfigurationResponse>>
        {
            Success = true,
            Message = "Exchange configurations retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Gets configuration for a specific exchange
    /// </summary>
    [HttpGet("{exchange}")]
    public async Task<ActionResult<ApiResponse<ExchangeConfigurationResponse>>> GetByExchangeAsync(Exchange exchange)
    {
        var config = await _exchangeConfigurationService.GetExchangeConfigurationAsync(exchange);

        if (config == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Exchange configuration not found"
            });
        }

        var response = new ExchangeConfigurationResponse
        {
            Name = config.Name,
            Exchange = config.Exchange,
            MakerFee = config.BaseMakerFeePercent,
            TakerFee = config.BaseTakerFeePercent,
            IsSupported = config.IsSupported,
            FeeTiers = config.FeeTiers?.Select(ft => new FeeTierResponse
            {
                TierLevel = ft.TierLevel,
                MinimumVolume = ft.MinimumVolumeThreshold,
                MakerFeePercentage = ft.MakerFeePercent,
                TakerFeePercentage = ft.TakerFeePercent
            }).ToList() ?? [],
            SymbolOverrides = config.SymbolOverrides?.Select(so => new SymbolFeeOverrideResponse
            {
                Symbol = so.Symbol,
                MakerFeeOverride = so.MakerFeePercent,
                TakerFeeOverride = so.TakerFeePercent,
                Reason = so.Reason
            }).ToList() ?? []
        };

        return Ok(new ApiResponse<ExchangeConfigurationResponse>
        {
            Success = true,
            Message = "Exchange configuration retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Updates base fees for an exchange
    /// </summary>
    [HttpPut("{exchange}/fees")]
    public async Task<ActionResult<ApiResponse>> UpdateFeesAsync(
        Exchange exchange,
        [FromBody] UpdateExchangeFeesRequest request)
    {
        var success = await _exchangeConfigurationService.SetBaseFeesAsync(exchange, request.MakerFee, request.TakerFee);

        if (!success)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Failed to update exchange fees"
            });
        }

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Exchange fees updated successfully"
        });
    }

    /// <summary>
    /// Gets fee tiers for a specific exchange
    /// </summary>
    [HttpGet("{exchange}/fee-tiers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FeeTierResponse>>>> GetFeeTiersAsync(Exchange exchange)
    {
        var feeTiers = await _exchangeConfigurationService.GetFeeTiersAsync(exchange);

        var response = feeTiers.Select(ft => new FeeTierResponse
        {
            TierLevel = ft.TierLevel,
            MinimumVolume = ft.MinimumVolumeThreshold,
            MakerFeePercentage = ft.MakerFeePercent,
            TakerFeePercentage = ft.TakerFeePercent
        });

        return Ok(new ApiResponse<IEnumerable<FeeTierResponse>>
        {
            Success = true,
            Message = "Fee tiers retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Creates or updates a symbol fee override for an exchange
    /// </summary>
    [HttpPost("{exchange}/symbol-overrides")]
    public async Task<ActionResult<ApiResponse<SymbolFeeOverrideResponse>>> CreateSymbolOverrideAsync(
        Exchange exchange,
        [FromBody] CreateSymbolFeeOverrideRequest request)
    {
        var createdOverride = await _exchangeConfigurationService.SetSymbolFeeOverrideAsync(
            exchange,
            request.Symbol,
            request.MakerFeeOverride ?? 0m,
            request.TakerFeeOverride ?? 0m,
            request.Reason);

        var response = new SymbolFeeOverrideResponse
        {
            Symbol = createdOverride.Symbol,
            MakerFeeOverride = createdOverride.MakerFeePercent,
            TakerFeeOverride = createdOverride.TakerFeePercent,
            Reason = createdOverride.Reason
        };

        return Ok(new ApiResponse<SymbolFeeOverrideResponse>
        {
            Success = true,
            Message = "Symbol fee override created successfully",
            Data = response
        });
    }

    /// <summary>
    /// Gets symbol fee overrides for a specific exchange
    /// </summary>
    [HttpGet("{exchange}/symbol-overrides")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SymbolFeeOverrideResponse>>>> GetSymbolOverridesAsync(
        Exchange exchange,
        [FromQuery] string? symbol = null)
    {
        var overrides = await _exchangeConfigurationService.GetSymbolOverridesAsync(exchange, symbol);

        var response = overrides.Select(so => new SymbolFeeOverrideResponse
        {
            Symbol = so.Symbol,
            MakerFeeOverride = so.MakerFeePercent,
            TakerFeeOverride = so.TakerFeePercent,
            Reason = so.Reason
        });

        return Ok(new ApiResponse<IEnumerable<SymbolFeeOverrideResponse>>
        {
            Success = true,
            Message = "Symbol overrides retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Calculates trading fees for a specific trade
    /// </summary>
    [HttpPost("{exchange}/calculate-fees")]
    public async Task<ActionResult<ApiResponse<FeeCalculationResult>>> CalculateFeesAsync(
        Exchange exchange,
        [FromQuery] string symbol,
        [FromQuery] decimal tradeValue,
        [FromQuery] decimal? volumeTier = null)
    {
        var feeResult = await _exchangeConfigurationService.CalculateFeesAsync(exchange, symbol, tradeValue, volumeTier);

        return Ok(new ApiResponse<FeeCalculationResult>
        {
            Success = true,
            Message = "Trading fee calculated successfully",
            Data = feeResult
        });
    }
}