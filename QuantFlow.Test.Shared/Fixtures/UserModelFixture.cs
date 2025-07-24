namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture class for creating UserModel test data
/// </summary>
public static class UserModelFixture
{
    /// <summary>
    /// Creates a default user model for testing
    /// </summary>
    public static UserModel CreateDefault(string? username = null, string? email = null)
    {
        var defaultUsername = username ?? "testuser";
        var defaultEmail = email ?? $"{defaultUsername}@example.com";

        return new UserModel
        {
            Id = Guid.NewGuid(),
            Username = defaultUsername,
            Email = defaultEmail,
            PasswordHash = "hashedpassword123",
            IsEmailVerified = true,
            IsSystemAdmin = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system",
            UpdatedAt = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a verified user model for testing
    /// </summary>
    public static UserModel CreateVerifiedUser(string? username = null, string? email = null)
    {
        var user = CreateDefault(username, email);
        user.IsEmailVerified = true;
        return user;
    }

    /// <summary>
    /// Creates an unverified user model for testing
    /// </summary>
    public static UserModel CreateUnverifiedUser(string? username = null, string? email = null)
    {
        var user = CreateDefault(username, email);
        user.IsEmailVerified = false;
        return user;
    }

    /// <summary>
    /// Creates a system admin user model for testing
    /// </summary>
    public static UserModel CreateSystemAdmin(string? username = null, string? email = null)
    {
        var adminUsername = username ?? "admin";
        var adminEmail = email ?? $"{adminUsername}@quantflow.com";

        return new UserModel
        {
            Id = Guid.NewGuid(),
            Username = adminUsername,
            Email = adminEmail,
            PasswordHash = "hashedadminpassword123",
            IsEmailVerified = true,
            IsSystemAdmin = true,
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            CreatedBy = "system",
            UpdatedAt = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a new user model (recently created) for testing
    /// </summary>
    public static UserModel CreateNewUser(string? username = null, string? email = null)
    {
        var newUsername = username ?? "newuser";
        var newEmail = email ?? $"{newUsername}@example.com";

        return new UserModel
        {
            Id = Guid.NewGuid(),
            Username = newUsername,
            Email = newEmail,
            PasswordHash = "hashedpassword123",
            IsEmailVerified = false,
            IsSystemAdmin = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "registration",
            UpdatedAt = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a user model with specific properties for testing
    /// </summary>
    public static UserModel CreateCustomUser(string username, string email, bool isEmailVerified = true,
        bool isSystemAdmin = false, DateTime? createdAt = null, string? passwordHash = null)
    {
        return new UserModel
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash ?? "hashedpassword123",
            IsEmailVerified = isEmailVerified,
            IsSystemAdmin = isSystemAdmin,
            CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-1),
            CreatedBy = "test",
            UpdatedAt = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a batch of user models for bulk testing
    /// </summary>
    public static List<UserModel> CreateBatch(int count = 5, string usernamePrefix = "user")
    {
        var users = new List<UserModel>();

        for (int i = 1; i <= count; i++)
        {
            users.Add(CreateDefault($"{usernamePrefix}{i}", $"{usernamePrefix}{i}@example.com"));
        }

        return users;
    }

    /// <summary>
    /// Creates a user model for updating tests
    /// </summary>
    public static UserModel CreateForUpdate(Guid userId, string? newUsername = null, string? newEmail = null)
    {
        return new UserModel
        {
            Id = userId,
            Username = newUsername ?? "updateduser",
            Email = newEmail ?? "updated@example.com",
            PasswordHash = "updatedpasswordhash",
            IsEmailVerified = true,
            IsSystemAdmin = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }
}