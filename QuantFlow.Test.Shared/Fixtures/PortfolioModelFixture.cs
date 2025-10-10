//namespace QuantFlow.Test.Shared.Fixtures;

///// <summary>
///// Fixture class for creating PortfolioModel test data
///// </summary>
//public static class PortfolioModelFixture
//{
//    /// <summary>
//    /// Creates a default portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateDefault(Guid? userId = null, string name = "Test Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "Default test portfolio for integration testing",
//            InitialBalance = 10000.0m,
//            CurrentBalance = 10000.0m,
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 15.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-30),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a default active portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateActivePortfolio(Guid? userId = null, string name = "My Trading Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A diversified cryptocurrency trading portfolio",
//            InitialBalance = 10000.0m,
//            CurrentBalance = 10000.0m,
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 15.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-30),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a profitable portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateProfitablePortfolio(Guid? userId = null, string name = "Profitable Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A portfolio that has generated profits",
//            InitialBalance = 10000.0m,
//            CurrentBalance = 12500.0m, // 25% profit
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 20.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = true,
//            CreatedAt = DateTime.UtcNow.AddDays(-60),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a losing portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateLosingPortfolio(Guid? userId = null, string name = "Struggling Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A portfolio that has experienced losses",
//            InitialBalance = 10000.0m,
//            CurrentBalance = 7500.0m, // 25% loss
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 10.0m,
//            CommissionRate = 0.002m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-45),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a paused portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreatePausedPortfolio(Guid? userId = null, string name = "Paused Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A portfolio that is temporarily paused",
//            InitialBalance = 5000.0m,
//            CurrentBalance = 5200.0m,
//            Status = PortfolioStatus.Paused,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 12.0m,
//            CommissionRate = 0.0015m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-20),
//            CreatedBy = "NormTheThird",
//            UpdatedAt = DateTime.UtcNow.AddDays(-5),
//            UpdatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates an archived portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateArchivedPortfolio(Guid? userId = null, string name = "Archived Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A portfolio that has been archived",
//            InitialBalance = 5000.0m,
//            CurrentBalance = 5100.0m,
//            Status = PortfolioStatus.Archived,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 10.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-90),
//            CreatedBy = "NormTheThird",
//            UpdatedAt = DateTime.UtcNow.AddDays(-7),
//            UpdatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a custom portfolio model with specific parameters
//    /// </summary>
//    public static PortfolioModel CreateCustomPortfolio(Guid? userId = null, string name = "Custom Portfolio", string description = "Custom portfolio description",
//        decimal initialBalance = 10000.0m, decimal currentBalance = 10000.0m, PortfolioStatus status = PortfolioStatus.Active,
//        decimal maxPositionSizePercent = 15.0m, decimal commissionRate = 0.001m, bool allowShortSelling = false, DateTime? createdAt = null)
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = description,
//            InitialBalance = initialBalance,
//            CurrentBalance = currentBalance,
//            Status = status,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = maxPositionSizePercent,
//            CommissionRate = commissionRate,
//            AllowShortSelling = allowShortSelling,
//            CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-30),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a high-risk trading portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateHighRiskPortfolio(Guid? userId = null, string name = "High Risk Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "Aggressive trading portfolio with higher risk tolerance",
//            InitialBalance = 25000.0m,
//            CurrentBalance = 28750.0m,
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 35.0m, // Higher position size
//            CommissionRate = 0.0005m, // Lower commission for frequent trading
//            AllowShortSelling = true,
//            CreatedAt = DateTime.UtcNow.AddDays(-90),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a conservative portfolio model for testing
//    /// </summary>
//    public static PortfolioModel CreateConservativePortfolio(Guid? userId = null, string name = "Conservative Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "Conservative portfolio with lower risk exposure",
//            InitialBalance = 50000.0m,
//            CurrentBalance = 51500.0m,
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 5.0m, // Lower position size
//            CommissionRate = 0.002m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-180),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a batch of portfolios for a single user
//    /// </summary>
//    public static List<PortfolioModel> CreateUserPortfolioBatch(Guid userId, int count = 3)
//    {
//        var portfolios = new List<PortfolioModel>();
//        var templates = new[]
//        {
//            ("Conservative", 50000.0m, 51000.0m, 5.0m, false),
//            ("Balanced", 25000.0m, 26500.0m, 15.0m, false),
//            ("Aggressive", 10000.0m, 11800.0m, 25.0m, true)
//        };

//        for (int i = 0; i < count; i++)
//        {
//            var template = templates[i % templates.Length];
//            portfolios.Add(CreateCustomPortfolio(
//                userId: userId,
//                name: $"{template.Item1} Portfolio {i + 1}",
//                description: $"User's {template.Item1.ToLower()} trading strategy",
//                initialBalance: template.Item2,
//                currentBalance: template.Item3,
//                maxPositionSizePercent: template.Item4,
//                allowShortSelling: template.Item5,
//                createdAt: DateTime.UtcNow.AddDays(-30 - (i * 10))
//            ));
//        }

//        return portfolios;
//    }

//    /// <summary>
//    /// Creates a batch of portfolios with different statuses
//    /// </summary>
//    public static List<PortfolioModel> CreateMixedStatusPortfolioBatch(Guid? userId = null, int count = 4)
//    {
//        var portfolios = new List<PortfolioModel>();
//        var statuses = Enum.GetValues<PortfolioStatus>();
//        var user = userId ?? Guid.NewGuid();

//        for (int i = 0; i < count; i++)
//        {
//            var status = statuses[i % statuses.Length];
//            var baseBalance = 10000.0m + (i * 5000.0m);
//            var currentBalance = status == PortfolioStatus.Active ? baseBalance * 1.1m : baseBalance;

//            portfolios.Add(CreateCustomPortfolio(
//                userId: user,
//                name: $"{status} Portfolio {i + 1}",
//                description: $"Portfolio in {status} status",
//                initialBalance: baseBalance,
//                currentBalance: currentBalance,
//                status: status,
//                createdAt: DateTime.UtcNow.AddDays(-30 - (i * 15))
//            ));
//        }

//        return portfolios;
//    }

//    /// <summary>
//    /// Creates portfolios for multiple users
//    /// </summary>
//    public static List<PortfolioModel> CreateMultiUserPortfolioBatch(int userCount = 3, int portfoliosPerUser = 2)
//    {
//        var portfolios = new List<PortfolioModel>();

//        for (int u = 0; u < userCount; u++)
//        {
//            var userId = Guid.NewGuid();
//            for (int p = 0; p < portfoliosPerUser; p++)
//            {
//                portfolios.Add(CreateCustomPortfolio(
//                    userId: userId,
//                    name: $"User {u + 1} Portfolio {p + 1}",
//                    description: $"Portfolio {p + 1} belonging to user {u + 1}",
//                    initialBalance: 10000.0m + (u * 5000.0m),
//                    currentBalance: 10000.0m + (u * 5000.0m) + (p * 1000.0m),
//                    createdAt: DateTime.UtcNow.AddDays(-30 - (u * 10) - (p * 5))
//                ));
//            }
//        }

//        return portfolios;
//    }

//    /// <summary>
//    /// Creates a portfolio for update testing
//    /// </summary>
//    public static PortfolioModel CreatePortfolioForUpdate(Guid portfolioId, Guid userId, string newName = "Updated Portfolio",
//        string newDescription = "Updated portfolio description", decimal newCurrentBalance = 12000.0m,
//        PortfolioStatus newStatus = PortfolioStatus.Paused, decimal newMaxPositionSizePercent = 20.0m,
//        decimal newCommissionRate = 0.002m, bool newAllowShortSelling = true)
//    {
//        return new PortfolioModel
//        {
//            Id = portfolioId,
//            Name = newName,
//            Description = newDescription,
//            InitialBalance = 10000.0m, // Typically doesn't change
//            CurrentBalance = newCurrentBalance,
//            Status = newStatus,
//            UserId = userId,
//            MaxPositionSizePercent = newMaxPositionSizePercent,
//            CommissionRate = newCommissionRate,
//            AllowShortSelling = newAllowShortSelling,
//            CreatedAt = DateTime.UtcNow.AddDays(-30),
//            CreatedBy = "NormTheThird",
//            UpdatedAt = DateTime.UtcNow,
//            UpdatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates a portfolio with specific performance metrics
//    /// </summary>
//    public static PortfolioModel CreatePortfolioWithPerformance(Guid? userId = null, decimal profitLossPercent = 15.0m, string name = "Performance Portfolio")
//    {
//        var initialBalance = 10000.0m;
//        var currentBalance = initialBalance * (1 + (profitLossPercent / 100m));

//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = $"Portfolio with {profitLossPercent:F1}% performance",
//            InitialBalance = initialBalance,
//            CurrentBalance = currentBalance,
//            Status = PortfolioStatus.Active,
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 15.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = profitLossPercent > 10.0m, // Allow short selling for higher performers
//            CreatedAt = DateTime.UtcNow.AddDays(-60),
//            CreatedBy = "NormTheThird"
//        };
//    }

//    /// <summary>
//    /// Creates portfolios with various balance ranges for testing
//    /// </summary>
//    public static List<PortfolioModel> CreatePortfoliosByBalanceRange(Guid? userId = null)
//    {
//        var user = userId ?? Guid.NewGuid();
//        return new List<PortfolioModel>
//        {
//            CreateCustomPortfolio(user, "Small Portfolio", "Small balance portfolio", 1000.0m, 1050.0m),
//            CreateCustomPortfolio(user, "Medium Portfolio", "Medium balance portfolio", 10000.0m, 10500.0m),
//            CreateCustomPortfolio(user, "Large Portfolio", "Large balance portfolio", 100000.0m, 105000.0m),
//            CreateCustomPortfolio(user, "Whale Portfolio", "Very large balance portfolio", 1000000.0m, 1050000.0m)
//        };
//    }

//    /// <summary>
//    /// Creates deleted (soft-deleted) portfolio for testing
//    /// </summary>
//    public static PortfolioModel CreateDeletedPortfolio(Guid? userId = null, string name = "Deleted Portfolio")
//    {
//        return new PortfolioModel
//        {
//            Id = Guid.NewGuid(),
//            Name = name,
//            Description = "A portfolio that has been soft deleted",
//            InitialBalance = 5000.0m,
//            CurrentBalance = 5100.0m,
//            Status = PortfolioStatus.Archived, // Changed from Inactive to Archived
//            UserId = userId ?? Guid.NewGuid(),
//            MaxPositionSizePercent = 10.0m,
//            CommissionRate = 0.001m,
//            AllowShortSelling = false,
//            CreatedAt = DateTime.UtcNow.AddDays(-90),
//            CreatedBy = "NormTheThird",
//            UpdatedAt = DateTime.UtcNow.AddDays(-7),
//            UpdatedBy = "NormTheThird"
//        };
//    }
//}