namespace QuantFlow.Test.Integration.Repositories;

/// <summary>
/// Integration tests for PortfolioRepository with in-memory database
/// </summary>
public class PortfolioRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly PortfolioRepository _repository;

    public PortfolioRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<PortfolioRepository>>();
        _repository = new PortfolioRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPortfolio_ReturnsPortfolioModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId, "Test Portfolio");

        // Act
        var result = await _repository.GetByIdAsync(portfolioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(portfolioId, result.Id);
        Assert.Equal("Test Portfolio", result.Name);
        Assert.Equal("Test portfolio description", result.Description);
        Assert.Equal(10000.0m, result.InitialBalance);
        Assert.Equal(12000.0m, result.CurrentBalance);
        Assert.Equal(PortfolioStatus.Active, result.Status);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(10.0m, result.MaxPositionSizePercent);
        Assert.Equal(0.001m, result.CommissionRate);
        Assert.False(result.AllowShortSelling);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPortfolio_ReturnsNull()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(portfolioId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedPortfolio_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        // Soft delete the portfolio
        var portfolio = await Context.Portfolios.FindAsync(portfolioId);
        portfolio!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(portfolioId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserPortfolios()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("otheruser", "other@example.com");

        var portfolio1Id = await SeedTestPortfolioAsync(userId, "Portfolio 1");
        var portfolio2Id = await SeedTestPortfolioAsync(userId, "Portfolio 2");
        var otherPortfolioId = await SeedTestPortfolioAsync(otherUserId, "Other Portfolio");

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var portfolios = result.ToList();
        Assert.Equal(2, portfolios.Count);
        Assert.All(portfolios, p => Assert.Equal(userId, p.UserId));
        Assert.Contains(portfolios, p => p.Name == "Portfolio 1");
        Assert.Contains(portfolios, p => p.Name == "Portfolio 2");
        Assert.DoesNotContain(portfolios, p => p.Name == "Other Portfolio");
    }

    [Fact]
    public async Task GetByUserIdAsync_UserWithDeletedPortfolios_ReturnsOnlyActivePortfolios()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var activePortfolioId = await SeedTestPortfolioAsync(userId, "Active Portfolio");
        var deletedPortfolioId = await SeedTestPortfolioAsync(userId, "Deleted Portfolio");

        // Soft delete one portfolio
        var deletedPortfolio = await Context.Portfolios.FindAsync(deletedPortfolioId);
        deletedPortfolio!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var portfolios = result.ToList();
        Assert.Single(portfolios);
        Assert.Equal("Active Portfolio", portfolios[0].Name);
    }

    [Fact]
    public async Task CreateAsync_ValidPortfolio_ReturnsCreatedPortfolio()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioModel = new PortfolioModel
        {
            Id = Guid.NewGuid(),
            Name = "New Portfolio",
            Description = "New portfolio description",
            InitialBalance = 15000.0m,
            CurrentBalance = 15000.0m,
            Status = PortfolioStatus.Active,
            UserId = userId,
            MaxPositionSizePercent = 20.0m,
            CommissionRate = 0.002m,
            AllowShortSelling = true
        };

        // Act
        var result = await _repository.CreateAsync(portfolioModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(portfolioModel.Name, result.Name);
        Assert.Equal(portfolioModel.Description, result.Description);
        Assert.Equal(portfolioModel.InitialBalance, result.InitialBalance);
        Assert.Equal(portfolioModel.CurrentBalance, result.CurrentBalance);
        Assert.Equal(portfolioModel.Status, result.Status);
        Assert.Equal(portfolioModel.UserId, result.UserId);
        Assert.Equal(portfolioModel.MaxPositionSizePercent, result.MaxPositionSizePercent);
        Assert.Equal(portfolioModel.CommissionRate, result.CommissionRate);
        Assert.Equal(portfolioModel.AllowShortSelling, result.AllowShortSelling);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbPortfolio = await Context.Portfolios.FindAsync(result.Id);
        Assert.NotNull(dbPortfolio);
        Assert.Equal(portfolioModel.Name, dbPortfolio.Name);
        Assert.Equal(portfolioModel.Description, dbPortfolio.Description);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPortfolio_ReturnsUpdatedPortfolio()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId, "Original Portfolio");

        var updatedModel = new PortfolioModel
        {
            Id = portfolioId,
            Name = "Updated Portfolio",
            Description = "Updated description",
            InitialBalance = 10000.0m,
            CurrentBalance = 15000.0m,
            Status = PortfolioStatus.Paused,
            UserId = userId,
            MaxPositionSizePercent = 25.0m,
            CommissionRate = 0.003m,
            AllowShortSelling = true
        };

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Portfolio", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(15000.0m, result.CurrentBalance);
        Assert.Equal(PortfolioStatus.Paused, result.Status);
        Assert.Equal(25.0m, result.MaxPositionSizePercent);
        Assert.Equal(0.003m, result.CommissionRate);
        Assert.True(result.AllowShortSelling);
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbPortfolio = await Context.Portfolios.FindAsync(portfolioId);
        Assert.NotNull(dbPortfolio);
        Assert.Equal("Updated Portfolio", dbPortfolio.Name);
        Assert.Equal("Updated description", dbPortfolio.Description);
        Assert.Equal(15000.0m, dbPortfolio.CurrentBalance);
        Assert.Equal((int)PortfolioStatus.Paused, dbPortfolio.Status);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPortfolio_ThrowsNotFoundException()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Id = Guid.NewGuid(),
            Name = "Nonexistent Portfolio",
            InitialBalance = 10000.0m,
            CurrentBalance = 10000.0m,
            Status = PortfolioStatus.Active,
            UserId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(portfolioModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingPortfolio_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        // Act
        var result = await _repository.DeleteAsync(portfolioId);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbPortfolio = await Context.Portfolios.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == portfolioId);
        Assert.NotNull(dbPortfolio);
        Assert.True(dbPortfolio.IsDeleted);
        Assert.NotNull(dbPortfolio.UpdatedAt);

        // Verify portfolio is not returned by normal queries
        var portfolioModel = await _repository.GetByIdAsync(portfolioId);
        Assert.Null(portfolioModel);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPortfolio_ReturnsFalse()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(portfolioId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("otheruser", "other@example.com");

        // Create portfolios for the user
        await SeedTestPortfolioAsync(userId, "Portfolio 1");
        await SeedTestPortfolioAsync(userId, "Portfolio 2");
        var deletedPortfolioId = await SeedTestPortfolioAsync(userId, "Deleted Portfolio");

        // Create portfolio for other user
        await SeedTestPortfolioAsync(otherUserId, "Other User Portfolio");

        // Soft delete one portfolio
        var deletedPortfolio = await Context.Portfolios.FindAsync(deletedPortfolioId);
        deletedPortfolio!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result); // Only active portfolios for the user
    }

    [Fact]
    public async Task CountByUserIdAsync_UserWithNoPortfolios_ReturnsZero()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetAllAsync_WithPortfolios_ReturnsAllActivePortfolios()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        var portfolio1Id = await SeedTestPortfolioAsync(user1Id, "Portfolio 1");
        var portfolio2Id = await SeedTestPortfolioAsync(user2Id, "Portfolio 2");
        var deletedPortfolioId = await SeedTestPortfolioAsync(user1Id, "Deleted Portfolio");

        // Soft delete one portfolio
        var deletedPortfolio = await Context.Portfolios.FindAsync(deletedPortfolioId);
        deletedPortfolio!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var portfolios = result.ToList();
        Assert.Equal(2, portfolios.Count); // Only active portfolios
        Assert.Contains(portfolios, p => p.Name == "Portfolio 1");
        Assert.Contains(portfolios, p => p.Name == "Portfolio 2");
        Assert.DoesNotContain(portfolios, p => p.Name == "Deleted Portfolio");
    }

    [Fact]
    public async Task GetAllAsync_NoPortfolios_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_DuplicatePortfolioNameForSameUser_ThrowsException()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        await SeedTestPortfolioAsync(userId, "Duplicate Name");

        var duplicatePortfolio = new PortfolioModel
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Name", // Same name for same user
            InitialBalance = 10000.0m,
            CurrentBalance = 10000.0m,
            Status = PortfolioStatus.Active,
            UserId = userId
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(duplicatePortfolio));
    }

    [Fact]
    public async Task CreateAsync_SamePortfolioNameForDifferentUsers_Succeeds()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        await SeedTestPortfolioAsync(user1Id, "Same Name");

        var portfolioForUser2 = new PortfolioModel
        {
            Id = Guid.NewGuid(),
            Name = "Same Name", // Same name but different user
            InitialBalance = 10000.0m,
            CurrentBalance = 10000.0m,
            Status = PortfolioStatus.Active,
            UserId = user2Id
        };

        // Act
        var result = await _repository.CreateAsync(portfolioForUser2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Same Name", result.Name);
        Assert.Equal(user2Id, result.UserId);
    }
}