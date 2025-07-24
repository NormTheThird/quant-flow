namespace QuantFlow.IntegrationTests.Repositories;

/// <summary>
/// Base class for repository integration tests with in-memory database
/// </summary>
public abstract class BaseRepositoryIntegrationTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly ILogger MockLogger;
    private readonly string _databaseName;

    protected BaseRepositoryIntegrationTest()
    {
        _databaseName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ApplicationDbContext(options);
        MockLogger = Substitute.For<ILogger>();

        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Seeds a test user and returns the user ID
    /// </summary>
    /// <param name="username">Username (optional)</param>
    /// <param name="email">Email (optional)</param>
    /// <returns>User ID</returns>
    protected async Task<Guid> SeedTestUserAsync(string username = "testuser", string email = "test@example.com")
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Username = username,
            Email = email,
            PasswordHash = "hashedpassword",
            IsEmailVerified = true,
            IsSystemAdmin = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return userId;
    }

    /// <summary>
    /// Seeds multiple test users
    /// </summary>
    /// <param name="count">Number of users to create</param>
    /// <returns>List of user IDs</returns>
    protected async Task<List<Guid>> SeedTestUsersAsync(int count)
    {
        var userIds = new List<Guid>();

        for (int i = 0; i < count; i++)
        {
            var userId = Guid.NewGuid();
            var user = new UserEntity
            {
                Id = userId,
                Username = $"testuser{i}",
                Email = $"test{i}@example.com",
                PasswordHash = "hashedpassword",
                IsEmailVerified = i % 2 == 0, // Alternate verification status
                IsSystemAdmin = false,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                CreatedBy = "test"
            };

            Context.Users.Add(user);
            userIds.Add(userId);
        }

        await Context.SaveChangesAsync();
        return userIds;
    }

    /// <summary>
    /// Seeds a test portfolio and returns the portfolio ID
    /// </summary>
    /// <param name="userId">Owner user ID</param>
    /// <param name="name">Portfolio name (optional)</param>
    /// <returns>Portfolio ID</returns>
    protected async Task<Guid> SeedTestPortfolioAsync(Guid userId, string name = "Test Portfolio")
    {
        var portfolioId = Guid.NewGuid();
        var portfolio = new PortfolioEntity
        {
            Id = portfolioId,
            Name = name,
            Description = "Test portfolio description",
            InitialBalance = 10000.0m,
            CurrentBalance = 12000.0m,
            Status = (int)PortfolioStatus.Active,
            UserId = userId,
            MaxPositionSizePercent = 10.0m,
            CommissionRate = 0.001m,
            AllowShortSelling = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        Context.Portfolios.Add(portfolio);
        await Context.SaveChangesAsync();
        return portfolioId;
    }

    /// <summary>
    /// Seeds a test backtest run and returns the backtest run ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="name">Backtest name (optional)</param>
    /// <returns>Backtest run ID</returns>
    protected async Task<Guid> SeedTestBacktestRunAsync(Guid userId, Guid portfolioId, string name = "Test Backtest")
    {
        var backtestId = Guid.NewGuid();
        var backtest = new BacktestRunEntity
        {
            Id = backtestId,
            Name = name,
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = portfolioId,
            UserId = userId,
            Symbol = "BTCUSDT",
            Exchange = (int)Exchange.Binance,
            Timeframe = (int)Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = (int)BacktestStatus.Completed,
            InitialBalance = 10000.0m,
            AlgorithmParameters = "{}",
            CommissionRate = 0.001m,
            FinalBalance = 12000.0m,
            TotalReturnPercent = 20.0m,
            MaxDrawdownPercent = -5.0m,
            SharpeRatio = 1.5m,
            TotalTrades = 10,
            WinningTrades = 7,
            LosingTrades = 3,
            WinRatePercent = 70.0m,
            AverageTradeReturnPercent = 2.0m,
            ExecutionDurationTicks = TimeSpan.FromMinutes(5).Ticks,
            ErrorMessage = string.Empty,
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            CreatedBy = "test"
        };

        Context.BacktestRuns.Add(backtest);
        await Context.SaveChangesAsync();
        return backtestId;
    }

    /// <summary>
    /// Seeds a test trade and returns the trade ID
    /// </summary>
    /// <param name="backtestRunId">Backtest run ID</param>
    /// <param name="tradeType">Trade type (optional)</param>
    /// <param name="symbol">Symbol (optional)</param>
    /// <returns>Trade ID</returns>
    protected async Task<Guid> SeedTestTradeAsync(Guid backtestRunId, TradeType tradeType = TradeType.Buy, string symbol = "BTCUSDT")
    {
        var tradeId = Guid.NewGuid();
        var trade = new TradeEntity
        {
            Id = tradeId,
            BacktestRunId = backtestRunId,
            Symbol = symbol,
            Type = (int)tradeType,
            ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
            Quantity = 0.1m,
            Price = 50000.0m,
            Value = 5000.0m,
            Commission = 5.0m,
            NetValue = 4995.0m,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = tradeType == TradeType.Buy ? 5000.0m : 15000.0m,
            AlgorithmReason = "Test trade",
            AlgorithmConfidence = 0.85m,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        Context.Trades.Add(trade);
        await Context.SaveChangesAsync();
        return tradeId;
    }

    /// <summary>
    /// Seeds a test symbol and returns the symbol ID
    /// </summary>
    /// <param name="symbol">Symbol name (optional)</param>
    /// <param name="baseAsset">Base asset (optional)</param>
    /// <param name="quoteAsset">Quote asset (optional)</param>
    /// <returns>Symbol ID</returns>
    protected async Task<Guid> SeedTestSymbolAsync(string symbol = "BTCUSDT", string baseAsset = "BTC", string quoteAsset = "USDT")
    {
        var symbolId = Guid.NewGuid();
        var symbolEntity = new SymbolEntity
        {
            Id = symbolId,
            Symbol = symbol,
            BaseAsset = baseAsset,
            QuoteAsset = quoteAsset,
            IsActive = true,
            MinTradeAmount = 0.001m,
            PricePrecision = 2,
            QuantityPrecision = 8,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        Context.Symbols.Add(symbolEntity);
        await Context.SaveChangesAsync();
        return symbolId;
    }

    /// <summary>
    /// Seeds a test subscription and returns the subscription ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="subscriptionType">Subscription type (optional)</param>
    /// <returns>Subscription ID</returns>
    protected async Task<Guid> SeedTestSubscriptionAsync(Guid userId, SubscriptionType subscriptionType = SubscriptionType.Free)
    {
        var subscriptionId = Guid.NewGuid();
        var subscription = new SubscriptionEntity
        {
            Id = subscriptionId,
            UserId = userId,
            Type = (int)subscriptionType,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(330),
            IsActive = true,
            MaxPortfolios = subscriptionType == SubscriptionType.Free ? 1 : 5,
            MaxAlgorithms = subscriptionType == SubscriptionType.Free ? 5 : 20,
            MaxBacktestRuns = subscriptionType == SubscriptionType.Free ? 10 : 100,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "test"
        };

        Context.Subscriptions.Add(subscription);
        await Context.SaveChangesAsync();
        return subscriptionId;
    }

    /// <summary>
    /// Clears all data from the context
    /// </summary>
    protected void ClearDatabase()
    {
        Context.Trades.RemoveRange(Context.Trades);
        Context.BacktestRuns.RemoveRange(Context.BacktestRuns);
        Context.Portfolios.RemoveRange(Context.Portfolios);
        Context.Subscriptions.RemoveRange(Context.Subscriptions);
        Context.ExchangeSymbols.RemoveRange(Context.ExchangeSymbols);
        Context.Symbols.RemoveRange(Context.Symbols);
        Context.Users.RemoveRange(Context.Users);
        Context.SaveChanges();
    }

    public virtual void Dispose()
    {
        Context.Dispose();
    }
}