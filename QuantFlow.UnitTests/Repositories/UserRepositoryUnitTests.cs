namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for UserRepository using mocked dependencies
/// </summary>
public class UserRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly UserRepository _repository;

    public UserRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsEmailVerified = true,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

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
        var users = new List<UserEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsDeleted = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(users.Where(u => !u.IsDeleted));
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

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
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = email,
                PasswordHash = "hashedpassword",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

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
        var users = new List<UserEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

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
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_CallsAddAndSaveChanges()
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

        var mockDbSet = new Mock<DbSet<UserEntity>>();
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.CreateAsync(userModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userModel.Username, result.Username);
        Assert.Equal(userModel.Email, result.Email);

        mockDbSet.Verify(m => m.Add(It.IsAny<UserEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Username = "originaluser",
            Email = "original@example.com",
            PasswordHash = "originalpassword",
            CreatedAt = DateTime.UtcNow
        };

        var updatedModel = new UserModel
        {
            Id = userId,
            Username = "updateduser",
            Email = "updated@example.com",
            PasswordHash = "updatedpassword",
            IsEmailVerified = true,
            IsSystemAdmin = false
        };

        var mockDbSet = new Mock<DbSet<UserEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingUser }, u => u.Id);

        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updateduser", result.Username);
        Assert.Equal("updated@example.com", result.Email);
        Assert.True(result.IsEmailVerified);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
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

        var mockDbSet = new Mock<DbSet<UserEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<UserEntity>(), u => u.Id);

        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(userModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_SoftDeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<UserEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingUser }, u => u.Id);

        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(userId);

        // Assert
        Assert.True(result);
        Assert.True(existingUser.IsDeleted);
        Assert.NotNull(existingUser.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<UserEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<UserEntity>(), u => u.Id);

        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.DeleteAsync(userId);

        // Assert
        Assert.False(result);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WithUsers_ReturnsAllActiveUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "activeuser1",
                Email = "active1@example.com",
                PasswordHash = "password",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "activeuser2",
                Email = "active2@example.com",
                PasswordHash = "password",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var userList = result.ToList();
        Assert.Equal(2, userList.Count);
        Assert.Contains(userList, u => u.Username == "activeuser1");
        Assert.Contains(userList, u => u.Username == "activeuser2");
    }

    [Fact]
    public async Task GetAllAsync_NoUsers_ReturnsEmptyCollection()
    {
        // Arrange
        var users = new List<UserEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(users);
        MockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }
}