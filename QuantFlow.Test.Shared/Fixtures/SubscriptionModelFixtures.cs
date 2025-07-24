namespace QuantFlow.Test.Shared.Fixtures;

/// <summary>
/// Fixture class for creating SubscriptionModel test data
/// </summary>
public static class SubscriptionModelFixture
{
    /// <summary>
    /// Creates a default Free subscription model for testing
    /// </summary>
    public static SubscriptionModel CreateFreeSubscription(Guid? userId = null)
    {
        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = SubscriptionType.Free,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(335), // ~1 year from start
            IsActive = true,
            MaxPortfolios = 1,
            MaxAlgorithms = 3,
            MaxBacktestRuns = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default Basic subscription model for testing
    /// </summary>
    public static SubscriptionModel CreateBasicSubscription(Guid? userId = null)
    {
        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = SubscriptionType.Basic,
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow.AddDays(350), // ~1 year from start
            IsActive = true,
            MaxPortfolios = 3,
            MaxAlgorithms = 15,
            MaxBacktestRuns = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default Premium subscription model for testing
    /// </summary>
    public static SubscriptionModel CreatePremiumSubscription(Guid? userId = null)
    {
        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(358), // ~1 year from start
            IsActive = true,
            MaxPortfolios = 10,
            MaxAlgorithms = 50,
            MaxBacktestRuns = 500,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a default Enterprise subscription model for testing
    /// </summary>
    public static SubscriptionModel CreateEnterpriseSubscription(Guid? userId = null)
    {
        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = SubscriptionType.Enterprise,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(364), // ~1 year from start
            IsActive = true,
            MaxPortfolios = 50,
            MaxAlgorithms = 200,
            MaxBacktestRuns = 2000,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates a custom subscription model with specific parameters
    /// </summary>
    public static SubscriptionModel CreateCustomSubscription(Guid? userId = null, SubscriptionType type = SubscriptionType.Basic, bool isActive = true,
        DateTime? startDate = null, DateTime? endDate = null, int? maxPortfolios = null, int? maxAlgorithms = null, int? maxBacktestRuns = null)
    {
        var defaults = GetDefaultLimits(type);
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);

        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = type,
            StartDate = start,
            EndDate = endDate ?? start.AddDays(365),
            IsActive = isActive,
            MaxPortfolios = maxPortfolios ?? defaults.MaxPortfolios,
            MaxAlgorithms = maxAlgorithms ?? defaults.MaxAlgorithms,
            MaxBacktestRuns = maxBacktestRuns ?? defaults.MaxBacktestRuns,
            CreatedAt = start,
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates an expired subscription model for testing
    /// </summary>
    public static SubscriptionModel CreateExpiredSubscription(Guid? userId = null, SubscriptionType type = SubscriptionType.Basic, 
        int daysExpiredAgo = 10)
    {
        var endDate = DateTime.UtcNow.AddDays(-daysExpiredAgo);
        var startDate = endDate.AddDays(-365);

        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true, // Still marked as active but expired by date
            MaxPortfolios = GetDefaultLimits(type).MaxPortfolios,
            MaxAlgorithms = GetDefaultLimits(type).MaxAlgorithms,
            MaxBacktestRuns = GetDefaultLimits(type).MaxBacktestRuns,
            CreatedAt = startDate,
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Creates an inactive subscription model for testing
    /// </summary>
    public static SubscriptionModel CreateInactiveSubscription(Guid? userId = null, SubscriptionType type = SubscriptionType.Free)
    {
        var startDate = DateTime.UtcNow.AddDays(-60);

        return new SubscriptionModel
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Type = type,
            StartDate = startDate,
            EndDate = startDate.AddDays(365),
            IsActive = false,
            MaxPortfolios = GetDefaultLimits(type).MaxPortfolios,
            MaxAlgorithms = GetDefaultLimits(type).MaxAlgorithms,
            MaxBacktestRuns = GetDefaultLimits(type).MaxBacktestRuns,
            CreatedAt = startDate,
            CreatedBy = "system",
            UpdatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedBy = "admin"
        };
    }

    /// <summary>
    /// Creates a subscription history for a user (multiple subscriptions over time)
    /// </summary>
    public static List<SubscriptionModel> CreateSubscriptionHistory(Guid userId)
    {
        return new List<SubscriptionModel>
        {
            // Started with Free (now inactive)
            CreateCustomSubscription(
                userId: userId,
                type: SubscriptionType.Free,
                isActive: false,
                startDate: DateTime.UtcNow.AddDays(-120),
                endDate: DateTime.UtcNow.AddDays(-60)
            ),
            
            // Upgraded to Basic (now inactive)
            CreateCustomSubscription(
                userId: userId,
                type: SubscriptionType.Basic,
                isActive: false,
                startDate: DateTime.UtcNow.AddDays(-60),
                endDate: DateTime.UtcNow.AddDays(-30)
            ),
            
            // Currently on Premium (active)
            CreateCustomSubscription(
                userId: userId,
                type: SubscriptionType.Premium,
                isActive: true,
                startDate: DateTime.UtcNow.AddDays(-30),
                endDate: DateTime.UtcNow.AddDays(335)
            )
        };
    }

    /// <summary>
    /// Creates a batch of subscriptions with different types for testing
    /// </summary>
    public static List<SubscriptionModel> CreateMixedSubscriptionBatch(int count = 5)
    {
        var subscriptions = new List<SubscriptionModel>();
        var types = Enum.GetValues<SubscriptionType>();

        for (int i = 0; i < count; i++)
        {
            var type = types[i % types.Length];
            var isActive = i % 3 != 0; // Make some inactive
            var startDate = DateTime.UtcNow.AddDays(-30 - (i * 5));

            subscriptions.Add(CreateCustomSubscription(
                type: type,
                isActive: isActive,
                startDate: startDate
            ));
        }

        return subscriptions;
    }

    /// <summary>
    /// Creates expired subscriptions for testing cleanup scenarios
    /// </summary>
    public static List<SubscriptionModel> CreateExpiredSubscriptionBatch(int count = 3)
    {
        var subscriptions = new List<SubscriptionModel>();
        var types = new[] { SubscriptionType.Basic, SubscriptionType.Premium, SubscriptionType.Enterprise };

        for (int i = 0; i < count; i++)
        {
            var type = types[i % types.Length];
            var daysExpired = 5 + (i * 5); // Different expiration dates

            subscriptions.Add(CreateExpiredSubscription(
                type: type,
                daysExpiredAgo: daysExpired
            ));
        }

        return subscriptions;
    }

    /// <summary>
    /// Creates a subscription for update testing
    /// </summary>
    public static SubscriptionModel CreateSubscriptionForUpdate(Guid subscriptionId, Guid userId, SubscriptionType newType = SubscriptionType.Premium, 
        bool newIsActive = true)
    {
        var limits = GetDefaultLimits(newType);

        return new SubscriptionModel
        {
            Id = subscriptionId,
            UserId = userId,
            Type = newType,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = newIsActive,
            MaxPortfolios = limits.MaxPortfolios,
            MaxAlgorithms = limits.MaxAlgorithms,
            MaxBacktestRuns = limits.MaxBacktestRuns,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "system",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "NormTheThird"
        };
    }

    /// <summary>
    /// Creates active subscriptions of a specific type for testing
    /// </summary>
    public static List<SubscriptionModel> CreateActiveSubscriptionsByType(SubscriptionType type, int count = 3)
    {
        var subscriptions = new List<SubscriptionModel>();

        for (int i = 0; i < count; i++)
        {
            subscriptions.Add(CreateCustomSubscription(
                type: type,
                isActive: true,
                startDate: DateTime.UtcNow.AddDays(-30 + (i * 5))
            ));
        }

        return subscriptions;
    }

    /// <summary>
    /// Creates subscriptions that will expire soon for testing notifications
    /// </summary>
    public static List<SubscriptionModel> CreateExpiringSubscriptionBatch(int count = 3)
    {
        var subscriptions = new List<SubscriptionModel>();
        var types = new[] { SubscriptionType.Basic, SubscriptionType.Premium, SubscriptionType.Enterprise };

        for (int i = 0; i < count; i++)
        {
            var type = types[i % types.Length];
            var daysUntilExpiry = 1 + i; // Expire in 1, 2, 3 days
            var startDate = DateTime.UtcNow.AddDays(-360);
            var endDate = DateTime.UtcNow.AddDays(daysUntilExpiry);

            subscriptions.Add(CreateCustomSubscription(
                type: type,
                isActive: true,
                startDate: startDate,
                endDate: endDate
            ));
        }

        return subscriptions;
    }

    /// <summary>
    /// Gets default limits for subscription types
    /// </summary>
    private static (int MaxPortfolios, int MaxAlgorithms, int MaxBacktestRuns) GetDefaultLimits(SubscriptionType type)
    {
        return type switch
        {
            SubscriptionType.Free => (1, 3, 10),
            SubscriptionType.Basic => (3, 15, 100),
            SubscriptionType.Premium => (10, 50, 500),
            SubscriptionType.Enterprise => (50, 200, 2000),
            _ => (1, 3, 10)
        };
    }
}