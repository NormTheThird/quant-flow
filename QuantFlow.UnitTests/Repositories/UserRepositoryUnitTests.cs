//namespace QuantFlow.Test.Unit.Repositories;

///// <summary>
///// Unit tests for UserRepository using in-memory database
///// </summary>
//public class UserRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
//{
//    private readonly Mock<ILogger<UserRepository>> _mockLogger;
//    private readonly UserRepository _repository;

//    public UserRepositoryUnitTests()
//    {
//        _mockLogger = new Mock<ILogger<UserRepository>>();
//        _repository = new UserRepository(Context, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingUser_ReturnsUserModel()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateVerifiedUser("testuser", "test@example.com");
//        var userEntity = userModel.ToEntity();

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(userEntity.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(userEntity.Id, result.Id);
//        Assert.Equal("testuser", result.Username);
//        Assert.Equal("test@example.com", result.Email);
//        Assert.True(result.IsEmailVerified);
//        Assert.False(result.IsSystemAdmin);
//    }

//    [Fact]
//    public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetByIdAsync(userId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByIdAsync_DeletedUser_ReturnsNull()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("testuser", "test@example.com");
//        var userEntity = userModel.ToEntity();
//        userEntity.IsDeleted = true; // Mark as deleted

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(userEntity.Id);

//        // Assert
//        Assert.Null(result); // Should return null for deleted users
//    }

//    [Fact]
//    public async Task GetByEmailAsync_ExistingEmail_ReturnsUserModel()
//    {
//        // Arrange
//        var email = "test@example.com";
//        var userModel = UserModelFixture.CreateDefault("testuser", email);
//        var userEntity = userModel.ToEntity();

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByEmailAsync(email);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(email, result.Email);
//        Assert.Equal("testuser", result.Username);
//    }

//    [Fact]
//    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
//    {
//        // Arrange
//        var email = "nonexistent@example.com";

//        // Act
//        var result = await _repository.GetByEmailAsync(email);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByUsernameAsync_ExistingUsername_ReturnsUserModel()
//    {
//        // Arrange
//        var username = "testuser";
//        var userModel = UserModelFixture.CreateDefault(username, "test@example.com");
//        var userEntity = userModel.ToEntity();

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUsernameAsync(username);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(username, result.Username);
//        Assert.Equal("test@example.com", result.Email);
//    }

//    [Fact]
//    public async Task GetByUsernameAsync_NonExistentUsername_ReturnsNull()
//    {
//        // Arrange
//        var username = "nonexistentuser";

//        // Act
//        var result = await _repository.GetByUsernameAsync(username);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task CreateAsync_ValidUser_CallsAddAndSaveChanges()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateNewUser("newuser", "newuser@example.com");

//        // Act
//        var result = await _repository.CreateAsync(userModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(userModel.Username, result.Username);
//        Assert.Equal(userModel.Email, result.Email);
//        Assert.Equal(userModel.IsEmailVerified, result.IsEmailVerified);

//        // Verify it was actually saved to the database
//        var savedEntity = await Context.Users.FindAsync(result.Id);
//        Assert.NotNull(savedEntity);
//        Assert.Equal(result.Username, savedEntity.Username);
//        Assert.Equal(result.Email, savedEntity.Email);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingUser_UpdatesAndSaves()
//    {
//        // Arrange
//        var originalUser = UserModelFixture.CreateDefault("originaluser", "original@example.com");
//        var userEntity = originalUser.ToEntity();

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Get the saved user and modify it
//        var savedUser = await _repository.GetByIdAsync(userEntity.Id);
//        var updatedModel = UserModelFixture.CreateForUpdate(savedUser.Id, "updateduser", "updated@example.com");

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("updateduser", result.Username);
//        Assert.Equal("updated@example.com", result.Email);
//        Assert.True(result.IsEmailVerified);

//        // Verify the changes were persisted
//        var updatedEntity = await Context.Users.FindAsync(result.Id);
//        Assert.NotNull(updatedEntity);
//        Assert.Equal("updateduser", updatedEntity.Username);
//        Assert.Equal("updated@example.com", updatedEntity.Email);
//    }

//    [Fact]
//    public async Task UpdateAsync_NonExistentUser_ThrowsNotFoundException()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("nonexistent", "nonexistent@example.com");

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(userModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingUser_SoftDeletesUser()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("testuser", "test@example.com");
//        var userEntity = userModel.ToEntity();

//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(userEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete was applied
//        var deletedEntity = await Context.Users.FindAsync(userEntity.Id);
//        Assert.NotNull(deletedEntity);
//        Assert.True(deletedEntity.IsDeleted);
//        Assert.NotNull(deletedEntity.UpdatedAt);
//    }

//    [Fact]
//    public async Task DeleteAsync_NonExistentUser_ReturnsFalse()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();

//        // Act
//        var result = await _repository.DeleteAsync(userId);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public async Task GetAllAsync_WithUsers_ReturnsAllActiveUsers()
//    {
//        // Arrange
//        var users = UserModelFixture.CreateBatch(2, "activeuser");
//        var userEntities = users.Select(u => u.ToEntity());

//        Context.Users.AddRange(userEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var userList = result.ToList();
//        Assert.Equal(2, userList.Count);
//        Assert.Contains(userList, u => u.Username == "activeuser1");
//        Assert.Contains(userList, u => u.Username == "activeuser2");
//    }

//    [Fact]
//    public async Task GetAllAsync_NoUsers_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }
//}