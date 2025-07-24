namespace QuantFlow.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for SubscriptionRepository with in-memory database
/// </summary>
public class SubscriptionRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly SubscriptionRepository _repository;

    public SubscriptionRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<SubscriptionRepository>>();
        _repository = new SubscriptionRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSubscription_ReturnsSubscriptionModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Premium);

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subscriptionId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(5, result.MaxPortfolios);
        Assert.Equal(20, result.MaxAlgorithms);
        Assert.Equal(100, result.MaxBacktestRuns);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedSubscription_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId);

        // Soft delete the subscription
        var subscription = await Context.Subscriptions.FindAsync(subscriptionId);
        subscription!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserSubscriptions()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("otheruser", "other@example.com");

        // Create multiple subscriptions for the user
        var oldSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Free);
        var currentSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Premium);
        var otherUserSubscriptionId = await SeedTestSubscriptionAsync(otherUserId, SubscriptionType.Basic);

        // Make the old subscription inactive
        var oldSubscription = await Context.Subscriptions.FindAsync(oldSubscriptionId);
        oldSubscription!.IsActive = false;
        oldSubscription.EndDate = DateTime.UtcNow.AddDays(-1);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count); // Both user's subscriptions
        Assert.All(subscriptions, s => Assert.Equal(userId, s.UserId));
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Free);
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Premium);
        Assert.DoesNotContain(subscriptions, s => s.Type == SubscriptionType.Basic);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ExistingUser_ReturnsActiveSubscription()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // Create inactive and active subscriptions
        var inactiveSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Free);
        var activeSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Premium);

        // Make the first subscription inactive
        var inactiveSubscription = await Context.Subscriptions.FindAsync(inactiveSubscriptionId);
        inactiveSubscription!.IsActive = false;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activeSubscriptionId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_NoActiveSubscription_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId);

        // Make the subscription inactive
        var subscription = await Context.Subscriptions.FindAsync(subscriptionId);
        subscription!.IsActive = false;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsSubscriptionsOfType()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");
        var user3Id = await SeedTestUserAsync("user3", "user3@example.com");

        var premiumSubscription1Id = await SeedTestSubscriptionAsync(user1Id, SubscriptionType.Premium);
        var premiumSubscription2Id = await SeedTestSubscriptionAsync(user2Id, SubscriptionType.Premium);
        var basicSubscriptionId = await SeedTestSubscriptionAsync(user3Id, SubscriptionType.Basic);

        // Act
        var result = await _repository.GetByTypeAsync(SubscriptionType.Premium);

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count);
        Assert.All(subscriptions, s => Assert.Equal(SubscriptionType.Premium, s.Type));
        Assert.DoesNotContain(subscriptions, s => s.Type == SubscriptionType.Basic);
    }

    [Fact]
    public async Task GetExpiredAsync_WithExpiredSubscriptions_ReturnsExpiredSubscriptions()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");
        var user3Id = await SeedTestUserAsync("user3", "user3@example.com");

        var expiredSubscription1Id = await SeedTestSubscriptionAsync(user1Id, SubscriptionType.Premium);
        var expiredSubscription2Id = await SeedTestSubscriptionAsync(user2Id, SubscriptionType.Basic);
        var activeSubscriptionId = await SeedTestSubscriptionAsync(user3Id, SubscriptionType.Free);

        // Make two subscriptions expired but still active (should be deactivated)
        var expiredSubscription1 = await Context.Subscriptions.FindAsync(expiredSubscription1Id);
        expiredSubscription1!.EndDate = DateTime.UtcNow.AddDays(-10);

        var expiredSubscription2 = await Context.Subscriptions.FindAsync(expiredSubscription2Id);
        expiredSubscription2!.EndDate = DateTime.UtcNow.AddDays(-5);

        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count);
        Assert.All(subscriptions, s => Assert.True(s.EndDate < DateTime.UtcNow));
        Assert.All(subscriptions, s => Assert.True(s.IsActive)); // Still marked as active but expired
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Basic);
        Assert.DoesNotContain(subscriptions, s => s.Type == SubscriptionType.Free);
    }

    [Fact]
    public async Task CreateAsync_ValidSubscription_ReturnsCreatedSubscription()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionModel = new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = SubscriptionType.Professional,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            MaxPortfolios = 25,
            MaxAlgorithms = 100,
            MaxBacktestRuns = 1000
        };

        // Act
        var result = await _repository.CreateAsync(subscriptionModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subscriptionModel.UserId, result.UserId);
        Assert.Equal(subscriptionModel.Type, result.Type);
        Assert.Equal(subscriptionModel.StartDate.Date, result.StartDate.Date);
        Assert.Equal(subscriptionModel.EndDate.Date, result.EndDate.Date);
        Assert.Equal(subscriptionModel.IsActive, result.IsActive);
        Assert.Equal(subscriptionModel.MaxPortfolios, result.MaxPortfolios);
        Assert.Equal(subscriptionModel.MaxAlgorithms, result.MaxAlgorithms);
        Assert.Equal(subscriptionModel.MaxBacktestRuns, result.MaxBacktestRuns);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbSubscription = await Context.Subscriptions.FindAsync(result.Id);
        Assert.NotNull(dbSubscription);
        Assert.Equal((int)subscriptionModel.Type, dbSubscription.Type);
        Assert.Equal(subscriptionModel.MaxPortfolios, dbSubscription.MaxPortfolios);
    }

    [Fact]
    public async Task UpdateAsync_ExistingSubscription_ReturnsUpdatedSubscription()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Free);

        var updatedModel = new SubscriptionModel
        {
            Id = subscriptionId,
            UserId = userId,
            Type = SubscriptionType.Premium, // Upgrade
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            MaxPortfolios = 15, // Increased limits
            MaxAlgorithms = 75,
            MaxBacktestRuns = 750
        };

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(15, result.MaxPortfolios);
        Assert.Equal(75, result.MaxAlgorithms);
        Assert.Equal(750, result.MaxBacktestRuns);
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbSubscription = await Context.Subscriptions.FindAsync(subscriptionId);
        Assert.NotNull(dbSubscription);
        Assert.Equal((int)SubscriptionType.Premium, dbSubscription.Type);
        Assert.Equal(15, dbSubscription.MaxPortfolios);
        Assert.Equal(75, dbSubscription.MaxAlgorithms);
        Assert.Equal(750, dbSubscription.MaxBacktestRuns);
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

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(subscriptionModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingSubscription_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId);

        // Act
        var result = await _repository.DeleteAsync(subscriptionId);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbSubscription = await Context.Subscriptions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == subscriptionId);
        Assert.NotNull(dbSubscription);
        Assert.True(dbSubscription.IsDeleted);
        Assert.NotNull(dbSubscription.UpdatedAt);

        // Verify subscription is not returned by normal queries
        var subscriptionModel = await _repository.GetByIdAsync(subscriptionId);
        Assert.Null(subscriptionModel);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentSubscription_ReturnsFalse()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(subscriptionId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_WithSubscriptions_ReturnsAllActiveSubscriptions()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        var subscription1Id = await SeedTestSubscriptionAsync(user1Id, SubscriptionType.Premium);
        var subscription2Id = await SeedTestSubscriptionAsync(user2Id, SubscriptionType.Basic);
        var deletedSubscriptionId = await SeedTestSubscriptionAsync(user1Id, SubscriptionType.Professional);

        // Soft delete one subscription
        var deletedSubscription = await Context.Subscriptions.FindAsync(deletedSubscriptionId);
        deletedSubscription!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count); // Only active subscriptions
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Basic);
        Assert.DoesNotContain(subscriptions, s => s.Type == SubscriptionType.Professional);
    }

    [Fact]
    public async Task GetAllAsync_NoSubscriptions_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserWithDeletedSubscriptions_ReturnsOnlyActiveSubscriptions()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        var activeSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Premium);
        var deletedSubscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Basic);

        // Soft delete one subscription
        var deletedSubscription = await Context.Subscriptions.FindAsync(deletedSubscriptionId);
        deletedSubscription!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var subscriptions = result.ToList();
        Assert.Single(subscriptions); // Only active subscription
        Assert.Equal(SubscriptionType.Premium, subscriptions[0].Type);
        Assert.False(subscriptions[0].IsDeleted);
    }

    [Fact]
    public async Task GetExpiredAsync_NoExpiredSubscriptions_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        await SeedTestSubscriptionAsync(userId, SubscriptionType.Premium); // Active and not expired

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTypeAsync_NoSubscriptionsOfType_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        await SeedTestSubscriptionAsync(userId, SubscriptionType.Free);

        // Act
        var result = await _repository.GetByTypeAsync(SubscriptionType.Enterprise);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_MultipleActiveSubscriptions_ReturnsFirstActive()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // This scenario shouldn't happen in real business logic, but tests repository behavior
        var subscription1 = new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = (int)SubscriptionType.Basic,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(300),
            IsActive = true,
            MaxPortfolios = 3,
            MaxAlgorithms = 15,
            MaxBacktestRuns = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            CreatedBy = "test"
        };

        var subscription2 = new SubscriptionEntity
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
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "test"
        };

        Context.Subscriptions.AddRange(subscription1, subscription2);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.True(result.IsActive);
        // Should return one of the active subscriptions (implementation dependent)
    }

    [Fact]
    public async Task GetExpiredAsync_MixedActiveInactiveExpired_ReturnsOnlyActiveExpired()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");
        var user3Id = await SeedTestUserAsync("user3", "user3@example.com");

        // Create expired but active subscription (should be returned)
        var expiredActiveSubscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user1Id,
            Type = (int)SubscriptionType.Premium,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(-10), // Expired
            IsActive = true, // But still marked as active
            MaxPortfolios = 10,
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500,
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            CreatedBy = "test"
        };

        // Create expired and inactive subscription (should NOT be returned)
        var expiredInactiveSubscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user2Id,
            Type = (int)SubscriptionType.Basic,
            StartDate = DateTime.UtcNow.AddDays(-90),
            EndDate = DateTime.UtcNow.AddDays(-20), // Expired
            IsActive = false, // Already deactivated
            MaxPortfolios = 3,
            MaxAlgorithms = 15,
            MaxBacktestRuns = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            CreatedBy = "test"
        };

        // Create active and not expired subscription (should NOT be returned)
        var activeNotExpiredSubscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user3Id,
            Type = (int)SubscriptionType.Professional,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(330), // Not expired
            IsActive = true,
            MaxPortfolios = 25,
            MaxAlgorithms = 100,
            MaxBacktestRuns = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "test"
        };

        Context.Subscriptions.AddRange(
            expiredActiveSubscription,
            expiredInactiveSubscription,
            activeNotExpiredSubscription
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        var subscriptions = result.ToList();
        Assert.Single(subscriptions); // Only the expired but active subscription
        Assert.Equal(SubscriptionType.Premium, subscriptions[0].Type);
        Assert.True(subscriptions[0].IsActive);
        Assert.True(subscriptions[0].EndDate < DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_SubscriptionForNonExistentUser_ThrowsDbUpdateException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var subscriptionModel = new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = nonExistentUserId, // User doesn't exist
            Type = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            MaxPortfolios = 10,
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(subscriptionModel));
    }

    [Fact]
    public async Task GetByTypeAsync_WithDeletedSubscriptions_ReturnsOnlyNonDeletedSubscriptions()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        var activeSubscriptionId = await SeedTestSubscriptionAsync(user1Id, SubscriptionType.Premium);
        var deletedSubscriptionId = await SeedTestSubscriptionAsync(user2Id, SubscriptionType.Premium);

        // Soft delete one subscription
        var deletedSubscription = await Context.Subscriptions.FindAsync(deletedSubscriptionId);
        deletedSubscription!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(SubscriptionType.Premium);

        // Assert
        var subscriptions = result.ToList();
        Assert.Single(subscriptions); // Only the non-deleted subscription
        Assert.Equal(activeSubscriptionId, subscriptions[0].Id);
        Assert.False(subscriptions[0].IsDeleted);
    }

    [Fact]
    public async Task UpdateAsync_ChangeSubscriptionDates_UpdatesCorrectly()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var subscriptionId = await SeedTestSubscriptionAsync(userId, SubscriptionType.Basic);

        var newStartDate = DateTime.UtcNow.AddDays(-15);
        var newEndDate = DateTime.UtcNow.AddDays(375); // Extend subscription

        var updatedModel = new SubscriptionModel
        {
            Id = subscriptionId,
            UserId = userId,
            Type = SubscriptionType.Basic,
            StartDate = newStartDate,
            EndDate = newEndDate,
            IsActive = true,
            MaxPortfolios = 3,
            MaxAlgorithms = 15,
            MaxBacktestRuns = 100
        };

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newStartDate.Date, result.StartDate.Date);
        Assert.Equal(newEndDate.Date, result.EndDate.Date);

        // Verify in database
        var dbSubscription = await Context.Subscriptions.FindAsync(subscriptionId);
        Assert.NotNull(dbSubscription);
        Assert.Equal(newStartDate.Date, dbSubscription.StartDate.Date);
        Assert.Equal(newEndDate.Date, dbSubscription.EndDate.Date);
    }
}