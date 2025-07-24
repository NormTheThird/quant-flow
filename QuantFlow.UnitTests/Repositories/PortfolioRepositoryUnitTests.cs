namespace QuantFlow.UnitTests.Repositories;

/// <summary>
/// Unit tests for PortfolioRepository using mocked dependencies
/// </summary>
public class PortfolioRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<PortfolioRepository>> _mockLogger;
    private readonly PortfolioRepository _repository;

    public PortfolioRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<PortfolioRepository>>();
        _repository = new PortfolioRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPortfolio_ReturnsPortfolioModel()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var portfolios = new List<PortfolioEntity>
        {
            new PortfolioEntity
            {
                Id = portfolioId,
                Name = "Test Portfolio",
                Description = "Test Description",
                InitialBalance = 10000.0m,
                CurrentBalance = 12000.0m,
                Status = (int)PortfolioStatus.Active,
                UserId = userId,
                MaxPositionSizePercent = 15.0m,
                CommissionRate = 0.002m,
                AllowShortSelling = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(portfolios);
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(portfolioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(portfolioId, result.Id);
        Assert.Equal("Test Portfolio", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(10000.0m, result.InitialBalance);
        Assert.Equal(12000.0m, result.CurrentBalance);
        Assert.Equal(PortfolioStatus.Active, result.Status);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(15.0m, result.MaxPositionSizePercent);
        Assert.Equal(0.002m, result.CommissionRate);
        Assert.True(result.AllowShortSelling);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPortfolio_ReturnsNull()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var portfolios = new List<PortfolioEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(portfolios);
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(portfolioId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserPortfolios()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var portfolios = new List<PortfolioEntity>
        {
            new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio 1",
                UserId = userId,
                InitialBalance = 10000.0m,
                CurrentBalance = 10000.0m,
                Status = (int)PortfolioStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio 2",
                UserId = userId,
                InitialBalance = 5000.0m,
                CurrentBalance = 5000.0m,
                Status = (int)PortfolioStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = "Other User Portfolio",
                UserId = otherUserId,
                InitialBalance = 20000.0m,
                CurrentBalance = 20000.0m,
                Status = (int)PortfolioStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var userPortfolios = portfolios.Where(p => p.UserId == userId && !p.IsDeleted);
        var mockDbSet = CreateMockDbSetWithAsync(userPortfolios);
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var portfolioList = result.ToList();
        Assert.Equal(2, portfolioList.Count);
        Assert.All(portfolioList, p => Assert.Equal(userId, p.UserId));
        Assert.Contains(portfolioList, p => p.Name == "Portfolio 1");
        Assert.Contains(portfolioList, p => p.Name == "Portfolio 2");
    }

    [Fact]
    public async Task CreateAsync_ValidPortfolio_CallsAddAndSaveChanges()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Id = Guid.NewGuid(),
            Name = "New Portfolio",
            Description = "New Description",
            InitialBalance = 15000.0m,
            CurrentBalance = 15000.0m,
            Status = PortfolioStatus.Active,
            UserId = Guid.NewGuid(),
            MaxPositionSizePercent = 10.0m,
            CommissionRate = 0.001m,
            AllowShortSelling = false
        };

        var mockDbSet = new Mock<DbSet<PortfolioEntity>>();
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

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

        mockDbSet.Verify(m => m.Add(It.IsAny<PortfolioEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPortfolio_UpdatesAndSaves()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var existingPortfolio = new PortfolioEntity
        {
            Id = portfolioId,
            Name = "Original Name",
            Description = "Original Description",
            InitialBalance = 10000.0m,
            CurrentBalance = 10000.0m,
            Status = (int)PortfolioStatus.Active,
            UserId = Guid.NewGuid(),
            MaxPositionSizePercent = 10.0m,
            CommissionRate = 0.001m,
            AllowShortSelling = false,
            CreatedAt = DateTime.UtcNow
        };

        var updatedModel = new PortfolioModel
        {
            Id = portfolioId,
            Name = "Updated Name",
            Description = "Updated Description",
            InitialBalance = 10000.0m,
            CurrentBalance = 12000.0m,
            Status = PortfolioStatus.Paused,
            UserId = existingPortfolio.UserId,
            MaxPositionSizePercent = 15.0m,
            CommissionRate = 0.002m,
            AllowShortSelling = true
        };

        var mockDbSet = new Mock<DbSet<PortfolioEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingPortfolio }, p => p.Id);

        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(12000.0m, result.CurrentBalance);
        Assert.Equal(PortfolioStatus.Paused, result.Status);
        Assert.Equal(15.0m, result.MaxPositionSizePercent);
        Assert.Equal(0.002m, result.CommissionRate);
        Assert.True(result.AllowShortSelling);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
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

        var mockDbSet = new Mock<DbSet<PortfolioEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<PortfolioEntity>(), p => p.Id);

        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(portfolioModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingPortfolio_SoftDeletesPortfolio()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var existingPortfolio = new PortfolioEntity
        {
            Id = portfolioId,
            Name = "Test Portfolio",
            InitialBalance = 10000.0m,
            CurrentBalance = 10000.0m,
            Status = (int)PortfolioStatus.Active,
            UserId = Guid.NewGuid(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<PortfolioEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingPortfolio }, p => p.Id);

        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(portfolioId);

        // Assert
        Assert.True(result);
        Assert.True(existingPortfolio.IsDeleted);
        Assert.NotNull(existingPortfolio.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPortfolio_ReturnsFalse()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<PortfolioEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<PortfolioEntity>(), p => p.Id);

        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.DeleteAsync(portfolioId);

        // Assert
        Assert.False(result);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolios = new List<PortfolioEntity>
        {
            new PortfolioEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new PortfolioEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new PortfolioEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true }, // Deleted
            new PortfolioEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsDeleted = false } // Different user
        };

        var userPortfolios = portfolios.Where(p => p.UserId == userId && !p.IsDeleted);
        var mockDbSet = CreateMockDbSetWithAsync(userPortfolios);
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetAllAsync_WithPortfolios_ReturnsAllActivePortfolios()
    {
        // Arrange
        var portfolios = new List<PortfolioEntity>
        {
            new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio 1",
                UserId = Guid.NewGuid(),
                InitialBalance = 10000.0m,
                CurrentBalance = 10000.0m,
                Status = (int)PortfolioStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio 2",
                UserId = Guid.NewGuid(),
                InitialBalance = 5000.0m,
                CurrentBalance = 5000.0m,
                Status = (int)PortfolioStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(portfolios);
        MockContext.Setup(c => c.Portfolios).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var portfolioList = result.ToList();
        Assert.Equal(2, portfolioList.Count);
        Assert.Contains(portfolioList, p => p.Name == "Portfolio 1");
        Assert.Contains(portfolioList, p => p.Name == "Portfolio 2");
    }
}