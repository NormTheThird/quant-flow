namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for SubscriptionRepository using mocked dependencies
/// </summary>
public class SubscriptionRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<SubscriptionRepository>> _mockLogger;
    private readonly SubscriptionRepository _repository;

    public SubscriptionRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<SubscriptionRepository>>();
        _repository = new SubscriptionRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSubscription_ReturnsSubscriptionModel()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = subscriptionId,
                UserId = userId,
                Type = (int)SubscriptionType.Premium,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(330),
                IsActive = true,
                MaxPortfolios = 10,
                MaxAlgorithms = 50,
                MaxBacktestRuns = 500,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(subscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subscriptionId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(10, result.MaxPortfolios);
        Assert.Equal(50, result.MaxAlgorithms);
        Assert.Equal(500, result.MaxBacktestRuns);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscriptions = new List<SubscriptionEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(subscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserSubscriptions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = (int)SubscriptionType.Free,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(-30),
                IsActive = false,
                MaxPortfolios = 1,
                MaxAlgorithms = 5,
                MaxBacktestRuns = 10,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = (int)SubscriptionType.Premium,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(330),
                IsActive = true,
                MaxPortfolios = 10,
                MaxAlgorithms = 50,
                MaxBacktestRuns = 500,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                Type = (int)SubscriptionType.Basic,
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow.AddDays(345),
                IsActive = true,
                MaxPortfolios = 3,
                MaxAlgorithms = 15,
                MaxBacktestRuns = 100,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        var userSubscriptions = subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt);
        var mockDbSet = CreateMockDbSetWithAsync(userSubscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var subscriptionList = result.ToList();
        Assert.Equal(2, subscriptionList.Count);
        Assert.All(subscriptionList, s => Assert.Equal(userId, s.UserId));
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Free);
        Assert.DoesNotContain(subscriptionList, s => s.Type == SubscriptionType.Basic);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ExistingUser_ReturnsActiveSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = (int)SubscriptionType.Free,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(-30),
                IsActive = false, // Inactive
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = (int)SubscriptionType.Premium,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(330),
                IsActive = true, // Active
                MaxPortfolios = 10,
                MaxAlgorithms = 50,
                MaxBacktestRuns = 500,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        var activeSubscription = subscriptions
            .Where(s => s.UserId == userId && s.IsActive && !s.IsDeleted);
        var mockDbSet = CreateMockDbSetWithAsync(activeSubscription);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(10, result.MaxPortfolios);
        Assert.Equal(50, result.MaxAlgorithms);
        Assert.Equal(500, result.MaxBacktestRuns);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_NoActiveSubscription_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = (int)SubscriptionType.Free,
                IsActive = false, // All subscriptions inactive
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var activeSubscriptions = subscriptions
            .Where(s => s.UserId == userId && s.IsActive && !s.IsDeleted);
        var mockDbSet = CreateMockDbSetWithAsync(activeSubscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsSubscriptionsOfType()
    {
        // Arrange
        var subscriptionType = SubscriptionType.Premium;
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Premium,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Premium,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Basic,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var premiumSubscriptions = subscriptions
            .Where(s => s.Type == (int)subscriptionType && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt);
        var mockDbSet = CreateMockDbSetWithAsync(premiumSubscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByTypeAsync(subscriptionType);

        // Assert
        var subscriptionList = result.ToList();
        Assert.Equal(2, subscriptionList.Count);
        Assert.All(subscriptionList, s => Assert.Equal(SubscriptionType.Premium, s.Type));
        Assert.DoesNotContain(subscriptionList, s => s.Type == SubscriptionType.Basic);
    }

    [Fact]
    public async Task GetExpiredAsync_WithExpiredSubscriptions_ReturnsExpiredSubscriptions()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Premium,
                EndDate = now.AddDays(-10), // Expired
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Basic,
                EndDate = now.AddDays(-5), // Expired
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Free,
                EndDate = now.AddDays(10), // Not expired
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        var expiredSubscriptions = subscriptions
            .Where(s => s.EndDate < now && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.EndDate);
        var mockDbSet = CreateMockDbSetWithAsync(expiredSubscriptions);
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        var subscriptionList = result.ToList();
        Assert.Equal(2, subscriptionList.Count);
        Assert.All(subscriptionList, s => Assert.True(s.EndDate < DateTime.UtcNow));
        Assert.All(subscriptionList, s => Assert.True(s.IsActive));
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
        Assert.DoesNotContain(subscriptionList, s => s.Type == SubscriptionType.Free);
    }

    [Fact]
    public async Task CreateAsync_ValidSubscription_CallsAddAndSaveChanges()
    {
        // Arrange
        var subscriptionModel = new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            MaxPortfolios = 10,
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500
        };

        var mockDbSet = new Mock<DbSet<SubscriptionEntity>>();
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.CreateAsync(subscriptionModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subscriptionModel.UserId, result.UserId);
        Assert.Equal(subscriptionModel.Type, result.Type);
        Assert.Equal(subscriptionModel.StartDate, result.StartDate);
        Assert.Equal(subscriptionModel.EndDate, result.EndDate);
        Assert.Equal(subscriptionModel.IsActive, result.IsActive);
        Assert.Equal(subscriptionModel.MaxPortfolios, result.MaxPortfolios);
        Assert.Equal(subscriptionModel.MaxAlgorithms, result.MaxAlgorithms);
        Assert.Equal(subscriptionModel.MaxBacktestRuns, result.MaxBacktestRuns);

        mockDbSet.Verify(m => m.Add(It.IsAny<SubscriptionEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingSubscription_UpdatesAndSaves()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = new SubscriptionEntity
        {
            Id = subscriptionId,
            UserId = Guid.NewGuid(),
            Type = (int)SubscriptionType.Free,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(330),
            IsActive = false,
            MaxPortfolios = 1,
            MaxAlgorithms = 5,
            MaxBacktestRuns = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var updatedModel = new SubscriptionModel
        {
            Id = subscriptionId,
            UserId = existingSubscription.UserId,
            Type = SubscriptionType.Premium, // Upgraded
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true, // Activated
            MaxPortfolios = 10, // Increased limits
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500
        };

        var mockDbSet = new Mock<DbSet<SubscriptionEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingSubscription }, s => s.Id);

        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(10, result.MaxPortfolios);
        Assert.Equal(50, result.MaxAlgorithms);
        Assert.Equal(500, result.MaxBacktestRuns);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionModel = new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            MaxPortfolios = 10,
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500
        };

        var mockDbSet = new Mock<DbSet<SubscriptionEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<SubscriptionEntity>(), s => s.Id);

        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(subscriptionModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingSubscription_SoftDeletesSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = new SubscriptionEntity
        {
            Id = subscriptionId,
            UserId = Guid.NewGuid(),
            Type = (int)SubscriptionType.Premium,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<SubscriptionEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingSubscription }, s => s.Id);

        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(subscriptionId);

        // Assert
        Assert.True(result);
        Assert.True(existingSubscription.IsDeleted);
        Assert.NotNull(existingSubscription.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentSubscription_ReturnsFalse()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<SubscriptionEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<SubscriptionEntity>(), s => s.Id);

        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.DeleteAsync(subscriptionId);

        // Assert
        Assert.False(result);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WithSubscriptions_ReturnsAllActiveSubscriptions()
    {
        // Arrange
        var subscriptions = new List<SubscriptionEntity>
        {
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Premium,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = (int)SubscriptionType.Basic,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(subscriptions.OrderByDescending(s => s.CreatedAt));
        MockContext.Setup(c => c.Subscriptions).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var subscriptionList = result.ToList();
        Assert.Equal(2, subscriptionList.Count);
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
    }
}