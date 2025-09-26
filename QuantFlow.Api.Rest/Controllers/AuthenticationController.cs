namespace QuantFlow.Api.Rest.Controllers;

/// <summary>
/// Authentication controller for JWT token management
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/pub/[controller]")]
[ApiVersion("1.0")]
public class AuthenticationController : BaseController<AuthenticationController>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthenticationController(ILogger<AuthenticationController> logger, IJwtTokenService jwtTokenService, IUserService userService,
                                    IConfiguration configuration)
        : base(logger)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Create a new user for testing purposes
    /// </summary>
    [HttpPost("create-user")]
    public async Task<ActionResult<ApiResponse<BaseUserModel>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating user with email: {Email}", request.Email);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))

                return BadRequest(new ApiResponse { Success = false, Message = "Email and password are required" });


            // Check if user already exists
            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                return Conflict(new ApiResponse { Success = false, Message = "User with this email already exists" });

            // Create the user
            var newUser = await _userService.CreateUserAsync(request.Email, request.Password, request.FirstName ?? string.Empty,
                request.LastName ?? string.Empty, request.Username ?? request.Email.Split('@')[0], request.IsSystemAdmin);

            var response = new BaseUserModel
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                IsSystemAdmin = newUser.IsSystemAdmin
            };

            _logger.LogInformation("User created successfully with ID: {UserId}", newUser.Id);
            return Success(response, "User created successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "User creation failed - conflict for email: {Email}", request.Email);
            return Conflict(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User creation failed for email: {Email}", request.Email);
            return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Authenticate with email and password
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<ActionResult<ApiResponse<AuthenticateResponse>>> Authenticate([FromBody] AuthenticateRequest request)
    {
        try
        {
            // Validate user credentials
            var user = await _userService.ValidateUserCredentialsAsync(request.Email, request.Password);
            if (user == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid email or password" });

            if (user.IsDeleted)
                return BadRequest(new ApiResponse { Success = false, Message = "Account is disabled" });

            var accessToken = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, user.IsSystemAdmin);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Store REFRESH token, not password reset token
            await _userService.CreateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7), RefreshTokenType.Refresh);

            var response = new AuthenticateResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                User = new BaseUserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsSystemAdmin = user.IsSystemAdmin
                },
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationHours"] ?? "24") * 3600
            };

            return Success(response, "Authentication successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for email: {Email}", request.Email);
            return BadRequest(new ApiResponse { Success = false, Message = "Authentication failed" });
        }
    }

    /// <summary>
    /// Revoke authentication - invalidate refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ApiResponse>> Revoke()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var userGuid))
                await _userService.RevokeAllUserRefreshTokensAsync(userGuid);

            return Success("Authentication revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revoke failed for user: {UserId}", ClaimTypes.NameIdentifier);
            return BadRequest(new ApiResponse { Success = false, Message = "Revoke failed" });
        }
    }

    /// <summary>
    /// Forgot password - send reset email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null || user.IsDeleted)
                // Don't reveal if email exists or not for security
                return Success("If the email exists, reset instructions have been sent");

            // Generate password reset token
            var resetToken = _jwtTokenService.GenerateRefreshToken();

            // Store reset token using the generic method
            await _userService.CreateRefreshTokenAsync(user.Id, resetToken, DateTime.UtcNow.AddHours(1), RefreshTokenType.PasswordReset);

            // Send email (you'll need to implement email service)
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return Success("If the email exists, reset instructions have been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password failed for email: {Email}", request.Email);
            return Success("If the email exists, reset instructions have been sent");
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var storedToken = await _userService.GetRefreshTokenAsync(request.RefreshToken, RefreshTokenType.PasswordReset);
            if (storedToken == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid or expired reset token" });

            await _userService.UpdatePasswordAsync(storedToken.UserId, request.NewPassword);

            await _userService.RevokeRefreshTokenAsync(request.RefreshToken);

            return Success("Password reset successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed");
            return BadRequest(new ApiResponse { Success = false, Message = "Password reset failed" });
        }
    }

    /// <summary>
    /// Refreshes an expired JWT token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate the refresh token
            var storedToken = await _userService.GetRefreshTokenAsync(request.RefreshToken, RefreshTokenType.Refresh);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid or expired refresh token" });

            // Get user info
            var user = await _userService.GetUserByIdAsync(storedToken.UserId);
            if (user == null || user.IsDeleted)
                return BadRequest(new ApiResponse { Success = false, Message = "User not found or inactive" });

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, user.IsSystemAdmin);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // Revoke old token and create new one
            await _userService.RevokeRefreshTokenAsync(request.RefreshToken);
            await _userService.CreateRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7), RefreshTokenType.Refresh);

            var response = new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationHours"] ?? "24") * 3600
            };

            return Success(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return BadRequest(new ApiResponse { Success = false, Message = "Token refresh failed" });
        }
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ApiResponse<BaseUserModel>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid user ID" });

            var user = await _userService.GetUserByIdAsync(userGuid);
            if (user == null)
                return BadRequest(new ApiResponse { Success = false, Message = "User not found" });

            if (user.IsDeleted)
                return BadRequest(new ApiResponse { Success = false, Message = "Account is disabled" });

            var baseUserModel = new BaseUserModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsSystemAdmin = user.IsSystemAdmin
            };

            return Success(baseUserModel, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user failed");
            return BadRequest(new ApiResponse { Success = false, Message = "Failed to get user information" });
        }
    }

    /// <summary>
    /// Generates a JWT token using API key authentication
    /// </summary>
    [HttpPost("token")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> GenerateToken([FromBody] TokenRequest request)
    {
        // Look up the user to get IsSystemAdmin
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new ApiResponse { Success = false, Message = "User not found" });

        if (user.IsDeleted)
            return BadRequest(new ApiResponse { Success = false, Message = "Account is disabled" });

        var token = _jwtTokenService.GenerateToken(request.UserId, request.Email, user.IsSystemAdmin);
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
    /// Validates a JWT token
    /// </summary>
    [HttpPost("validate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult<ApiResponse<TokenValidationResponse>> ValidateToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "user";

        var response = new TokenValidationResponse
        {
            UserId = userId,
            Email = email,
            IsSystemAdmin = role == "admin",
            IsValid = true
        };

        return Success(response, "Token is valid");
    }
}