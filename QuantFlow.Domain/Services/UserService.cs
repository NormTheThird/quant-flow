namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for user management and authentication operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository, IUserRefreshTokenRepository userRefreshTokenRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userRefreshTokenRepository = userRefreshTokenRepository ?? throw new ArgumentNullException(nameof(userRefreshTokenRepository));
    }

    /// <summary>
    /// Validates user credentials during authentication
    /// </summary>
    public async Task<UserModel?> ValidateUserCredentialsAsync(string email, string password)
    {
        _logger.LogInformation("Validating credentials for email: {Email}", email);

        // Get password hash separately for validation
        var passwordHash = await _userRepository.GetPasswordHashByEmailAsync(email);
        if (passwordHash == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
            return null;
        }

        if (!VerifyPassword(password, passwordHash))
        {
            _logger.LogWarning("Invalid password for email: {Email}", email);
            return null;
        }

        // Only get full user data if password is valid
        var user = await _userRepository.GetByEmailAsync(email);
        return user;
    }

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    public async Task<UserModel?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    public async Task<UserModel?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    /// <summary>
    /// Creates a new user with hashed password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's plain text password (will be hashed)</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="username">User's username</param>
    /// <param name="isSystemAdmin">Whether user should have admin privileges</param>
    /// <returns>Created user business model</returns>
    public async Task<UserModel> CreateUserAsync(string email, string password, string firstName, string lastName, string username, bool isSystemAdmin = false)
    {
        _logger.LogInformation("Creating user with email: {Email}", email);

        // Validate input parameters
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        // Check if user already exists by email
        var existingUserByEmail = await _userRepository.GetByEmailAsync(email);
        if (existingUserByEmail != null)
            throw new InvalidOperationException($"Email '{email}' already exists");

        // Check if user already exists by username
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(username);
        if (existingUserByUsername != null)
            throw new InvalidOperationException($"Username '{username}' already exists");

        // Hash the password
        var passwordHash = HashPassword(password);

        // Create user model
        var userModel = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsSystemAdmin = isSystemAdmin,
            IsEmailVerified = false, // New users start with unverified email
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create user in repository
        var createdUser = await _userRepository.CreateAsync(userModel);
        
        await _userRepository.UpdatePasswordHashAsync(createdUser.Id, passwordHash);

        _logger.LogInformation("Successfully created user with ID: {UserId}", createdUser.Id);

        return createdUser;
    }

    /// <summary>
    /// Updates a user's password
    /// </summary>
    public async Task UpdatePasswordAsync(Guid userId, string newPassword)
    {
        _logger.LogInformation("Updating password for user: {UserId}", userId);

        var hashedPassword = HashPassword(newPassword);
        var success = await _userRepository.UpdatePasswordHashAsync(userId, hashedPassword);

        if (!success)
            throw new NotFoundException($"User with ID {userId} not found");
    }

    /// <summary>
    /// Creates a token for a user
    /// </summary>
    public async Task<UserRefreshTokenModel> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, RefreshTokenType tokenType)
    {
        _logger.LogInformation("Creating {TokenType} token for user: {UserId}", tokenType, userId);

        return tokenType switch
        {
            RefreshTokenType.Refresh => await _userRefreshTokenRepository.CreateRefreshTokenAsync(userId, token, expiresAt),
            RefreshTokenType.PasswordReset => await _userRefreshTokenRepository.CreatePasswordResetTokenAsync(userId, token, expiresAt),
            _ => throw new ArgumentException("Invalid token type", nameof(tokenType))
        };
    }

    /// <summary>
    /// Gets a token by token string and type
    /// </summary>
    /// <param name="token">The token string</param>
    /// <param name="tokenType">Type of token to retrieve</param>
    /// <returns>User refresh token model if found, null otherwise</returns>
    public async Task<UserRefreshTokenModel?> GetRefreshTokenAsync(string token, RefreshTokenType tokenType)
    {
        return tokenType switch
        {
            RefreshTokenType.Refresh => await _userRefreshTokenRepository.GetByTokenAsync(token),
            RefreshTokenType.PasswordReset => await _userRefreshTokenRepository.GetPasswordResetTokenAsync(token),
            _ => throw new ArgumentException("Invalid token type", nameof(tokenType))
        };
    }

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Revoking refresh token");

        return await _userRefreshTokenRepository.RevokeTokenAsync(refreshToken);
    }

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices)
    /// </summary>
    public async Task<int> RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        _logger.LogInformation("Revoking all refresh tokens for user: {UserId}", userId);

        return await _userRefreshTokenRepository.RevokeAllUserTokensAsync(userId);
    }



    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    private static string HashPassword(string password)
    {
        // You'll need to add BCrypt.Net NuGet package
        // return BCrypt.Net.BCrypt.HashPassword(password);

        // For now, just return a placeholder (DO NOT USE IN PRODUCTION)
        return $"HASHED_{password}";
    }

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        // You'll need to add BCrypt.Net NuGet package
        // return BCrypt.Net.BCrypt.Verify(password, hash);

        // For now, just do a simple comparison (DO NOT USE IN PRODUCTION)
        return hash == $"HASHED_{password}";
    }
}