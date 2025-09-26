//namespace QuantFlow.Test.Unit.Repositories;

///// <summary>
///// Unit tests for BacktestRunRepository using in-memory database
///// </summary>
//public class BacktestRunRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
//{
//    private readonly Mock<ILogger<BacktestRunRepository>> _mockLogger;
//    private readonly BacktestRunRepository _repository;

//    public BacktestRunRepositoryUnitTests()
//    {
//        _mockLogger = new Mock<ILogger<BacktestRunRepository>>();
//        _repository = new BacktestRunRepository(Context, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ExistingBacktestRun_ReturnsBacktestRunModel()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Test Backtest");
//        var backtestEntity = backtestModel.ToEntity();
//        Context.BacktestRuns.Add(backtestEntity);

//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByIdAsync(backtestEntity.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(backtestEntity.Id, result.Id);
//        Assert.Equal("Test Backtest", result.Name);
//        Assert.Equal(userEntity.Id, result.UserId);
//        Assert.Equal(portfolioEntity.Id, result.PortfolioId);
//        Assert.Equal("BTCUSDT", result.Symbol);
//        Assert.Equal(Exchange.Kraken, result.Exchange);
//        Assert.Equal(Timeframe.OneHour, result.Timeframe);
//        Assert.Equal(BacktestStatus.Completed, result.Status);
//        Assert.Equal(10000.0m, result.InitialBalance);
//        Assert.Equal(12000.0m, result.FinalBalance);
//        Assert.Equal(20.0m, result.TotalReturnPercent);
//        Assert.Equal(-5.0m, result.MaxDrawdownPercent);
//        Assert.Equal(1.5m, result.SharpeRatio);
//        Assert.Equal(10, result.TotalTrades);
//        Assert.Equal(7, result.WinningTrades);
//        Assert.Equal(3, result.LosingTrades);
//        Assert.Equal(70.0m, result.WinRatePercent);
//        Assert.Equal(2.0m, result.AverageTradeReturnPercent);
//        Assert.Equal(TimeSpan.FromMinutes(5), result.ExecutionDuration);
//    }

//    [Fact]
//    public async Task GetByIdAsync_NonExistentBacktestRun_ReturnsNull()
//    {
//        // Arrange
//        var backtestId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetByIdAsync(backtestId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserBacktestRuns()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var otherUserModel = UserModelFixture.CreateDefault("OtherUser", "other@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        var otherUserEntity = otherUserModel.ToEntity();
//        Context.Users.AddRange(new[] { userEntity, otherUserEntity });

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModels = new[]
//        {
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 1"),
//            BacktestRunModelFixture.CreateRunningBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 2"),
//            BacktestRunModelFixture.CreatePendingBacktestRun(otherUserEntity.Id, portfolioEntity.Id, "Other User Backtest")
//        };

//        var backtestEntities = backtestModels.Select(b => b.ToEntity());
//        Context.BacktestRuns.AddRange(backtestEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByUserIdAsync(userEntity.Id);

//        // Assert
//        var backtestList = result.ToList();
//        Assert.Equal(2, backtestList.Count);
//        Assert.All(backtestList, b => Assert.Equal(userEntity.Id, b.UserId));
//        Assert.Contains(backtestList, b => b.Name == "Backtest 1");
//        Assert.Contains(backtestList, b => b.Name == "Backtest 2");
//        Assert.DoesNotContain(backtestList, b => b.Name == "Other User Backtest");
//    }

//    [Fact]
//    public async Task GetByPortfolioIdAsync_ExistingPortfolio_ReturnsPortfolioBacktestRuns()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModels = new[]
//        {
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Portfolio Backtest 1"),
//            BacktestRunModelFixture.CreateRunningBacktestRun(userEntity.Id, portfolioEntity.Id, "Portfolio Backtest 2")
//        };

//        var backtestEntities = backtestModels.Select(b => b.ToEntity());
//        Context.BacktestRuns.AddRange(backtestEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByPortfolioIdAsync(portfolioEntity.Id);

//        // Assert
//        var backtestList = result.ToList();
//        Assert.Equal(2, backtestList.Count);
//        Assert.All(backtestList, b => Assert.Equal(portfolioEntity.Id, b.PortfolioId));
//    }

//    [Fact]
//    public async Task GetByStatusAsync_ExistingStatus_ReturnsBacktestRunsWithStatus()
//    {
//        // Arrange
//        var status = BacktestStatus.Completed;
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModels = new[]
//        {
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Completed Backtest 1"),
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Completed Backtest 2"),
//            BacktestRunModelFixture.CreatePendingBacktestRun(userEntity.Id, portfolioEntity.Id, "Pending Backtest")
//        };

//        var backtestEntities = backtestModels.Select(b => b.ToEntity());
//        Context.BacktestRuns.AddRange(backtestEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetByStatusAsync(status);

//        // Assert
//        var backtestList = result.ToList();
//        Assert.Equal(2, backtestList.Count);
//        Assert.All(backtestList, b => Assert.Equal(BacktestStatus.Completed, b.Status));
//    }

//    [Fact]
//    public async Task CreateAsync_ValidBacktestRun_CallsAddAndSaveChanges()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        await Context.SaveChangesAsync();

//        var backtestModel = BacktestRunModelFixture.CreatePendingBacktestRun(userEntity.Id, portfolioEntity.Id, "New Backtest");

//        // Act
//        var result = await _repository.CreateAsync(backtestModel);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(backtestModel.Name, result.Name);
//        Assert.Equal(backtestModel.Symbol, result.Symbol);
//        Assert.Equal(backtestModel.Exchange, result.Exchange);
//        Assert.Equal(backtestModel.Timeframe, result.Timeframe);
//        Assert.Equal(backtestModel.Status, result.Status);

//        // Verify it was actually saved to the database
//        var savedEntity = await Context.BacktestRuns.FindAsync(result.Id);
//        Assert.NotNull(savedEntity);
//        Assert.Equal(result.Name, savedEntity.Name);
//    }

//    //[Fact]
//    //public async Task UpdateAsync_ExistingBacktestRun_UpdatesAndSaves()
//    //{
//    //    // Arrange
//    //    var backtestId = Guid.NewGuid();
//    //    var existingBacktest = new BacktestRunEntity
//    //    {
//    //        Id = backtestId,
//    //        Name = "Original Backtest",
//    //        Status = (int)BacktestStatus.Running,
//    //        FinalBalance = 0.0m,
//    //        TotalReturnPercent = 0.0m,
//    //        TotalTrades = 0,
//    //        WinningTrades = 0,
//    //        LosingTrades = 0,
//    //        WinRatePercent = 0.0m,
//    //        ExecutionDurationTicks = 0,
//    //        ErrorMessage = string.Empty,
//    //        CompletedAt = null,
//    //        CreatedAt = DateTime.UtcNow.AddDays(-1)
//    //    };

//    //    var updatedModel = new BacktestRunModel
//    //    {
//    //        Id = backtestId,
//    //        Name = "Updated Backtest",
//    //        Status = BacktestStatus.Completed,
//    //        FinalBalance = 12000.0m,
//    //        TotalReturnPercent = 20.0m,
//    //        MaxDrawdownPercent = -5.0m,
//    //        SharpeRatio = 1.5m,
//    //        TotalTrades = 15,
//    //        WinningTrades = 10,
//    //        LosingTrades = 5,
//    //        WinRatePercent = 66.67m,
//    //        AverageTradeReturnPercent = 1.33m,
//    //        ExecutionDuration = TimeSpan.FromMinutes(10),
//    //        ErrorMessage = string.Empty,
//    //        CompletedAt = DateTime.UtcNow
//    //    };

//    //    var mockDbSet = new Mock<DbSet<BacktestRunEntity>>();
//    //    SetupFindAsync(mockDbSet, new[] { existingBacktest }, b => b.Id);

//    //    MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);
//    //    MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

//    //    // Act
//    //    var result = await _repository.UpdateAsync(updatedModel);

//    //    // Assert
//    //    Assert.NotNull(result);
//    //    Assert.Equal("Updated Backtest", result.Name);
//    //    Assert.Equal(BacktestStatus.Completed, result.Status);
//    //    Assert.Equal(12000.0m, result.FinalBalance);
//    //    Assert.Equal(20.0m, result.TotalReturnPercent);
//    //    Assert.Equal(15, result.TotalTrades);
//    //    Assert.Equal(10, result.WinningTrades);
//    //    Assert.Equal(5, result.LosingTrades);
//    //    Assert.Equal(66.67m, result.WinRatePercent);

//    //    MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
//    //}

//    [Fact]
//    public async Task UpdateAsync_NonExistentBacktestRun_ThrowsNotFoundException()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        await Context.SaveChangesAsync();

//        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Nonexistent Backtest");

//        // Act & Assert
//        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(backtestModel));
//    }

//    [Fact]
//    public async Task DeleteAsync_ExistingBacktestRun_SoftDeletesBacktestRun()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Test Backtest");
//        var backtestEntity = backtestModel.ToEntity();
//        Context.BacktestRuns.Add(backtestEntity);

//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.DeleteAsync(backtestEntity.Id);

//        // Assert
//        Assert.True(result);

//        // Verify soft delete was applied
//        var deletedEntity = await Context.BacktestRuns.FindAsync(backtestEntity.Id);
//        Assert.NotNull(deletedEntity);
//        Assert.True(deletedEntity.IsDeleted);
//        Assert.NotNull(deletedEntity.UpdatedAt);
//    }

//    [Fact]
//    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var otherUserModel = UserModelFixture.CreateDefault("OtherUser", "other@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        var otherUserEntity = otherUserModel.ToEntity();
//        Context.Users.AddRange(new[] { userEntity, otherUserEntity });

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModels = new[]
//        {
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 1"),
//            BacktestRunModelFixture.CreateRunningBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 2"),
//            BacktestRunModelFixture.CreatePendingBacktestRun(otherUserEntity.Id, portfolioEntity.Id, "Different user backtest")
//        };

//        var backtestEntities = backtestModels.Select(b => b.ToEntity());
//        Context.BacktestRuns.AddRange(backtestEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.CountByUserIdAsync(userEntity.Id);

//        // Assert
//        Assert.Equal(2, result);
//    }

//    [Fact]
//    public async Task GetAllAsync_WithBacktestRuns_ReturnsAllActiveBacktestRuns()
//    {
//        // Arrange
//        var userModel = UserModelFixture.CreateDefault("NormTheThird", "norm@quantflow.com");
//        var userEntity = userModel.ToEntity();
//        Context.Users.Add(userEntity);

//        var portfolioModel = PortfolioModelFixture.CreateActivePortfolio(userEntity.Id, "Test Portfolio");
//        var portfolioEntity = portfolioModel.ToEntity();
//        Context.Portfolios.Add(portfolioEntity);

//        var backtestModels = new[]
//        {
//            BacktestRunModelFixture.CreateCompletedBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 1"),
//            BacktestRunModelFixture.CreateRunningBacktestRun(userEntity.Id, portfolioEntity.Id, "Backtest 2")
//        };

//        var backtestEntities = backtestModels.Select(b => b.ToEntity());
//        Context.BacktestRuns.AddRange(backtestEntities);
//        await Context.SaveChangesAsync();

//        // Act
//        var result = await _repository.GetAllAsync();

//        // Assert
//        var backtestList = result.ToList();
//        Assert.Equal(2, backtestList.Count);
//        Assert.Contains(backtestList, b => b.Name == "Backtest 1");
//        Assert.Contains(backtestList, b => b.Name == "Backtest 2");
//    }
//}