namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// User preferences controller for managing user settings
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/[controller]")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : BaseController<UserController>
{
    private readonly IUserPreferencesRepository _userPreferencesRepository;

    public UserController(ILogger<UserController> logger, IUserPreferencesRepository userPreferencesRepository)
        : base(logger)
    {
        _userPreferencesRepository = userPreferencesRepository ?? throw new ArgumentNullException(nameof(userPreferencesRepository));
    }

    /// <summary>
    /// Ensures user preferences exist, creates default if they don't
    /// </summary>
    [HttpPost("preferences/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<UserPreferencesModel>>> ValidateUserPreferences(Guid userId)
    {
        try
        {
            _logger.LogInformation("Ensuring preferences exist for user: {UserId}", userId);

            var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId);

            if (preferences == null)
            {
                var defaultPreferences = await _userPreferencesRepository.GetDefaultPreferencesAsync(userId);
                preferences = await _userPreferencesRepository.CreateAsync(defaultPreferences);
                _logger.LogInformation("Default preferences created for user: {UserId}", userId);
            }

            return Success(preferences, "User preferences retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure preferences for user: {UserId}", userId);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Failed to ensure user preferences" });
        }
    }
}