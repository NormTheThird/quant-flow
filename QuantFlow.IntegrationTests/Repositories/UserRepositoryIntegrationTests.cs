namespace QuantFlow.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for UserRepository with in-memory database
/// </summary>
public class UserRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<UserRepository>>();
        _repository = new UserRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync("testuser", "test@example.com");

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.True(result.IsEmailVerified);
        Assert.False(result.IsSystemAdmin);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedUser_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // Soft delete the user
        var user = await Context.Users.FindAsync(userId);
        user!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUserModel()
    {
        // Arrange
        var email = "test@example.com";
        await SeedTestUserAsync("testuser", email);

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUsername_ReturnsUserModel()
    {
        // Arrange
        var username = "testuser";
        await SeedTestUserAsync(username, "test@example.com");

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistentUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentuser";

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var userModel = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "newuser",
            Email = "newuser@example.com",
            PasswordHash = "hashedpassword",
            IsEmailVerified = false,
            IsSystemAdmin = false
        };

        // Act
        var result = await _repository.CreateAsync(userModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userModel.Username, result.Username);
        Assert.Equal(userModel.Email, result.Email);
        Assert.Equal(userModel.PasswordHash, result.PasswordHash);
        Assert.False(result.IsEmailVerified);
        Assert.False(result.IsSystemAdmin);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbUser = await Context.Users.FindAsync(result.Id);
        Assert.NotNull(dbUser);
        Assert.Equal(userModel.Username, dbUser.Username);
        Assert.Equal(userModel.Email, dbUser.Email);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = await SeedTestUserAsync("originaluser", "original@example.com");

        var updatedModel = new UserModel
        {
            Id = userId,
            Username = "updateduser",
            Email = "updated@example.com",
            PasswordHash = "updatedpassword",
            IsEmailVerified = true,
            IsSystemAdmin = true
        };

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updateduser", result.Username);
        Assert.Equal("updated@example.com", result.Email);
        Assert.Equal("updatedpassword", result.PasswordHash);
        Assert.True(result.IsEmailVerified);
        Assert.True(result.IsSystemAdmin);
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbUser = await Context.Users.FindAsync(userId);
        Assert.NotNull(dbUser);
        Assert.Equal("updateduser", dbUser.Username);
        Assert.Equal("updated@example.com", dbUser.Email);
        Assert.True(dbUser.IsEmailVerified);
        Assert.True(dbUser.IsSystemAdmin);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var userModel = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "nonexistent",
            Email = "nonexistent@example.com",
            PasswordHash = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(userModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // Act
        var result = await _repository.DeleteAsync(userId);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbUser = await Context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(dbUser);
        Assert.True(dbUser.IsDeleted);
        Assert.NotNull(dbUser.UpdatedAt);

        // Verify user is not returned by normal queries
        var userModel = await _repository.GetByIdAsync(userId);
        Assert.Null(userModel);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_WithUsers_ReturnsAllActiveUsers()
    {
        // Arrange
        var userIds = await SeedTestUsersAsync(3);

        // Soft delete one user
        var userToDelete = await Context.Users.FindAsync(userIds[1]);
        userToDelete!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var users = result.ToList();
        Assert.Equal(2, users.Count); // Only non-deleted users
        Assert.Contains(users, u => u.Username == "testuser0");
        Assert.Contains(users, u => u.Username == "testuser2");
        Assert.DoesNotContain(users, u => u.Username == "testuser1");
    }

    [Fact]
    public async Task GetAllAsync_NoUsers_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ThrowsException()
    {
        // Arrange
        await SeedTestUserAsync("duplicateuser", "user1@example.com");

        var duplicateUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "duplicateuser", // Same username
            Email = "user2@example.com",
            PasswordHash = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicateUser));
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        await SeedTestUserAsync("user1", "duplicate@example.com");

        var duplicateUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user2",
            Email = "duplicate@example.com", // Same email
            PasswordHash = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicateUser));
    }
}