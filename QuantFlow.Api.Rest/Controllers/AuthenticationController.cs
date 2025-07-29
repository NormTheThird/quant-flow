namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// Authentication controller for JWT token management
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/[controller]")]
[ApiVersion("1.0")]
public class AuthenticationController : BaseController
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IJwtTokenService jwtTokenService, IConfiguration configuration, ILogger<AuthenticationController> logger) : base(logger)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a JWT token using API key authentication
    /// </summary>
    [HttpPost("token")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public ActionResult<ApiResponse<TokenResponse>> GenerateToken([FromBody] TokenRequest request)
    {
        var token = _jwtTokenService.GenerateToken(
            request.UserId,
            request.Email,
            request.Roles ?? []
        );

        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new TokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationHours"] ?? "24") * 3600
        };

        return Success(response, "Token generated successfully");
    }

    /// <summary>
    /// Refreshes an expired JWT token
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<ApiResponse<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = _jwtTokenService.ValidateToken(request.AccessToken);
        if (principal == null)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid token"
            });
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var newToken = _jwtTokenService.GenerateToken(userId, email, roles);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new TokenResponse
        {
            AccessToken = newToken,
            RefreshToken = newRefreshToken,
            TokenType = "Bearer",
            ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationHours"] ?? "24") * 3600
        };

        return Ok(new ApiResponse<TokenResponse>
        {
            Success = true,
            Message = "Token refreshed successfully",
            Data = response
        });
    }

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    [HttpPost("validate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult<ApiResponse<TokenValidationResponse>> ValidateToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var response = new TokenValidationResponse
        {
            UserId = userId,
            Email = email,
            Roles = roles,
            IsValid = true
        };

        return Success(response, "Token is valid");
    }
}
