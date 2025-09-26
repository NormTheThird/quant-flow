//namespace QuantFlow.Test.Unit.Repositories;

///// <summary>
///// Unit tests for SubscriptionRepository using in-memory database
///// </summary>
//public class SubscriptionRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
//{
//    private readonly Mock<ILogger<SubscriptionRepository>> _mockLogger;
//    private readonly SubscriptionRepository _repository;

//    public SubscriptionRepositoryUnitTests()
//    {
//        _mockLogger = new Mock<ILogger<SubscriptionRepository>>();
//        _repository = new SubscriptionRepository(Context, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingSubscription_ReturnsSubscriptionModel()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription(userId);
//        var subscriptionEntity = subscriptionModel.ToEntity();

//        Context.Subscriptions.Add(subscriptionEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(subscriptionEntity.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(subscriptionEntity.Id, result.Id);
//        Assert.Equal(userId, result.UserId);
//        Assert.Equal(SubscriptionType.Premium, result.Type);
//        Assert.True(result.IsActive);
//        Assert.Equal(10, result.MaxPortfolios);
//        Assert.Equal(50, result.MaxAlgorithms);
//        Assert.Equal(500, result.MaxBacktestRuns);
//    }

//    [Fact]
//    public async Task GetByIdAsync_NonExistentSubscription_ReturnsNull()
//    {
//        // Arrange
//        var subscriptionId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetByIdAsync(subscriptionId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserSubscriptions()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var otherUserId = Guid.NewGuid();

//        var userSubscriptions = SubscriptionModelFixture.CreateSubscriptionHistory(userId);
//        var otherUserSubscription = SubscriptionModelFixture.CreateBasicSubscription(otherUserId);

//        var allSubscriptions = userSubscriptions.Concat(new[] { otherUserSubscription });
//        var subscriptionEntities = allSubscriptions.Select(s => s.ToEntity());

//        Context.Subscriptions.AddRange(subscriptionEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(userId);

//        // Assert
//        var subscriptionList = result.ToList();
//        Assert.Equal(3, subscriptionList.Count); // Should return all 3 subscriptions for the user
//        Assert.All(subscriptionList, s => Assert.Equal(userId, s.UserId));
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Free);

//        // Verify ordering (most recent first)
//        Assert.Equal(SubscriptionType.Premium, subscriptionList[0].Type); // Most recent should be first
//    }

//    [Fact]
//    public async Task GetActiveByUserIdAsync_ExistingUser_ReturnsActiveSubscription()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var subscriptions = SubscriptionModelFixture.CreateSubscriptionHistory(userId);
//        var subscriptionEntities = subscriptions.Select(s => s.ToEntity());

//        Context.Subscriptions.AddRange(subscriptionEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetActiveByUserIdAsync(userId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(userId, result.UserId);
//        Assert.Equal(SubscriptionType.Premium, result.Type); // Should return the active Premium subscription
//        Assert.True(result.IsActive);
//        Assert.Equal(10, result.MaxPortfolios);
//        Assert.Equal(50, result.MaxAlgorithms);
//        Assert.Equal(500, result.MaxBacktestRuns);
//    }

//    [Fact]
//    public async Task GetActiveByUserIdAsync_NoActiveSubscription_ReturnsNull()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var inactiveSubscription = SubscriptionModelFixture.CreateInactiveSubscription(userId);
//        var subscriptionEntity = inactiveSubscription.ToEntity();

//        Context.Subscriptions.Add(subscriptionEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetActiveByUserIdAsync(userId);

//        // Assert
//        Assert.Null(result); // Should return null since no active subscriptions
//    }

//    [Fact]
//    public async Task GetByTypeAsync_ExistingType_ReturnsSubscriptionsOfType()
//    {
//        // Arrange
//        var subscriptionType = SubscriptionType.Premium;
//        var premiumSubscriptions = SubscriptionModelFixture.CreateActiveSubscriptionsByType(subscriptionType, 2);
//        var basicSubscription = SubscriptionModelFixture.CreateBasicSubscription();

//        var allSubscriptions = premiumSubscriptions.Concat(new[] { basicSubscription });
//        var subscriptionEntities = allSubscriptions.Select(s => s.ToEntity());

//        Context.Subscriptions.AddRange(subscriptionEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByTypeAsync(subscriptionType);

//        // Assert
//        var subscriptionList = result.ToList();
//        Assert.Equal(2, subscriptionList.Count);
//        Assert.All(subscriptionList, s => Assert.Equal(SubscriptionType.Premium, s.Type));
//        Assert.DoesNotContain(subscriptionList, s => s.Type == SubscriptionType.Basic);
//    }

//    [Fact]
//    public async Task GetExpiredAsync_WithExpiredSubscriptions_ReturnsExpiredSubscriptions()
//    {
//        // Arrange
//        var expiredSubscriptions = SubscriptionModelFixture.CreateExpiredSubscriptionBatch(2);
//        var activeSubscription = SubscriptionModelFixture.CreatePremiumSubscription(); // Not expired

//        var allSubscriptions = expiredSubscriptions.Concat(new[] { activeSubscription });
//        var subscriptionEntities = allSubscriptions.Select(s => s.ToEntity());

//        Context.Subscriptions.AddRange(subscriptionEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetExpiredAsync();

//        // Assert
//        var subscriptionList = result.ToList();
//        Assert.Equal(2, subscriptionList.Count);
//        Assert.All(subscriptionList, s => Assert.True(s.EndDate < DateTime.UtcNow));
//        Assert.All(subscriptionList, s => Assert.True(s.IsActive)); // Still marked as active but expired by date
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
//        Assert.DoesNotContain(subscriptionList, s => s.EndDate > DateTime.UtcNow); // No non-expired subscriptions
//    }

//    [Fact]
//    public async Task CreateAsync_ValidSubscription_CallsAddAndSaveChanges()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription(userId);

//        // Act
//        var result = await _repository.CreateAsync(subscriptionModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(subscriptionModel.UserId, result.UserId);
//        Assert.Equal(subscriptionModel.Type, result.Type);
//        Assert.Equal(subscriptionModel.StartDate, result.StartDate);
//        Assert.Equal(subscriptionModel.EndDate, result.EndDate);
//        Assert.Equal(subscriptionModel.IsActive, result.IsActive);
//        Assert.Equal(subscriptionModel.MaxPortfolios, result.MaxPortfolios);
//        Assert.Equal(subscriptionModel.MaxAlgorithms, result.MaxAlgorithms);
//        Assert.Equal(subscriptionModel.MaxBacktestRuns, result.MaxBacktestRuns);

//        // Verify it was actually saved to the database
//        var savedEntity = await Context.Subscriptions.FindAsync(result.Id);
//        Assert.NotNull(savedEntity);
//        Assert.Equal(result.UserId, savedEntity.UserId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ExistingSubscription_UpdatesAndSaves()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var originalSubscription = SubscriptionModelFixture.CreateFreeSubscription(userId);
//        var subscriptionEntity = originalSubscription.ToEntity();

//        Context.Subscriptions.Add(subscriptionEntity);
//        await Context.SaveChangesAsync();

//        // Get the saved subscription and create update model
//        var savedSubscription = await _repository.GetByIdAsync(subscriptionEntity.Id);
//        var updatedModel = SubscriptionModelFixture.CreateSubscriptionForUpdate(
//            savedSubscription.Id,
//            userId,
//            SubscriptionType.Premium,
//            true
//        );

//        // Act
//        var result = await _repository.UpdateAsync(updatedModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(SubscriptionType.Premium, result.Type);
//        Assert.True(result.IsActive);
//        Assert.Equal(10, result.MaxPortfolios);
//        Assert.Equal(50, result.MaxAlgorithms);
//        Assert.Equal(500, result.MaxBacktestRuns);

//        // Verify the changes were persisted
//        var updatedEntity = await Context.Subscriptions.FindAsync(result.Id);
//        Assert.NotNull(updatedEntity);
//        Assert.Equal((int)SubscriptionType.Premium, updatedEntity.Type);
//        Assert.True(updatedEntity.IsActive);
//    }

//    [Fact]
//    public async Task UpdateAsync_NonExistentSubscription_ThrowsNotFoundException()
//    {
//        // Arrange
//        var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription();

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(subscriptionModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingSubscription_SoftDeletesSubscription()
//    {
//        // Arrange
//        var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription();
//        var subscriptionEntity = subscriptionModel.ToEntity();

//        Context.Subscriptions.Add(subscriptionEntity);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(subscriptionEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete was applied
//        var deletedEntity = await Context.Subscriptions.FindAsync(subscriptionEntity.Id);
//        Assert.NotNull(deletedEntity);
//        Assert.True(deletedEntity.IsDeleted);
//        Assert.NotNull(deletedEntity.UpdatedAt);
//    }

//    [Fact]
//    public async Task DeleteAsync_NonExistentSubscription_ReturnsFalse()
//    {
//        // Arrange
//        var subscriptionId = Guid.NewGuid();

//        // Act
//        var result = await _repository.DeleteAsync(subscriptionId);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public async Task GetAllAsync_WithSubscriptions_ReturnsAllActiveSubscriptions()
//    {
//        // Arrange
//        var subscriptions = new List<SubscriptionModel>
//        {
//            SubscriptionModelFixture.CreatePremiumSubscription(),
//            SubscriptionModelFixture.CreateBasicSubscription()
//        };
//        var subscriptionEntities = subscriptions.Select(s => s.ToEntity());

//        Context.Subscriptions.AddRange(subscriptionEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var subscriptionList = result.ToList();
//        Assert.Equal(2, subscriptionList.Count);
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
//        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
//    }

//    [Fact]
//    public async Task GetAllAsync_NoSubscriptions_ReturnsEmptyCollection()
//    {
//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        Assert.Empty(result);
//    }
//}