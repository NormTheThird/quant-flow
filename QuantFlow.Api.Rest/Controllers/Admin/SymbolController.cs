namespace QuantFlow.Api.Rest.Controllers.Admin;

/// <summary>
/// API controller for symbol management - Admin Only
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/admin/[controller]")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},ApiKey")]
public class SymbolController : BaseController<SymbolController>
{
    private readonly ISymbolService _symbolService;

    public SymbolController(ILogger<SymbolController> logger, ISymbolService symbolService)
        : base(logger)
    {
        _symbolService = symbolService ?? throw new ArgumentNullException(nameof(symbolService));
    }

    /// <summary>
    /// Gets all symbols
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SymbolModel>>>> GetAllSymbolsAsync()
    {
        try
        {
            var symbols = await _symbolService.GetAllAsync();

            return Ok(new ApiResponse<IEnumerable<SymbolModel>>
            {
                Success = true,
                Message = "Symbols retrieved successfully",
                Data = symbols
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving symbols");
            return StatusCode(500, new ApiResponse<IEnumerable<SymbolModel>> { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a symbol by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SymbolModel>>> GetSymbolByIdAsync(Guid id)
    {
        try
        {
            var symbol = await _symbolService.GetByIdAsync(id);

            if (symbol == null)
                return NotFound(new ApiResponse<SymbolModel> { Success = false, Message = $"Symbol with ID {id} not found" });

            return Ok(new ApiResponse<SymbolModel>
            {
                Success = true,
                Message = "Symbol retrieved successfully",
                Data = symbol
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving symbol {SymbolId}", id);
            return StatusCode(500, new ApiResponse<SymbolModel> { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets active symbols only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SymbolModel>>>> GetActiveSymbolsAsync()
    {
        try
        {
            var symbols = await _symbolService.GetActiveAsync();

            return Ok(new ApiResponse<IEnumerable<SymbolModel>>
            {
                Success = true,
                Message = "Active symbols retrieved successfully",
                Data = symbols
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active symbols");
            return StatusCode(500, new ApiResponse<IEnumerable<SymbolModel>> { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new symbol
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SymbolModel>>> CreateSymbolAsync([FromBody] CreateSymbolRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new ApiResponse<SymbolModel> { Success = false, Message = "Request body is required" });

            // Check for duplicate symbol (including deleted ones)
            var existingSymbol = await _symbolService.GetBySymbolAsync(request.Symbol);
            if (existingSymbol != null)
            {
                if (existingSymbol.IsDeleted)
                {
                    // Return special response indicating symbol can be restored
                    return Conflict(new ApiResponse<SymbolModel>
                    {
                        Success = false,
                        Message = $"Symbol '{request.Symbol}' was previously deleted. Use the restore endpoint to restore it.",
                        Data = existingSymbol
                    });
                }

                return Conflict(new ApiResponse<SymbolModel> { Success = false, Message = $"Symbol '{request.Symbol}' already exists" });
            }

            var symbolModel = request.ToModel();
            var createdSymbol = await _symbolService.CreateAsync(symbolModel);

            return Ok(new ApiResponse<SymbolModel>
            {
                Success = true,
                Message = "Symbol created successfully",
                Data = createdSymbol
            });
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(ex, "Duplicate symbol creation attempt: {Symbol}", request.Symbol);
            return Conflict(new ApiResponse<SymbolModel> { Success = false, Message = $"Symbol '{request.Symbol}' already exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating symbol");
            return StatusCode(500, new ApiResponse<SymbolModel> { Success = false, Message = "An error occurred while creating the symbol" });
        }
    }

    /// <summary>
    /// Restores a deleted symbol
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<ApiResponse<SymbolModel>>> RestoreSymbolAsync(Guid id, [FromBody] UpdateSymbolRequest request)
    {
        try
        {
            var symbolModel = request.ToModelWithId(id);

            var restoredSymbol = await _symbolService.RestoreSymbolAsync(symbolModel);

            return Ok(new ApiResponse<SymbolModel>
            {
                Success = true,
                Message = "Symbol restored successfully",
                Data = restoredSymbol
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<SymbolModel> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<SymbolModel> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring symbol {SymbolId}", id);
            return StatusCode(500, new ApiResponse<SymbolModel> { Success = false, Message = "An error occurred while restoring the symbol" });
        }
    }

    /// <summary>
    /// Updates an existing symbol
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SymbolModel>>> UpdateSymbolAsync(Guid id, [FromBody] UpdateSymbolRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new ApiResponse<SymbolModel> { Success = false, Message = "Request body is required" });

            var existingSymbol = await _symbolService.GetByIdAsync(id);
            if (existingSymbol == null)
                return NotFound(new ApiResponse<SymbolModel> { Success = false, Message = $"Symbol with ID {id} not found" });

            request.UpdateModel(existingSymbol);
            var updatedSymbol = await _symbolService.UpdateAsync(existingSymbol);

            return Ok(new ApiResponse<SymbolModel>
            {
                Success = true,
                Message = "Symbol updated successfully",
                Data = updatedSymbol
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating symbol {SymbolId}", id);
            return StatusCode(500, new ApiResponse<SymbolModel> { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a symbol (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSymbolAsync(Guid id)
    {
        try
        {
            var result = await _symbolService.DeleteAsync(id);

            if (!result)
                return NotFound(new ApiResponse<bool> { Success = false, Message = $"Symbol with ID {id} not found" });

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Symbol deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting symbol {SymbolId}", id);
            return StatusCode(500, new ApiResponse<bool> { Success = false, Message = ex.Message });
        }
    }
}