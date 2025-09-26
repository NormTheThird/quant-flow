namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of user refresh token repository
/// </summary>
public class UserRefreshTokenRepository : IUserRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRefreshTokenRepository> _logger;

    public UserRefreshTokenRepository(ApplicationDbContext context, ILogger<UserRefreshTokenRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new refresh token for a user
    /// </summary>
    public async Task<UserRefreshTokenModel> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        _logger.LogInformation("Creating refresh token for user: {UserId}", userId);

        var entity = new UserRefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            TokenType = "refresh",
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.UserRefreshTokens.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Creates a password reset token
    /// </summary>
    public async Task<UserRefreshTokenModel> CreatePasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        _logger.LogInformation("Creating password reset token for user: {UserId}", userId);

        var entity = new UserRefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            TokenType = "password_reset",
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.UserRefreshTokens.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Gets a refresh token by token string
    /// </summary>
    public async Task<UserRefreshTokenModel?> GetByTokenAsync(string token)
    {
        _logger.LogInformation("Getting refresh token by token string");

        var entity = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token
                                    && rt.TokenType == "refresh"
                                    && !rt.IsRevoked
                                    && !rt.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets a password reset token by token string
    /// </summary>
    public async Task<UserRefreshTokenModel?> GetPasswordResetTokenAsync(string token)
    {
        _logger.LogInformation("Getting password reset token by token string");

        var entity = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token
                                    && rt.TokenType == "password_reset"
                                    && !rt.IsRevoked
                                    && !rt.IsDeleted
                                    && rt.ExpiresAt > DateTime.UtcNow);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string token)
    {
        _logger.LogInformation("Revoking refresh token");

        var entity = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);

        if (entity == null)
            return false;

        entity.IsRevoked = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = "System";

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    public async Task<int> RevokeAllUserTokensAsync(Guid userId)
    {
        _logger.LogInformation("Revoking all refresh tokens for user: {UserId}", userId);

        var entities = await _context.UserRefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsDeleted)
            .ToListAsync();

        foreach (var entity in entities)
        {
            entity.IsRevoked = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";
        }

        await _context.SaveChangesAsync();
        return entities.Count;
    }

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    public async Task<IEnumerable<UserRefreshTokenModel>> GetActiveTokensByUserAsync(Guid userId)
    {
        _logger.LogInformation("Getting active refresh tokens for user: {UserId}", userId);

        var entities = await _context.UserRefreshTokens
            .Where(rt => rt.UserId == userId
                      && rt.TokenType == "refresh"
                      && !rt.IsRevoked
                      && !rt.IsDeleted
                      && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        return entities.Select(e => e.ToBusinessModel());
    }

    /// <summary>
    /// Deletes expired tokens (cleanup)
    /// </summary>
    public async Task<int> DeleteExpiredTokensAsync()
    {
        _logger.LogInformation("Deleting expired refresh tokens");

        var expiredTokens = await _context.UserRefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow && !rt.IsDeleted)
            .ToListAsync();

        foreach (var token in expiredTokens)
        {
            token.IsDeleted = true;
            token.UpdatedAt = DateTime.UtcNow;
            token.UpdatedBy = "System";
        }

        await _context.SaveChangesAsync();
        return expiredTokens.Count;
    }
}