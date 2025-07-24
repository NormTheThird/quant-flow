namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for BacktestRunRepository using mocked dependencies
/// </summary>
public class BacktestRunRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<BacktestRunRepository>> _mockLogger;
    private readonly BacktestRunRepository _repository;

    public BacktestRunRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<BacktestRunRepository>>();
        _repository = new BacktestRunRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBacktestRun_ReturnsBacktestRunModel()
    {
        // Arrange
        var backtestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity
            {
                Id = backtestId,
                Name = "Test Backtest",
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
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(backtestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(backtestId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestId, result.Id);
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
        var backtestRuns = new List<BacktestRunEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(backtestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(backtestId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsUserBacktestRuns()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Backtest 1",
                UserId = userId,
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Completed,
                InitialBalance = 10000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Backtest 2",
                UserId = userId,
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Running,
                InitialBalance = 5000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Other User Backtest",
                UserId = otherUserId,
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "ADAUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Pending,
                InitialBalance = 20000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var userBacktestRuns = backtestRuns
            .Where(b => b.UserId == userId && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt);
        var mockDbSet = CreateMockDbSetWithAsync(userBacktestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

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
        var portfolioId = Guid.NewGuid();
        var otherPortfolioId = Guid.NewGuid();
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio Backtest 1",
                PortfolioId = portfolioId,
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Completed,
                InitialBalance = 10000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Portfolio Backtest 2",
                PortfolioId = portfolioId,
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Running,
                InitialBalance = 5000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var portfolioBacktestRuns = backtestRuns
            .Where(b => b.PortfolioId == portfolioId && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt);
        var mockDbSet = CreateMockDbSetWithAsync(portfolioBacktestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

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
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Completed Backtest 1",
                Status = (int)BacktestStatus.Completed,
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                InitialBalance = 10000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Completed Backtest 2",
                Status = (int)BacktestStatus.Completed,
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                InitialBalance = 5000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var completedBacktestRuns = backtestRuns
            .Where(b => b.Status == (int)status && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt);
        var mockDbSet = CreateMockDbSetWithAsync(completedBacktestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

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
        var backtestModel = new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = "New Backtest",
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Exchange = Exchange.Binance,
            Timeframe = Timeframe.OneHour,
            BacktestStartDate = DateTime.UtcNow.AddDays(-30),
            BacktestEndDate = DateTime.UtcNow.AddDays(-1),
            Status = BacktestStatus.Pending,
            InitialBalance = 10000.0m,
            AlgorithmParameters = "{\"param1\": \"value1\"}",
            CommissionRate = 0.001m
        };

        var mockDbSet = new Mock<DbSet<BacktestRunEntity>>();
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.CreateAsync(backtestModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(backtestModel.Name, result.Name);
        Assert.Equal(backtestModel.Symbol, result.Symbol);
        Assert.Equal(backtestModel.Exchange, result.Exchange);
        Assert.Equal(backtestModel.Timeframe, result.Timeframe);
        Assert.Equal(backtestModel.Status, result.Status);

        mockDbSet.Verify(m => m.Add(It.IsAny<BacktestRunEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
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
        var backtestModel = new BacktestRunModel
        {
            Id = Guid.NewGuid(),
            Name = "Nonexistent Backtest",
            AlgorithmId = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Exchange = Exchange.Binance,
            Timeframe = Timeframe.OneHour,
            Status = BacktestStatus.Completed
        };

        var mockDbSet = new Mock<DbSet<BacktestRunEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<BacktestRunEntity>(), b => b.Id);

        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(backtestModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingBacktestRun_SoftDeletesBacktestRun()
    {
        // Arrange
        var backtestId = Guid.NewGuid();
        var existingBacktest = new BacktestRunEntity
        {
            Id = backtestId,
            Name = "Test Backtest",
            Status = (int)BacktestStatus.Completed,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<BacktestRunEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingBacktest }, b => b.Id);

        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(backtestId);

        // Assert
        Assert.True(result);
        Assert.True(existingBacktest.IsDeleted);
        Assert.NotNull(existingBacktest.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new BacktestRunEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new BacktestRunEntity { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true }, // Deleted
            new BacktestRunEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsDeleted = false } // Different user
        };

        var userBacktestRuns = backtestRuns.Where(b => b.UserId == userId && !b.IsDeleted);
        var mockDbSet = CreateMockDbSetWithAsync(userBacktestRuns);
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.CountByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetAllAsync_WithBacktestRuns_ReturnsAllActiveBacktestRuns()
    {
        // Arrange
        var backtestRuns = new List<BacktestRunEntity>
        {
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Backtest 1",
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Completed,
                InitialBalance = 10000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new BacktestRunEntity
            {
                Id = Guid.NewGuid(),
                Name = "Backtest 2",
                UserId = Guid.NewGuid(),
                AlgorithmId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Exchange = (int)Exchange.Binance,
                Timeframe = (int)Timeframe.OneHour,
                Status = (int)BacktestStatus.Running,
                InitialBalance = 5000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(backtestRuns.OrderByDescending(b => b.CreatedAt));
        MockContext.Setup(c => c.BacktestRuns).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var backtestList = result.ToList();
        Assert.Equal(2, backtestList.Count);
        Assert.Contains(backtestList, b => b.Name == "Backtest 1");
        Assert.Contains(backtestList, b => b.Name == "Backtest 2");
    }
}