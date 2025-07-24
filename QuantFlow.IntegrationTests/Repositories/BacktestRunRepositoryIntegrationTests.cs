namespace QuantFlow.Test.Integration.Repositories;

/// <summary>
/// Integration tests for BacktestRunRepository with in-memory database
/// </summary>
public class BacktestRunRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly BacktestRunRepository _repository;

    public BacktestRunRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<BacktestRunRepository>>();
        _repository = new BacktestRunRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBacktestRun_ReturnsBacktestRunModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Integration Test Backtest");

        // Convert model to entity and add to context
        var backtestEntity = backtestModel.ToEntity();
        Context.BacktestRuns.Add(backtestEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(backtestModel.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestModel.Id, result.Id);
        Assert.Equal("Integration Test Backtest", result.Name);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(portfolioId, result.PortfolioId);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal(Exchange.Binance, result.Exchange);
        Assert.Equal(Timeframe.OneHour, result.Timeframe);
        Assert.Equal(BacktestStatus.Completed, result.Status);
        Assert.Equal(10000.0m, result.InitialBalance);
        Assert.Equal(12000.0m, result.FinalBalance);
        Assert.Equal(20.0m, result.TotalReturnPercent);
        Assert.Equal(-5.0m, result.MaxDrawdownPercent);
        Assert.Equal(1.5m, result.SharpeRatio);
        Assert.Equal(10, result.TotalTrades);
        Assert.Equal(7, result.WinningTrades);
        Assert.Equal(3, result.LosingTrades);
        Assert.Equal(70.0m, result.WinRatePercent);
        Assert.Equal(2.0m, result.AverageTradeReturnPercent);
        Assert.Equal(TimeSpan.FromMinutes(5), result.ExecutionDuration);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentBacktestRun_ReturnsNull()
    {
        // Arrange
        var backtestId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(backtestId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedBacktestRun_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestModel = BacktestRunModelFixture.CreateDefault();
        backtestModel.UserId = userId;
        backtestModel.PortfolioId = portfolioId;

        var backtestEntity = backtestModel.ToEntity();
        Context.BacktestRuns.Add(backtestEntity);
        await Context.SaveChangesAsync();

        // Soft delete the backtest run
        var backtest = await Context.BacktestRuns.FindAsync(backtestModel.Id);
        backtest!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(backtestModel.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserBacktestRuns()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("otheruser", "other@example.com");

        var portfolioId = await SeedTestPortfolioAsync(userId);
        var otherPortfolioId = await SeedTestPortfolioAsync(otherUserId);

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(userId, portfolioId, "User Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(userId, portfolioId, "User Backtest 2");
        var otherBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(otherUserId, otherPortfolioId, "Other User Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(backtest1.ToEntity(), backtest2.ToEntity(), otherBacktest.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count);
        Assert.All(backtestRuns, b => Assert.Equal(userId, b.UserId));
        Assert.Contains(backtestRuns, b => b.Name == "User Backtest 1");
        Assert.Contains(backtestRuns, b => b.Name == "User Backtest 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Other User Backtest");
    }

    [Fact]
    public async Task GetByPortfolioIdAsync_ExistingPortfolio_ReturnsPortfolioBacktestRuns()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        var portfolio1Id = await SeedTestPortfolioAsync(user1Id, "Portfolio 1");
        var portfolio2Id = await SeedTestPortfolioAsync(user2Id, "Portfolio 2");

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user1Id, portfolio1Id, "Portfolio 1 Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user1Id, portfolio1Id, "Portfolio 1 Backtest 2");
        var backtest3 = BacktestRunModelFixture.CreateCustomBacktestRun(user2Id, portfolio2Id, "Portfolio 2 Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(
            backtest1.ToEntity(),
            backtest2.ToEntity(),
            backtest3.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPortfolioIdAsync(portfolio1Id);

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count);
        Assert.All(backtestRuns, b => Assert.Equal(portfolio1Id, b.PortfolioId));
        Assert.Contains(backtestRuns, b => b.Name == "Portfolio 1 Backtest 1");
        Assert.Contains(backtestRuns, b => b.Name == "Portfolio 1 Backtest 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Portfolio 2 Backtest");
    }

    [Fact]
    public async Task GetByStatusAsync_ExistingStatus_ReturnsBacktestRunsWithStatus()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        // Create backtest runs with different statuses
        var completedBacktest1 = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Completed 1");
        var completedBacktest2 = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Completed 2");
        var runningBacktest = BacktestRunModelFixture.CreateRunningBacktestRun(userId, portfolioId, "Running Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(
            completedBacktest1.ToEntity(),
            completedBacktest2.ToEntity(),
            runningBacktest.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(BacktestStatus.Completed);

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count);
        Assert.All(backtestRuns, b => Assert.Equal(BacktestStatus.Completed, b.Status));
        Assert.Contains(backtestRuns, b => b.Name == "Completed 1");
        Assert.Contains(backtestRuns, b => b.Name == "Completed 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Running Backtest");
    }

    [Fact]
    public async Task CreateAsync_ValidBacktestRun_ReturnsCreatedBacktestRun()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModel = BacktestRunModelFixture.CreatePendingBacktestRun(userId, portfolioId, "New Integration Backtest");

        // Act
        var result = await _repository.CreateAsync(backtestModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestModel.Name, result.Name);
        Assert.Equal(backtestModel.AlgorithmId, result.AlgorithmId);
        Assert.Equal(backtestModel.PortfolioId, result.PortfolioId);
        Assert.Equal(backtestModel.UserId, result.UserId);
        Assert.Equal(backtestModel.Symbol, result.Symbol);
        Assert.Equal(backtestModel.Exchange, result.Exchange);
        Assert.Equal(backtestModel.Timeframe, result.Timeframe);
        Assert.Equal(backtestModel.Status, result.Status);
        Assert.Equal(backtestModel.InitialBalance, result.InitialBalance);
        Assert.Equal(backtestModel.AlgorithmParameters, result.AlgorithmParameters);
        Assert.Equal(backtestModel.CommissionRate, result.CommissionRate);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbBacktest = await Context.BacktestRuns.FindAsync(result.Id);
        Assert.NotNull(dbBacktest);
        Assert.Equal(backtestModel.Name, dbBacktest.Name);
        Assert.Equal(backtestModel.Symbol, dbBacktest.Symbol);
    }

    //[Fact]
    //public async Task UpdateAsync_ExistingBacktestRun_ReturnsUpdatedBacktestRun()
    //{
    //    // Arrange
    //    var userId = await SeedTestUserAsync();
    //    var portfolioId = await SeedTestPortfolioAsync(userId);
    //    var backtestId = await SeedTestBacktestRunAsync(userId, portfolioId, "Original Backtest");

    //    var updatedModel = new BacktestRunModel
    //    {
    //        Id = backtestId,
    //        Name = "Updated Integration Backtest",
    //        Status = BacktestStatus.Failed,
    //        FinalBalance = 8000.0m,
    //        TotalReturnPercent = -20.0m,
    //        MaxDrawdownPercent = -15.0m,
    //        SharpeRatio = -0.5m,
    //        TotalTrades = 5,
    //        WinningTrades = 1,
    //        LosingTrades = 4,
    //        WinRatePercent = 20.0m,
    //        AverageTradeReturnPercent = -4.0m,
    //        ExecutionDuration = TimeSpan.FromMinutes(3),
    //        ErrorMessage = "Algorithm execution failed",
    //        CompletedAt = DateTime.UtcNow
    //    };

    //    // Act
    //    var result = await _repository.UpdateAsync(updatedModel);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal("Updated Integration Backtest", result.Name);
    //    Assert.Equal(BacktestStatus.Failed, result.Status);
    //    Assert.Equal(8000.0m, result.FinalBalance);
    //    Assert.Equal(-20.0m, result.TotalReturnPercent);
    //    Assert.Equal(-15.0m, result.MaxDrawdownPercent);
    //    Assert.Equal(-0.5m, result.SharpeRatio);
    //    Assert.Equal(5, result.TotalTrades);
    //    Assert.Equal(1, result.WinningTrades);
    //    Assert.Equal(4, result.LosingTrades);
    //    Assert.Equal(20.0m, result.WinRatePercent);
    //    Assert.Equal(-4.0m, result.AverageTradeReturnPercent);
    //    Assert.Equal(TimeSpan.FromMinutes(3), result.ExecutionDuration);
    //    Assert.Equal("Algorithm execution failed", result.ErrorMessage);
    //    Assert.NotNull(result.UpdatedAt);

    //    // Verify in database
    //    var dbBacktest = await Context.BacktestRuns.FindAsync(backtestId);
    //    Assert.NotNull(dbBacktest);
    //    Assert.Equal("Updated Integration Backtest", dbBacktest.Name);
    //    Assert.Equal((int)BacktestStatus.Failed, dbBacktest.Status);
    //    Assert.Equal(8000.0m, dbBacktest.FinalBalance);
    //    Assert.Equal("Algorithm execution failed", dbBacktest.ErrorMessage);
    //}

    [Fact]
    public async Task UpdateAsync_NonExistentBacktestRun_ThrowsNotFoundException()
    {
        // Arrange
        var backtestModel = BacktestRunModelFixture.CreateDefault();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(backtestModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingBacktestRun_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestModel = BacktestRunModelFixture.CreateDefault();
        backtestModel.UserId = userId;
        backtestModel.PortfolioId = portfolioId;

        var backtestEntity = backtestModel.ToEntity();
        Context.BacktestRuns.Add(backtestEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(backtestModel.Id);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbBacktest = await Context.BacktestRuns.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == backtestModel.Id);
        Assert.NotNull(dbBacktest);
        Assert.True(dbBacktest.IsDeleted);
        Assert.NotNull(dbBacktest.UpdatedAt);

        // Verify backtest run is not returned by normal queries
        var backtestResult = await _repository.GetByIdAsync(backtestModel.Id);
        Assert.Null(backtestResult);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentBacktestRun_ReturnsFalse()
    {
        // Arrange
        var backtestId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(backtestId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("otheruser", "other@example.com");

        var portfolioId = await SeedTestPortfolioAsync(userId);
        var otherPortfolioId = await SeedTestPortfolioAsync(otherUserId);

        // Create backtest runs for the user
        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(userId, portfolioId, "Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(userId, portfolioId, "Backtest 2");
        var deletedBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(userId, portfolioId, "Deleted Backtest");
        var otherBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(otherUserId, otherPortfolioId, "Other User Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(
            backtest1.ToEntity(),
            backtest2.ToEntity(),
            deletedBacktest.ToEntity(),
            otherBacktest.ToEntity());
        await Context.SaveChangesAsync();

        // Soft delete one backtest run
        var toDelete = await Context.BacktestRuns.FindAsync(deletedBacktest.Id);
        toDelete!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result); // Only active backtest runs for the user
    }

    [Fact]
    public async Task CountByUserIdAsync_UserWithNoBacktestRuns_ReturnsZero()
    {
        // Arrange
        var userId = await SeedTestUserAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetAllAsync_WithBacktestRuns_ReturnsAllActiveBacktestRuns()
    {
        // Arrange
        var user1Id = await SeedTestUserAsync("user1", "user1@example.com");
        var user2Id = await SeedTestUserAsync("user2", "user2@example.com");

        var portfolio1Id = await SeedTestPortfolioAsync(user1Id);
        var portfolio2Id = await SeedTestPortfolioAsync(user2Id);

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user1Id, portfolio1Id, "Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user2Id, portfolio2Id, "Backtest 2");
        var deletedBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(user1Id, portfolio1Id, "Deleted Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(
            backtest1.ToEntity(),
            backtest2.ToEntity(),
            deletedBacktest.ToEntity());
        await Context.SaveChangesAsync();

        // Soft delete one backtest run
        var toDelete = await Context.BacktestRuns.FindAsync(deletedBacktest.Id);
        toDelete!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count); // Only active backtest runs
        Assert.Contains(backtestRuns, b => b.Name == "Backtest 1");
        Assert.Contains(backtestRuns, b => b.Name == "Backtest 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Deleted Backtest");
    }

    [Fact]
    public async Task GetAllAsync_NoBacktestRuns_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }
}