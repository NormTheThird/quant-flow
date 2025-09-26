//using QuantFlow.Test.Shared.Fixtures;

//namespace QuantFlow.Test.Integration.Repositories;

///// <summary>
///// Integration tests for PortfolioRepository with in-memory database
///// </summary>
//public class PortfolioRepositoryIntegrationTests : BaseRepositoryIntegrationTest
//{
//    private readonly PortfolioRepository _repository;

//    public PortfolioRepositoryIntegrationTests()
//    {
//        var logger = Substitute.For<ILogger<PortfolioRepository>>();
//        _repository = new PortfolioRepository(Context, logger);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingPortfolio_ReturnsPortfolioModel()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id, "Test Portfolio");
//        var entity = portfolio.ToEntity();
//        Context.Portfolios.Add(entity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(portfolio.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(portfolio.Id, result.Id);
//        Assert.Equal("Test Portfolio", result.Name);
//        Assert.Equal("A diversified cryptocurrency trading portfolio", result.Description);
//        Assert.Equal(10000.0m, result.InitialBalance);
//        Assert.Equal(10000.0m, result.CurrentBalance);
//        Assert.Equal(PortfolioStatus.Active, result.Status);
//        Assert.Equal(user.Id, result.UserId);
//        Assert.Equal(15.0m, result.MaxPositionSizePercent);
//        Assert.Equal(0.001m, result.CommissionRate);
//        Assert.False(result.AllowShortSelling);
//    }

//    [Fact]
//    public async Task GetByIdAsync_NonExistentPortfolio_ReturnsNull()
//    {
//        // Arrange
//        var portfolioId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetByIdAsync(portfolioId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByIdAsync_DeletedPortfolio_ReturnsNull()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id);
//        var entity = portfolio.ToEntity();
//        entity.IsDeleted = true;
//        Context.Portfolios.Add(entity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(portfolio.Id);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserPortfolios()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        var otherUser = UserModelFixture.CreateDefault("otheruser", "other@example.com");
//        Context.Users.AddRange(user.ToEntity(), otherUser.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolio1 = PortfolioModelFixture.CreateActivePortfolio(user.Id, "Portfolio 1");
//        var portfolio2 = PortfolioModelFixture.CreateProfitablePortfolio(user.Id, "Portfolio 2");
//        var otherPortfolio = PortfolioModelFixture.CreateActivePortfolio(otherUser.Id, "Other Portfolio");

//        var portfolios = new[] { portfolio1, portfolio2, otherPortfolio };
//        var entities = portfolios.Select(p => p.ToEntity());
//        Context.Portfolios.AddRange(entities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(user.Id);

//        // Assert
//        var userPortfolios = result.ToList();
//        Assert.Equal(2, userPortfolios.Count);
//        Assert.All(userPortfolios, p => Assert.Equal(user.Id, p.UserId));
//        Assert.Contains(userPortfolios, p => p.Name == "Portfolio 1");
//        Assert.Contains(userPortfolios, p => p.Name == "Portfolio 2");
//        Assert.DoesNotContain(userPortfolios, p => p.Name == "Other Portfolio");
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_UserWithDeletedPortfolios_ReturnsOnlyActivePortfolios()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var activePortfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id, "Active Portfolio");
//        var deletedPortfolio = PortfolioModelFixture.CreatePausedPortfolio(user.Id, "Deleted Portfolio");
//        deletedPortfolio.IsDeleted = true;

//        var portfolios = new[] { activePortfolio, deletedPortfolio };
//        var entities = portfolios.Select(p => p.ToEntity());
//        Context.Portfolios.AddRange(entities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(user.Id);

//        // Assert
//        var userPortfolios = result.ToList();
//        Assert.Single(userPortfolios);
//        Assert.Equal("Active Portfolio", userPortfolios[0].Name);
//        Assert.False(userPortfolios[0].IsDeleted);
//    }

//    [Fact]
//    public async Task CreateAsync_ValidPortfolio_ReturnsCreatedPortfolio()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(user.Id, "New Portfolio");
//        portfolioModel.Description = "New portfolio description";
//        portfolioModel.InitialBalance = 15000.0m;
//        portfolioModel.CurrentBalance = 15000.0m;
//        portfolioModel.MaxPositionSizePercent = 20.0m;
//        portfolioModel.CommissionRate = 0.002m;
//        portfolioModel.AllowShortSelling = true;

//        // Act
//        var result = await _repository.CreateAsync(portfolioModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(portfolioModel.Name, result.Name);
//        Assert.Equal(portfolioModel.Description, result.Description);
//        Assert.Equal(portfolioModel.InitialBalance, result.InitialBalance);
//        Assert.Equal(portfolioModel.CurrentBalance, result.CurrentBalance);
//        Assert.Equal(portfolioModel.Status, result.Status);
//        Assert.Equal(portfolioModel.UserId, result.UserId);
//        Assert.Equal(portfolioModel.MaxPositionSizePercent, result.MaxPositionSizePercent);
//        Assert.Equal(portfolioModel.CommissionRate, result.CommissionRate);
//        Assert.Equal(portfolioModel.AllowShortSelling, result.AllowShortSelling);
//        Assert.True(result.CreatedAt > DateTime.MinValue);

//        // Verify in database
//        var dbPortfolio = await Context.Portfolios.FindAsync(result.Id);
//        Assert.NotNull(dbPortfolio);
//        Assert.Equal(portfolioModel.Name, dbPortfolio.Name);
//        Assert.Equal(portfolioModel.Description, dbPortfolio.Description);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingPortfolio_ReturnsUpdatedPortfolio()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var originalPortfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id, "Original Portfolio");
//        var entity = originalPortfolio.ToEntity();
//        Context.Portfolios.Add(entity);
//        await Context.SaveChangesAsync();

//        var updatedModel = PortfolioModelFixture.CreateProfitablePortfolio(user.Id, "Updated Portfolio");
//        updatedModel.Id = originalPortfolio.Id;
//        updatedModel.Description = "Updated description";
//        updatedModel.CurrentBalance = 15000.0m;
//        updatedModel.Status = PortfolioStatus.Paused;
//        updatedModel.MaxPositionSizePercent = 25.0m;
//        updatedModel.CommissionRate = 0.003m;

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("Updated Portfolio", result.Name);
//        Assert.Equal("Updated description", result.Description);
//        Assert.Equal(15000.0m, result.CurrentBalance);
//        Assert.Equal(PortfolioStatus.Paused, result.Status);
//        Assert.Equal(25.0m, result.MaxPositionSizePercent);
//        Assert.Equal(0.003m, result.CommissionRate);
//        Assert.True(result.AllowShortSelling);
//        Assert.NotNull(result.UpdatedAt);

//        // Verify in database
//        var dbPortfolio = await Context.Portfolios.FindAsync(originalPortfolio.Id);
//        Assert.NotNull(dbPortfolio);
//        Assert.Equal("Updated Portfolio", dbPortfolio.Name);
//        Assert.Equal("Updated description", dbPortfolio.Description);
//        Assert.Equal(15000.0m, dbPortfolio.CurrentBalance);
//        Assert.Equal((int)PortfolioStatus.Paused, dbPortfolio.Status);
//    }

//    [Fact]
//    public async Task UpdateAsync_NonExistentPortfolio_ThrowsNotFoundException()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(user.Id, "Nonexistent Portfolio");

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(portfolioModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingPortfolio_ReturnsTrueAndSoftDeletes()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id);
//        var entity = portfolio.ToEntity();
//        Context.Portfolios.Add(entity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(portfolio.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete
//        var dbPortfolio = await Context.Portfolios.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == portfolio.Id);
//        Assert.NotNull(dbPortfolio);
//        Assert.True(dbPortfolio.IsDeleted);
//        Assert.NotNull(dbPortfolio.UpdatedAt);

//        // Verify portfolio is not returned by normal queries
//        var portfolioModel = await _repository.GetByIdAsync(portfolio.Id);
//        Assert.Null(portfolioModel);
//    }

//    [Fact]
//    public async Task DeleteAsync_NonExistentPortfolio_ReturnsFalse()
//    {
//        // Arrange
//        var portfolioId = Guid.NewGuid();

//        // Act
//        var result = await _repository.DeleteAsync(portfolioId);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        var otherUser = UserModelFixture.CreateDefault("otheruser", "other@example.com");
//        Context.Users.AddRange(user.ToEntity(), otherUser.ToEntity());
//        await Context.SaveChangesAsync();

//        var userPortfolios = new[]
//        {
//            PortfolioModelFixture.CreateActivePortfolio(user.Id, "Portfolio 1"),
//            PortfolioModelFixture.CreateProfitablePortfolio(user.Id, "Portfolio 2"),
//            PortfolioModelFixture.CreatePausedPortfolio(user.Id, "Deleted Portfolio")
//        };

//        // Soft delete one portfolio
//        userPortfolios[2].IsDeleted = true;

//        var otherUserPortfolio = PortfolioModelFixture.CreateActivePortfolio(otherUser.Id, "Other User Portfolio");

//        var allPortfolios = userPortfolios.Concat(new[] { otherUserPortfolio });
//        var entities = allPortfolios.Select(p => p.ToEntity());
//        Context.Portfolios.AddRange(entities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.CountByUserIdAsync(user.Id);

//        // Assert
//        Assert.Equal(2, result); // Only active portfolios for the user
//    }

//    [Fact]
//    public async Task CountByUserIdAsync_UserWithNoPortfolios_ReturnsZero()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.CountByUserIdAsync(user.Id);

//        // Assert
//        Assert.Equal(0, result);
//    }

//    [Fact]
//    public async Task GetAllAsync_WithPortfolios_ReturnsAllActivePortfolios()
//    {
//        // Arrange
//        var user1 = UserModelFixture.CreateDefault("user1", "user1@example.com");
//        var user2 = UserModelFixture.CreateDefault("user2", "user2@example.com");
//        Context.Users.AddRange(user1.ToEntity(), user2.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolios = new[]
//        {
//            PortfolioModelFixture.CreateActivePortfolio(user1.Id, "Portfolio 1"),
//            PortfolioModelFixture.CreateProfitablePortfolio(user2.Id, "Portfolio 2"),
//            PortfolioModelFixture.CreateArchivedPortfolio(user1.Id, "Deleted Portfolio")
//        };

//        // Soft delete one portfolio
//        portfolios[2].IsDeleted = true;

//        var entities = portfolios.Select(p => p.ToEntity());
//        Context.Portfolios.AddRange(entities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var allPortfolios = result.ToList();
//        Assert.Equal(2, allPortfolios.Count); // Only active portfolios
//        Assert.Contains(allPortfolios, p => p.Name == "Portfolio 1");
//        Assert.Contains(allPortfolios, p => p.Name == "Portfolio 2");
//        Assert.DoesNotContain(allPortfolios, p => p.Name == "Deleted Portfolio");
//        Assert.All(allPortfolios, p => Assert.False(p.IsDeleted));
//    }

//    [Fact]
//    public async Task GetAllAsync_NoPortfolios_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }

//    // TODO: Trey: The test is failing because EF Core's In-Memory database provider doesn't enforce foreign key
//    // constraints. This is the same issue I mentioned earlier - the In-Memory provider is designed for simple
//    // testing and doesn't validate referential integrity.
//    //[Fact]
//    //public async Task CreateAsync_DuplicatePortfolioNameForSameUser_ThrowsException()
//    //{
//    //    // Arrange
//    //    var userId = await SeedTestUserAsync();
//    //    var existingPortfolio = PortfolioModelFixture.CreateActivePortfolio(userId, "Duplicate Name");
//    //    var entity = existingPortfolio.ToEntity();
//    //    Context.Portfolios.Add(entity);
//    //    await Context.SaveChangesAsync();

//    //    var duplicatePortfolio = PortfolioModelFixture.CreateProfitablePortfolio(userId, "Duplicate Name");

//    //    // Act & Assert
//    //    await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicatePortfolio));
//    //}

//    [Fact]
//    public async Task CreateAsync_SamePortfolioNameForDifferentUsers_Succeeds()
//    {
//        // Arrange
//        var user1 = UserModelFixture.CreateDefault("user1", "user1@example.com");
//        var user2 = UserModelFixture.CreateDefault("user2", "user2@example.com");
//        Context.Users.AddRange(user1.ToEntity(), user2.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolio1 = PortfolioModelFixture.CreateActivePortfolio(user1.Id, "Same Name");
//        var entity1 = portfolio1.ToEntity();
//        Context.Portfolios.Add(entity1);
//        await Context.SaveChangesAsync();

//        var portfolio2 = PortfolioModelFixture.CreateProfitablePortfolio(user2.Id, "Same Name");

//        // Act
//        var result = await _repository.CreateAsync(portfolio2);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("Same Name", result.Name);
//        Assert.Equal(user2.Id, result.UserId);
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_MultiplePortfolioTypes_ReturnsAllUserPortfolios()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolios = new[]
//        {
//            PortfolioModelFixture.CreateActivePortfolio(user.Id, "Active Portfolio"),
//            PortfolioModelFixture.CreateProfitablePortfolio(user.Id, "Profitable Portfolio"),
//            PortfolioModelFixture.CreateLosingPortfolio(user.Id, "Losing Portfolio"),
//            PortfolioModelFixture.CreatePausedPortfolio(user.Id, "Paused Portfolio")
//        };

//        var entities = portfolios.Select(p => p.ToEntity());
//        Context.Portfolios.AddRange(entities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(user.Id);

//        // Assert
//        var userPortfolios = result.ToList();
//        Assert.Equal(4, userPortfolios.Count);
//        Assert.All(userPortfolios, p => Assert.Equal(user.Id, p.UserId));
//        Assert.Contains(userPortfolios, p => p.Status == PortfolioStatus.Active);
//        Assert.Contains(userPortfolios, p => p.Status == PortfolioStatus.Paused);
//        Assert.Contains(userPortfolios, p => p.CurrentBalance > p.InitialBalance); // Profitable
//        Assert.Contains(userPortfolios, p => p.CurrentBalance < p.InitialBalance); // Losing
//    }

//    [Fact]
//    public async Task CreateAsync_PortfolioWithCustomSettings_ReturnsPortfolioWithCorrectSettings()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var portfolioModel = PortfolioModelFixture.CreateCustomPortfolio(
//            userId: user.Id,
//            name: "Custom Portfolio",
//            description: "Custom portfolio description",
//            initialBalance: 25000.0m,
//            currentBalance: 25000.0m,
//            status: PortfolioStatus.Paused,
//            maxPositionSizePercent: 30.0m,
//            commissionRate: 0.0025m,
//            allowShortSelling: true
//        );

//        // Act
//        var result = await _repository.CreateAsync(portfolioModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("Custom Portfolio", result.Name);
//        Assert.Equal(25000.0m, result.InitialBalance);
//        Assert.Equal(25000.0m, result.CurrentBalance);
//        Assert.Equal(30.0m, result.MaxPositionSizePercent);
//        Assert.Equal(0.0025m, result.CommissionRate);
//        Assert.True(result.AllowShortSelling);
//        Assert.Equal(PortfolioStatus.Paused, result.Status);
//        Assert.Equal(user.Id, result.UserId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ChangePortfolioBalance_UpdatesCorrectly()
//    {
//        // Arrange
//        var user = UserModelFixture.CreateDefault();
//        Context.Users.Add(user.ToEntity());
//        await Context.SaveChangesAsync();

//        var originalPortfolio = PortfolioModelFixture.CreateActivePortfolio(user.Id);
//        var entity = originalPortfolio.ToEntity();
//        Context.Portfolios.Add(entity);
//        await Context.SaveChangesAsync();

//        var updatedModel = PortfolioModelFixture.CreateCustomPortfolio(
//            userId: user.Id,
//            name: originalPortfolio.Name,
//            initialBalance: originalPortfolio.InitialBalance,
//            currentBalance: 18000.0m // Changed balance
//        );
//        updatedModel.Id = originalPortfolio.Id;

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(originalPortfolio.InitialBalance, result.InitialBalance); // Unchanged
//        Assert.Equal(18000.0m, result.CurrentBalance); // Updated
//        Assert.NotNull(result.UpdatedAt);

//        // Verify in database
//        var dbPortfolio = await Context.Portfolios.FindAsync(originalPortfolio.Id);
//        Assert.NotNull(dbPortfolio);
//        Assert.Equal(18000.0m, dbPortfolio.CurrentBalance);
//    }
//}