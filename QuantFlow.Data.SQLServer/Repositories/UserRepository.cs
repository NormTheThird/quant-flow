namespace QuantFlow.Data.SQLServer.Repositories;

/// <summary>
/// SQL Server implementation of user repository
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>User business model if found, null otherwise</returns>
    public async Task<UserModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting user with ID: {UserId}", id);

        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of user business models</returns>
    public async Task<IEnumerable<UserModel>> GetAllAsync()
    {
        _logger.LogInformation("Getting all users");

        var entities = await _context.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        return entities.ToBusinessModels();
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User business model to create</param>
    /// <returns>Created user business model</returns>
    public async Task<UserModel> CreateAsync(UserModel user)
    {
        _logger.LogInformation("Creating user with email: {Email}", user.Email);

        var entity = user.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">User business model with updates</param>
    /// <returns>Updated user business model</returns>
    public async Task<UserModel> UpdateAsync(UserModel user)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", user.Id);

        var entity = await _context.Users.FindAsync(user.Id);
        if (entity == null)
            throw new NotFoundException($"User with ID {user.Id} not found");

        entity.Username = user.Username;
        entity.Email = user.Email;
        entity.PasswordHash = user.PasswordHash;
        entity.IsEmailVerified = user.IsEmailVerified;
        entity.IsSystemAdmin = user.IsSystemAdmin;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    /// <summary>
    /// Soft deletes a user
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        var entity = await _context.Users.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <returns>User business model if found, null otherwise</returns>
    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Getting user by email: {Email}", email);

        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

        return entity?.ToBusinessModel();
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username to search for</param>
    /// <returns>User business model if found, null otherwise</returns>
    public async Task<UserModel?> GetByUsernameAsync(string username)
    {
        _logger.LogInformation("Getting user by username: {Username}", username);

        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

        return entity?.ToBusinessModel();
    }
}