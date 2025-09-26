//using QuantFlow.Test.Shared.Fixtures;
//using QuantFlow.Data.SQLServer.Extensions;
//using Microsoft.Extensions.Logging;
//using Moq;

//namespace QuantFlow.Test.Unit.Repositories;

///// <summary>
///// Unit tests for PortfolioRepository using in-memory database
///// </summary>
//public class PortfolioRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
//{
//    private readonly Mock<ILogger<PortfolioRepository>> _mockLogger;
//    private readonly PortfolioRepository _repository;

//    public PortfolioRepositoryUnitTests()
//    {
//        _mockLogger = new Mock<ILogger<PortfolioRepository>>();
//        _repository = new PortfolioRepository(Context, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingPortfolio_ReturnsPortfolioModel()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userId, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();

//        Context.Portfolios.Add(portfolioEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(portfolioEntity.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(portfolioEntity.Id, result.Id);
//        Assert.Equal("Test Portfolio", result.Name);
//        Assert.Equal("A diversified cryptocurrency trading portfolio", result.Description);
//        Assert.Equal(10000.0m, result.InitialBalance);
//        Assert.Equal(10000.0m, result.CurrentBalance);
//        Assert.Equal(PortfolioStatus.Active, result.Status);
//        Assert.Equal(userId, result.UserId);
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
//    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserPortfolios()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var otherUserId = Guid.NewGuid();

//        var userPortfolios = PortfolioModelFixture.CreateUserPortfolioBatch(userId, 2);
//        var otherUserPortfolio = PortfolioModelFixture.CreateActivePortfolio(otherUserId, "Other User Portfolio");

//        var allPortfolios = userPortfolios.Concat(new[] { otherUserPortfolio });
//        var portfolioEntities = allPortfolios.Select(p => p.ToEntity());

//        Context.Portfolios.AddRange(portfolioEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(userId);

//        // Assert
//        var portfolioList = result.ToList();
//        Assert.Equal(2, portfolioList.Count);
//        Assert.All(portfolioList, p => Assert.Equal(userId, p.UserId));
//        Assert.Contains(portfolioList, p => p.Name.Contains("Conservative Portfolio"));
//        Assert.Contains(portfolioList, p => p.Name.Contains("Balanced Portfolio"));
//        Assert.DoesNotContain(portfolioList, p => p.Name == "Other User Portfolio");
//    }

//    [Fact]
//    public async Task CreateAsync_ValidPortfolio_CallsAddAndSaveChanges()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var portfolioModel = PortfolioModelFixture.CreateCustomPortfolio(
//            userId: userId,
//            name: "New Portfolio",
//            description: "New Description",
//            initialBalance: 15000.0m,
//            currentBalance: 15000.0m,
//            maxPositionSizePercent: 10.0m,
//            commissionRate: 0.001m,
//            allowShortSelling: false
//        );

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

//        // Verify it was actually saved to the database
//        var savedEntity = await Context.Portfolios.FindAsync(result.Id);
//        Assert.NotNull(savedEntity);
//        Assert.Equal(result.Name, savedEntity.Name);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingPortfolio_UpdatesAndSaves()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var originalPortfolio = PortfolioModelFixture.CreateActivePortfolio(userId, "Original Name");
//        var portfolioEntity = originalPortfolio.ToEntity();

//        Context.Portfolios.Add(portfolioEntity);
//        await Context.SaveChangesAsync();

//        // Get the saved portfolio and create update model
//        var savedPortfolio = await _repository.GetByIdAsync(portfolioEntity.Id);
//        var updatedModel = PortfolioModelFixture.CreatePortfolioForUpdate(
//            savedPortfolio.Id,
//            userId,
//            "Updated Name",
//            "Updated Description",
//            12000.0m,
//            PortfolioStatus.Paused,
//            15.0m,
//            0.002m,
//            true
//        );

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("Updated Name", result.Name);
//        Assert.Equal("Updated Description", result.Description);
//        Assert.Equal(12000.0m, result.CurrentBalance);
//        Assert.Equal(PortfolioStatus.Paused, result.Status);
//        Assert.Equal(15.0m, result.MaxPositionSizePercent);
//        Assert.Equal(0.002m, result.CommissionRate);
//        Assert.True(result.AllowShortSelling);

//        // Verify the changes were persisted
//        var updatedEntity = await Context.Portfolios.FindAsync(result.Id);
//        Assert.NotNull(updatedEntity);
//        Assert.Equal("Updated Name", updatedEntity.Name);
//        Assert.Equal((int)PortfolioStatus.Paused, updatedEntity.Status);
//    }

//    [Fact]
//    public async Task UpdateAsync_NonExistentPortfolio_ThrowsNotFoundException()
//    {
//        // Arrange
//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio();

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(portfolioModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingPortfolio_SoftDeletesPortfolio()
//    {
//        // Arrange
//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio();
//        var portfolioEntity = portfolioModel.ToEntity();

//        Context.Portfolios.Add(portfolioEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(portfolioEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete was applied
//        var deletedEntity = await Context.Portfolios.FindAsync(portfolioEntity.Id);
//        Assert.NotNull(deletedEntity);
//        Assert.True(deletedEntity.IsDeleted);
//        Assert.NotNull(deletedEntity.UpdatedAt);
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
//        var userId = Guid.NewGuid();
//        var otherUserId = Guid.NewGuid();

//        var userPortfolios = PortfolioModelFixture.CreateUserPortfolioBatch(userId, 2);
//        var deletedPortfolio = PortfolioModelFixture.CreateDeletedPortfolio(userId);
//        var otherUserPortfolio = PortfolioModelFixture.CreateActivePortfolio(otherUserId);

//        // Mark the deleted portfolio as actually deleted
//        var deletedEntity = deletedPortfolio.ToEntity();
//        deletedEntity.IsDeleted = true;

//        var allPortfolios = userPortfolios.Select(p => p.ToEntity())
//            .Concat(new[] { deletedEntity, otherUserPortfolio.ToEntity() });

//        Context.Portfolios.AddRange(allPortfolios);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.CountByUserIdAsync(userId);

//        // Assert
//        Assert.Equal(2, result); // Should only count non-deleted portfolios for the user
//    }

//    [Fact]
//    public async Task GetAllAsync_WithPortfolios_ReturnsAllActivePortfolios()
//    {
//        // Arrange
//        var portfolios = new List<PortfolioModel>
//        {
//            PortfolioModelFixture.CreateActivePortfolio(name: "Portfolio 1"),
//            PortfolioModelFixture.CreateProfitablePortfolio(name: "Portfolio 2")
//        };
//        var portfolioEntities = portfolios.Select(p => p.ToEntity());

//        Context.Portfolios.AddRange(portfolioEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var portfolioList = result.ToList();
//        Assert.Equal(2, portfolioList.Count);
//        Assert.Contains(portfolioList, p => p.Name == "Portfolio 1");
//        Assert.Contains(portfolioList, p => p.Name == "Portfolio 2");
//    }

//    [Fact]
//    public async Task GetAllAsync_NoPortfolios_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_WithDeletedPortfolios_ExcludesDeletedPortfolios()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var activePortfolio = PortfolioModelFixture.CreateActivePortfolio(userId, "Active Portfolio");
//        var deletedPortfolio = PortfolioModelFixture.CreateDeletedPortfolio(userId, "Deleted Portfolio");

//        // Mark as deleted
//        var deletedEntity = deletedPortfolio.ToEntity();
//        deletedEntity.IsDeleted = true;

//        Context.Portfolios.AddRange(new[] { activePortfolio.ToEntity(), deletedEntity });
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(userId);

//        // Assert
//        var portfolioList = result.ToList();
//        Assert.Single(portfolioList);
//        Assert.Equal("Active Portfolio", portfolioList[0].Name);
//        Assert.DoesNotContain(portfolioList, p => p.Name == "Deleted Portfolio");
//    }
//}