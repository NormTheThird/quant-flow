namespace QuantFlow.Test.Integration.Repositories;

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
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscription = SubscriptionModelFixture.CreatePremiumSubscription(user.Id);
        var entity = subscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(subscription.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subscription.Id, result.Id);
        Assert.Equal(user.Id, result.UserId);
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

        // Act
        var result = await _repository.GetByIdAsync(subscriptionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedSubscription_ReturnsNull()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscription = SubscriptionModelFixture.CreateFreeSubscription(user.Id);
        var entity = subscription.ToEntity();
        entity.IsDeleted = true;
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(subscription.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserSubscriptions()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        var otherUser = UserModelFixture.CreateDefault("otheruser", "other@example.com");
        Context.Users.AddRange(user.ToEntity(), otherUser.ToEntity());
        await Context.SaveChangesAsync();

        var userSubscriptions = new[]
        {
            SubscriptionModelFixture.CreateFreeSubscription(user.Id),
            SubscriptionModelFixture.CreatePremiumSubscription(user.Id)
        };
        var otherUserSubscription = SubscriptionModelFixture.CreateBasicSubscription(otherUser.Id);

        // Make the first subscription inactive
        userSubscriptions[0].IsActive = false;
        userSubscriptions[0].EndDate = DateTime.UtcNow.AddDays(-1);

        var allSubscriptions = userSubscriptions.Concat(new[] { otherUserSubscription });
        var entities = allSubscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count); // Both user's subscriptions
        Assert.All(subscriptions, s => Assert.Equal(user.Id, s.UserId));
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Free);
        Assert.Contains(subscriptions, s => s.Type == SubscriptionType.Premium);
        Assert.DoesNotContain(subscriptions, s => s.Type == SubscriptionType.Basic);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ExistingUser_ReturnsActiveSubscription()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscriptions = SubscriptionModelFixture.CreateSubscriptionHistory(user.Id);
        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(SubscriptionType.Premium, result.Type);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_NoActiveSubscription_ReturnsNull()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var inactiveSubscription = SubscriptionModelFixture.CreateInactiveSubscription(user.Id);
        var entity = inactiveSubscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsSubscriptionsOfType()
    {
        // Arrange
        var premiumSubscriptions = new[]
        {
            SubscriptionModelFixture.CreatePremiumSubscription(),
            SubscriptionModelFixture.CreatePremiumSubscription()
        };
        var basicSubscription = SubscriptionModelFixture.CreateBasicSubscription();

        var allSubscriptions = premiumSubscriptions.Concat(new[] { basicSubscription });
        var entities = allSubscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

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
        var expiredSubscriptions = SubscriptionModelFixture.CreateExpiredSubscriptionBatch(2);
        var activeSubscription = SubscriptionModelFixture.CreatePremiumSubscription();

        var allSubscriptions = expiredSubscriptions.Concat(new[] { activeSubscription });
        var entities = allSubscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        var subscriptions = result.ToList();
        Assert.Equal(2, subscriptions.Count);
        Assert.All(subscriptions, s => Assert.True(s.EndDate < DateTime.UtcNow));
        Assert.All(subscriptions, s => Assert.True(s.IsActive));
        Assert.DoesNotContain(subscriptions, s => s.EndDate >= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_ValidSubscription_ReturnsCreatedSubscription()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscriptionModel = SubscriptionModelFixture.CreateCustomSubscription(
            userId: user.Id,
            type: SubscriptionType.Professional,
            maxPortfolios: 25,
            maxAlgorithms: 100,
            maxBacktestRuns: 1000
        );

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
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var originalSubscription = SubscriptionModelFixture.CreateFreeSubscription(user.Id);
        var entity = originalSubscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        var updatedModel = SubscriptionModelFixture.CreateCustomSubscription(
            userId: user.Id,
            type: SubscriptionType.Premium,
            maxPortfolios: 15,
            maxAlgorithms: 75,
            maxBacktestRuns: 750
        );
        updatedModel.Id = originalSubscription.Id;

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
        var dbSubscription = await Context.Subscriptions.FindAsync(originalSubscription.Id);
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
        var user = UserModelFixture.CreateDefault();
        var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription(user.Id);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(subscriptionModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingSubscription_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscription = SubscriptionModelFixture.CreateFreeSubscription(user.Id);
        var entity = subscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(subscription.Id);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbSubscription = await Context.Subscriptions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == subscription.Id);
        Assert.NotNull(dbSubscription);
        Assert.True(dbSubscription.IsDeleted);
        Assert.NotNull(dbSubscription.UpdatedAt);

        // Verify subscription is not returned by normal queries
        var subscriptionModel = await _repository.GetByIdAsync(subscription.Id);
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
        var subscriptions = new[]
        {
            SubscriptionModelFixture.CreatePremiumSubscription(),
            SubscriptionModelFixture.CreateBasicSubscription(),
            SubscriptionModelFixture.CreateEnterpriseSubscription()
        };

        // Soft delete one subscription
        subscriptions[2].IsDeleted = true;

        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var subscriptionList = result.ToList();
        Assert.Equal(2, subscriptionList.Count); // Only active subscriptions
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Premium);
        Assert.Contains(subscriptionList, s => s.Type == SubscriptionType.Basic);
        Assert.DoesNotContain(subscriptionList, s => s.Type == SubscriptionType.Enterprise);
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
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscriptions = new[]
        {
            SubscriptionModelFixture.CreatePremiumSubscription(user.Id),
            SubscriptionModelFixture.CreateBasicSubscription(user.Id)
        };

        // Soft delete one subscription
        subscriptions[1].IsDeleted = true;

        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        var subscriptionList = result.ToList();
        Assert.Single(subscriptionList); // Only active subscription
        Assert.Equal(SubscriptionType.Premium, subscriptionList[0].Type);
        Assert.False(subscriptionList[0].IsDeleted);
    }

    [Fact]
    public async Task GetExpiredAsync_NoExpiredSubscriptions_ReturnsEmptyCollection()
    {
        // Arrange
        var subscription = SubscriptionModelFixture.CreatePremiumSubscription(); // Active and not expired
        var entity = subscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTypeAsync_NoSubscriptionsOfType_ReturnsEmptyCollection()
    {
        // Arrange
        var subscription = SubscriptionModelFixture.CreateFreeSubscription();
        var entity = subscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(SubscriptionType.Enterprise);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_MultipleActiveSubscriptions_ReturnsFirstActive()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var subscriptions = new[]
        {
            SubscriptionModelFixture.CreateBasicSubscription(user.Id),
            SubscriptionModelFixture.CreatePremiumSubscription(user.Id)
        };

        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.True(result.IsActive);
        // Should return one of the active subscriptions (implementation dependent)
    }

    [Fact]
    public async Task GetExpiredAsync_MixedActiveInactiveExpired_ReturnsOnlyActiveExpired()
    {
        // Arrange
        var expiredActiveSubscription = SubscriptionModelFixture.CreateCustomSubscription(
            type: SubscriptionType.Premium,
            isActive: true,
            startDate: DateTime.UtcNow.AddDays(-60),
            endDate: DateTime.UtcNow.AddDays(-10) // Expired
        );

        var expiredInactiveSubscription = SubscriptionModelFixture.CreateCustomSubscription(
            type: SubscriptionType.Basic,
            isActive: false,
            startDate: DateTime.UtcNow.AddDays(-90),
            endDate: DateTime.UtcNow.AddDays(-20) // Expired
        );

        var activeNotExpiredSubscription = SubscriptionModelFixture.CreateCustomSubscription(
            type: SubscriptionType.Enterprise,
            isActive: true,
            startDate: DateTime.UtcNow.AddDays(-30),
            endDate: DateTime.UtcNow.AddDays(330) // Not expired
        );

        var subscriptions = new[] { expiredActiveSubscription, expiredInactiveSubscription, activeNotExpiredSubscription };
        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        var subscriptionList = result.ToList();
        Assert.Single(subscriptionList); // Only the expired but active subscription
        Assert.Equal(SubscriptionType.Premium, subscriptionList[0].Type);
        Assert.True(subscriptionList[0].IsActive);
        Assert.True(subscriptionList[0].EndDate < DateTime.UtcNow);
    }

    // TODO: Trey: The test is failing because EF Core's In-Memory database provider doesn't enforce foreign key
    // constraints. This is the same issue I mentioned earlier - the In-Memory provider is designed for simple
    // testing and doesn't validate referential integrity.
    //[Fact]
    //public async Task CreateAsync_SubscriptionForNonExistentUser_ThrowsDbUpdateException()
    //{
    //    // Arrange
    //    var subscriptionModel = SubscriptionModelFixture.CreatePremiumSubscription(); // Has a non-existent user ID

    //    // Act & Assert
    //    await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(subscriptionModel));
    //}

    [Fact]
    public async Task GetByTypeAsync_WithDeletedSubscriptions_ReturnsOnlyNonDeletedSubscriptions()
    {
        // Arrange
        var subscriptions = new[]
        {
            SubscriptionModelFixture.CreatePremiumSubscription(),
            SubscriptionModelFixture.CreatePremiumSubscription()
        };

        // Soft delete one subscription
        subscriptions[1].IsDeleted = true;

        var entities = subscriptions.Select(s => s.ToEntity());
        Context.Subscriptions.AddRange(entities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(SubscriptionType.Premium);

        // Assert
        var subscriptionList = result.ToList();
        Assert.Single(subscriptionList); // Only the non-deleted subscription
        Assert.Equal(subscriptions[0].Id, subscriptionList[0].Id);
        Assert.False(subscriptionList[0].IsDeleted);
    }

    [Fact]
    public async Task UpdateAsync_ChangeSubscriptionDates_UpdatesCorrectly()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        var originalSubscription = SubscriptionModelFixture.CreateBasicSubscription(user.Id);
        var entity = originalSubscription.ToEntity();
        Context.Subscriptions.Add(entity);
        await Context.SaveChangesAsync();

        var newStartDate = DateTime.UtcNow.AddDays(-15);
        var newEndDate = DateTime.UtcNow.AddDays(375);

        var updatedModel = SubscriptionModelFixture.CreateCustomSubscription(
            userId: user.Id,
            type: SubscriptionType.Basic,
            startDate: newStartDate,
            endDate: newEndDate
        );
        updatedModel.Id = originalSubscription.Id;

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newStartDate.Date, result.StartDate.Date);
        Assert.Equal(newEndDate.Date, result.EndDate.Date);

        // Verify in database
        var dbSubscription = await Context.Subscriptions.FindAsync(originalSubscription.Id);
        Assert.NotNull(dbSubscription);
        Assert.Equal(newStartDate.Date, dbSubscription.StartDate.Date);
        Assert.Equal(newEndDate.Date, dbSubscription.EndDate.Date);
    }
}