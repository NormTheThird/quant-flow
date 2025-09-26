//namespace QuantFlow.Test.Integration.Repositories;

///// <summary>
///// Integration tests for UserRepository with in-memory database
///// </summary>
//public class UserRepositoryIntegrationTests : BaseRepositoryIntegrationTest
//{
//    private readonly UserRepository _repository;

//    public UserRepositoryIntegrationTests()
//    {
//        var logger = Substitute.For<ILogger<UserRepository>>();
//        _repository = new UserRepository(Context, logger);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingUser_ReturnsUserModel()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("testuser", "test@example.com");
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
//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        // Soft delete the user
//        var user = await Context.Users.FindAsync(userEntity.Id);
//        user!.IsDeleted = true;
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(userEntity.Id);

//        // Assert
//        Assert.Null(result);
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
//    public async Task CreateAsync_ValidUser_ReturnsCreatedUser()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("newuser", "newuser@example.com");
//        // Override some properties for this test
//        userModel.IsEmailVerified = false;
//        userModel.IsSystemAdmin = false;

//        // Act
//        var result = await _repository.CreateAsync(userModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(userModel.Username, result.Username);
//        Assert.Equal(userModel.Email, result.Email);
//        Assert.Equal(userModel.PasswordHash, result.PasswordHash);
//        Assert.False(result.IsEmailVerified);
//        Assert.False(result.IsSystemAdmin);
//        Assert.True(result.CreatedAt > DateTime.MinValue);

//        // Verify in database
//        var dbUser = await Context.Users.FindAsync(result.Id);
//        Assert.NotNull(dbUser);
//        Assert.Equal(userModel.Username, dbUser.Username);
//        Assert.Equal(userModel.Email, dbUser.Email);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
//    {
//        // Arrange
//        var originalUserModel = UserModelFixture.CreateDefault("originaluser", "original@example.com");
//        var userEntity = originalUserModel.ToEntity();
//        Context.Users.Add(userEntity);
//        await Context.SaveChangesAsync();

//        var updatedModel = UserModelFixture.CreateForUpdate(
//            userEntity.Id,
//            "updateduser",
//            "updated@example.com",
//            "updatedpassword",
//            isEmailVerified: true,
//            isSystemAdmin: true
//        );

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("updateduser", result.Username);
//        Assert.Equal("updated@example.com", result.Email);
//        Assert.Equal("updatedpassword", result.PasswordHash);
//        Assert.True(result.IsEmailVerified);
//        Assert.True(result.IsSystemAdmin);
//        Assert.NotNull(result.UpdatedAt);

//        // Verify in database
//        var dbUser = await Context.Users.FindAsync(userEntity.Id);
//        Assert.NotNull(dbUser);
//        Assert.Equal("updateduser", dbUser.Username);
//        Assert.Equal("updated@example.com", dbUser.Email);
//        Assert.True(dbUser.IsEmailVerified);
//        Assert.True(dbUser.IsSystemAdmin);
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
//    public async Task DeleteAsync_ExistingUser_ReturnsTrueAndSoftDeletes()
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

//        // Verify soft delete
//        var dbUser = await Context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userEntity.Id);
//        Assert.NotNull(dbUser);
//        Assert.True(dbUser.IsDeleted);
//        Assert.NotNull(dbUser.UpdatedAt);

//        // Verify user is not returned by normal queries
//        var userModelResult = await _repository.GetByIdAsync(userEntity.Id);
//        Assert.Null(userModelResult);
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
//        var userModels = UserModelFixture.CreateBatch(3);
//        var userEntities = userModels.Select(u => u.ToEntity()).ToList();
//        Context.Users.AddRange(userEntities);
//        await Context.SaveChangesAsync();

//        // Soft delete one user
//        var userToDelete = userEntities[1];
//        userToDelete.IsDeleted = true;
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var users = result.ToList();
//        Assert.Equal(2, users.Count); // Only non-deleted users
//        Assert.Contains(users, u => u.Username == userModels[0].Username);
//        Assert.Contains(users, u => u.Username == userModels[2].Username);
//        Assert.DoesNotContain(users, u => u.Username == userModels[1].Username);
//    }

//    [Fact]
//    public async Task GetAllAsync_NoUsers_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task CreateAsync_DuplicateUsername_ThrowsException()
//    {
//        // Arrange
//        var existingUserModel = UserModelFixture.CreateDefault("duplicateuser", "user1@example.com");
//        var existingUserEntity = existingUserModel.ToEntity();
//        Context.Users.Add(existingUserEntity);
//        await Context.SaveChangesAsync();

//        var duplicateUser = UserModelFixture.CreateDefault("duplicateuser", "user2@example.com");

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateAsync(duplicateUser));
//        Assert.Contains("Username 'duplicateuser' already exists", exception.Message);
//    }

//    [Fact]
//    public async Task CreateAsync_DuplicateEmail_ThrowsException()
//    {
//        // Arrange
//        var existingUserModel = UserModelFixture.CreateDefault("user1", "duplicate@example.com");
//        var existingUserEntity = existingUserModel.ToEntity();
//        Context.Users.Add(existingUserEntity);
//        await Context.SaveChangesAsync();

//        var duplicateUser = UserModelFixture.CreateDefault("user2", "duplicate@example.com");

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateAsync(duplicateUser));
//        Assert.Contains("Email 'duplicate@example.com' already exists", exception.Message);
//    }
//}