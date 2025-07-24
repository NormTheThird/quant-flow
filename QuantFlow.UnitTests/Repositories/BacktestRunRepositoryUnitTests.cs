namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for BacktestRunRepository using in-memory database
/// </summary>
public class BacktestRunRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
{
    private readonly Mock<ILogger<BacktestRunRepository>> _mockLogger;
    private readonly BacktestRunRepository _repository;

    public BacktestRunRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<BacktestRunRepository>>();
        _repository = new BacktestRunRepository(Context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBacktestRun_ReturnsBacktestRunModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Test Backtest");
        var backtestEntity = backtestModel.ToEntity();

        Context.BacktestRuns.Add(backtestEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(backtestEntity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestEntity.Id, result.Id);
        Assert.Equal("Test Backtest", result.Name);
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
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserBacktestRuns()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("OtherUser");
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModels = new[]
        {
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Backtest 1"),
            BacktestRunModelFixture.CreateRunningBacktestRun(userId, portfolioId, "Backtest 2"),
            BacktestRunModelFixture.CreatePendingBacktestRun(otherUserId, portfolioId, "Other User Backtest")
        };

        var backtestEntities = backtestModels.Select(b => b.ToEntity());
        Context.BacktestRuns.AddRange(backtestEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var backtestList = result.ToList();
        Assert.Equal(2, backtestList.Count);
        Assert.All(backtestList, b => Assert.Equal(userId, b.UserId));
        Assert.Contains(backtestList, b => b.Name == "Backtest 1");
        Assert.Contains(backtestList, b => b.Name == "Backtest 2");
        Assert.DoesNotContain(backtestList, b => b.Name == "Other User Backtest");
    }

    [Fact]
    public async Task GetByPortfolioIdAsync_ExistingPortfolio_ReturnsPortfolioBacktestRuns()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModels = new[]
        {
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Portfolio Backtest 1"),
            BacktestRunModelFixture.CreateRunningBacktestRun(userId, portfolioId, "Portfolio Backtest 2")
        };

        var backtestEntities = backtestModels.Select(b => b.ToEntity());
        Context.BacktestRuns.AddRange(backtestEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPortfolioIdAsync(portfolioId);

        // Assert
        var backtestList = result.ToList();
        Assert.Equal(2, backtestList.Count);
        Assert.All(backtestList, b => Assert.Equal(portfolioId, b.PortfolioId));
    }

    [Fact]
    public async Task GetByStatusAsync_ExistingStatus_ReturnsBacktestRunsWithStatus()
    {
        // Arrange
        var status = BacktestStatus.Completed;
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModels = new[]
        {
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Completed Backtest 1"),
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Completed Backtest 2"),
            BacktestRunModelFixture.CreatePendingBacktestRun(userId, portfolioId, "Pending Backtest")
        };

        var backtestEntities = backtestModels.Select(b => b.ToEntity());
        Context.BacktestRuns.AddRange(backtestEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(status);

        // Assert
        var backtestList = result.ToList();
        Assert.Equal(2, backtestList.Count);
        Assert.All(backtestList, b => Assert.Equal(BacktestStatus.Completed, b.Status));
    }

    [Fact]
    public async Task CreateAsync_ValidBacktestRun_CallsAddAndSaveChanges()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestModel = BacktestRunModelFixture.CreatePendingBacktestRun(userId, portfolioId, "New Backtest");

        // Act
        var result = await _repository.CreateAsync(backtestModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestModel.Name, result.Name);
        Assert.Equal(backtestModel.Symbol, result.Symbol);
        Assert.Equal(backtestModel.Exchange, result.Exchange);
        Assert.Equal(backtestModel.Timeframe, result.Timeframe);
        Assert.Equal(backtestModel.Status, result.Status);

        // Verify it was actually saved to the database
        var savedEntity = await Context.BacktestRuns.FindAsync(result.Id);
        Assert.NotNull(savedEntity);
        Assert.Equal(result.Name, savedEntity.Name);
    }

    //[Fact]
    //public async Task UpdateAsync_ExistingBacktestRun_UpdatesAndSaves()
    //{
    //    // Arrange
    //    var backtestId = Guid.NewGuid();
    //    var existingBacktest = new BacktestRunEntity
    //    {
    //        Id = backtestId,
    //        Name = "Original Backtest",
    //        Status = (int)BacktestStatus.Running,
    //        FinalBalance = 0.0m,
    //        TotalReturnPercent = 0.0m,
    //        TotalTrades = 0,
    //        WinningTrades = 0,
    //        LosingTrades = 0,
    //        WinRatePercent = 0.0m,
    //        ExecutionDurationTicks = 0,
    //        ErrorMessage = string.Empty,
    //        CompletedAt = null,
    //        CreatedAt = DateTime.UtcNow.AddDays(-1)
    //    };

    //    var updatedModel = new BacktestRunModel
    //    {
    //        Id = backtestId,
    //        Name = "Updated Backtest",
    //        Status = BacktestStatus.Completed,
    //        FinalBalance = 12000.0m,
    //        TotalReturnPercent = 20.0m,
    //        MaxDrawdownPercent = -5.0m,
    //        SharpeRatio = 1.5m,
    //        TotalTrades = 15,
    //        WinningTrades = 10,
    //        LosingTrades = 5,
    //        WinRatePercent = 66.67m,
    //        AverageTradeReturnPercent = 1.33m,
    //        ExecutionDuration = TimeSpan.FromMinutes(10),
    //        ErrorMessage = string.Empty,
    //        CompletedAt = DateTime.UtcNow
    //    };

    //    var mockDbSet = new Mock<DbSet<BacktestRunEntity>>();
    //    SetupFindAsync(mockDbSet, new[] { existingBacktest }, b => b.Id);

    //    MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);
    //    MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

    //    // Act
    //    var result = await _repository.UpdateAsync(updatedModel);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal("Updated Backtest", result.Name);
    //    Assert.Equal(BacktestStatus.Completed, result.Status);
    //    Assert.Equal(12000.0m, result.FinalBalance);
    //    Assert.Equal(20.0m, result.TotalReturnPercent);
    //    Assert.Equal(15, result.TotalTrades);
    //    Assert.Equal(10, result.WinningTrades);
    //    Assert.Equal(5, result.LosingTrades);
    //    Assert.Equal(66.67m, result.WinRatePercent);

    //    MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    //}

    [Fact]
    public async Task UpdateAsync_NonExistentBacktestRun_ThrowsNotFoundException()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Nonexistent Backtest");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(backtestModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingBacktestRun_SoftDeletesBacktestRun()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Test Backtest");
        var backtestEntity = backtestModel.ToEntity();

        Context.BacktestRuns.Add(backtestEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(backtestEntity.Id);

        // Assert
        Assert.True(result);

        // Verify soft delete was applied
        var deletedEntity = await Context.BacktestRuns.FindAsync(backtestEntity.Id);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.UpdatedAt);
    }

    [Fact]
    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var otherUserId = await SeedTestUserAsync("OtherUser");
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModels = new[]
        {
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Backtest 1"),
            BacktestRunModelFixture.CreateRunningBacktestRun(userId, portfolioId, "Backtest 2"),
            BacktestRunModelFixture.CreatePendingBacktestRun(otherUserId, portfolioId, "Different user backtest")
        };

        var backtestEntities = backtestModels.Select(b => b.ToEntity());
        Context.BacktestRuns.AddRange(backtestEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetAllAsync_WithBacktestRuns_ReturnsAllActiveBacktestRuns()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);

        var backtestModels = new[]
        {
            BacktestRunModelFixture.CreateCompletedBacktestRun(userId, portfolioId, "Backtest 1"),
            BacktestRunModelFixture.CreateRunningBacktestRun(userId, portfolioId, "Backtest 2")
        };

        var backtestEntities = backtestModels.Select(b => b.ToEntity());
        Context.BacktestRuns.AddRange(backtestEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var backtestList = result.ToList();
        Assert.Equal(2, backtestList.Count);
        Assert.Contains(backtestList, b => b.Name == "Backtest 1");
        Assert.Contains(backtestList, b => b.Name == "Backtest 2");
    }
}