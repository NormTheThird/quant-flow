namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for TradeRepository using mocked dependencies
/// </summary>
public class TradeRepositoryUnitTests : BaseRepositoryUnitTest
{
    private readonly Mock<ILogger<TradeRepository>> _mockLogger;
    private readonly TradeRepository _repository;

    public TradeRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<TradeRepository>>();
        _repository = new TradeRepository(MockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTrade_ReturnsTradeModel()
    {
        // Arrange
        var tradeId = Guid.NewGuid();
        var backtestRunId = Guid.NewGuid();
        var trades = new List<TradeEntity>
        {
            new TradeEntity
            {
                Id = tradeId,
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
                Quantity = 0.1m,
                Price = 50000.0m,
                Value = 5000.0m,
                Commission = 5.0m,
                NetValue = 4995.0m,
                PortfolioBalanceBefore = 10000.0m,
                PortfolioBalanceAfter = 5000.0m,
                AlgorithmReason = "Buy signal detected",
                AlgorithmConfidence = 0.85m,
                RealizedProfitLoss = null,
                RealizedProfitLossPercent = null,
                EntryTradeId = null,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(trades);
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(tradeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tradeId, result.Id);
        Assert.Equal(backtestRunId, result.BacktestRunId);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal(TradeType.Buy, result.Type);
        Assert.Equal(0.1m, result.Quantity);
        Assert.Equal(50000.0m, result.Price);
        Assert.Equal(5000.0m, result.Value);
        Assert.Equal(5.0m, result.Commission);
        Assert.Equal(4995.0m, result.NetValue);
        Assert.Equal(10000.0m, result.PortfolioBalanceBefore);
        Assert.Equal(5000.0m, result.PortfolioBalanceAfter);
        Assert.Equal("Buy signal detected", result.AlgorithmReason);
        Assert.Equal(0.85m, result.AlgorithmConfidence);
        Assert.Null(result.RealizedProfitLoss);
        Assert.Null(result.RealizedProfitLossPercent);
        Assert.Null(result.EntryTradeId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTrade_ReturnsNull()
    {
        // Arrange
        var tradeId = Guid.NewGuid();
        var trades = new List<TradeEntity>();

        var mockDbSet = CreateMockDbSetWithAsync(trades);
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(tradeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBacktestRunIdAsync_ExistingBacktestRun_ReturnsTrades()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var otherBacktestRunId = Guid.NewGuid();
        var trades = new List<TradeEntity>
        {
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-2),
                Quantity = 0.1m,
                Price = 48000.0m,
                Value = 4800.0m,
                Commission = 4.8m,
                NetValue = 4795.2m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Sell,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
                Quantity = 0.1m,
                Price = 52000.0m,
                Value = 5200.0m,
                Commission = 5.2m,
                NetValue = 5194.8m,
                RealizedProfitLoss = 399.6m,
                RealizedProfitLossPercent = 8.33m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = otherBacktestRunId,
                Symbol = "ETHUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow,
                Quantity = 1.0m,
                Price = 3000.0m,
                Value = 3000.0m,
                Commission = 3.0m,
                NetValue = 2997.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var backtestTrades = trades
            .Where(t => t.BacktestRunId == backtestRunId && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp);
        var mockDbSet = CreateMockDbSetWithAsync(backtestTrades);
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByBacktestRunIdAsync(backtestRunId);

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count);
        Assert.All(tradeList, t => Assert.Equal(backtestRunId, t.BacktestRunId));
        Assert.Equal(TradeType.Buy, tradeList[0].Type); // First chronologically
        Assert.Equal(TradeType.Sell, tradeList[1].Type); // Second chronologically
    }

    [Fact]
    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolTrades()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var symbol = "BTCUSDT";
        var trades = new List<TradeEntity>
        {
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
                Quantity = 0.1m,
                Price = 50000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Sell,
                ExecutionTimestamp = DateTime.UtcNow,
                Quantity = 0.1m,
                Price = 52000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "ETHUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow,
                Quantity = 1.0m,
                Price = 3000.0m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var symbolTrades = trades
            .Where(t => t.BacktestRunId == backtestRunId && t.Symbol == symbol && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp);
        var mockDbSet = CreateMockDbSetWithAsync(symbolTrades);
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetBySymbolAsync(backtestRunId, symbol);

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count);
        Assert.All(tradeList, t => Assert.Equal(symbol, t.Symbol));
        Assert.All(tradeList, t => Assert.Equal(backtestRunId, t.BacktestRunId));
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsTypedTrades()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var tradeType = TradeType.Buy;
        var trades = new List<TradeEntity>
        {
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Sell,
                ExecutionTimestamp = DateTime.UtcNow,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var buyTrades = trades
            .Where(t => t.BacktestRunId == backtestRunId && t.Type == (int)tradeType && !t.IsDeleted)
            .OrderBy(t => t.ExecutionTimestamp);
        var mockDbSet = CreateMockDbSetWithAsync(buyTrades);
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByTypeAsync(backtestRunId, tradeType);

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count);
        Assert.All(tradeList, t => Assert.Equal(TradeType.Buy, t.Type));
        Assert.All(tradeList, t => Assert.Equal(backtestRunId, t.BacktestRunId));
    }

    [Fact]
    public async Task CreateAsync_ValidTrade_CallsAddAndSaveChanges()
    {
        // Arrange
        var tradeModel = new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Type = TradeType.Buy,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 0.05m,
            Price = 45000.0m,
            Value = 2250.0m,
            Commission = 2.25m,
            NetValue = 2247.75m,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = 7752.25m,
            AlgorithmReason = "Strong buy signal",
            AlgorithmConfidence = 0.92m
        };

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.CreateAsync(tradeModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tradeModel.Symbol, result.Symbol);
        Assert.Equal(tradeModel.Type, result.Type);
        Assert.Equal(tradeModel.Quantity, result.Quantity);
        Assert.Equal(tradeModel.Price, result.Price);
        Assert.Equal(tradeModel.Value, result.Value);
        Assert.Equal(tradeModel.Commission, result.Commission);
        Assert.Equal(tradeModel.NetValue, result.NetValue);
        Assert.Equal(tradeModel.AlgorithmReason, result.AlgorithmReason);
        Assert.Equal(tradeModel.AlgorithmConfidence, result.AlgorithmConfidence);

        mockDbSet.Verify(m => m.Add(It.IsAny<TradeEntity>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateBatchAsync_ValidTrades_CallsAddRangeAndSaveChanges()
    {
        // Arrange
        var tradeModels = new List<TradeModel>
        {
            new TradeModel
            {
                Id = Guid.NewGuid(),
                BacktestRunId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Type = TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddMinutes(-30),
                Quantity = 0.1m,
                Price = 50000.0m,
                Value = 5000.0m,
                Commission = 5.0m,
                NetValue = 4995.0m
            },
            new TradeModel
            {
                Id = Guid.NewGuid(),
                BacktestRunId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Type = TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow,
                Quantity = 1.0m,
                Price = 3000.0m,
                Value = 3000.0m,
                Commission = 3.0m,
                NetValue = 2997.0m
            }
        };

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(2);

        // Act
        var result = await _repository.CreateBatchAsync(tradeModels);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(tradeModels[0].Symbol, resultList[0].Symbol);
        Assert.Equal(tradeModels[1].Symbol, resultList[1].Symbol);

        mockDbSet.Verify(m => m.AddRange(It.IsAny<IEnumerable<TradeEntity>>()), Times.Once);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTrade_UpdatesAndSaves()
    {
        // Arrange
        var tradeId = Guid.NewGuid();
        var existingTrade = new TradeEntity
        {
            Id = tradeId,
            BacktestRunId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Type = (int)TradeType.Buy,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 0.1m,
            Price = 50000.0m,
            Value = 5000.0m,
            Commission = 5.0m,
            NetValue = 4995.0m,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = 5000.0m,
            AlgorithmReason = "Original reason",
            AlgorithmConfidence = 0.8m,
            RealizedProfitLoss = null,
            RealizedProfitLossPercent = null,
            EntryTradeId = null,
            CreatedAt = DateTime.UtcNow
        };

        var updatedModel = new TradeModel
        {
            Id = tradeId,
            BacktestRunId = existingTrade.BacktestRunId,
            Symbol = "BTCUSDT",
            Type = TradeType.Sell,
            ExecutionTimestamp = DateTime.UtcNow.AddMinutes(30),
            Quantity = 0.1m,
            Price = 52000.0m,
            Value = 5200.0m,
            Commission = 5.2m,
            NetValue = 5194.8m,
            PortfolioBalanceBefore = 5000.0m,
            PortfolioBalanceAfter = 10194.8m,
            AlgorithmReason = "Updated sell signal",
            AlgorithmConfidence = 0.95m,
            RealizedProfitLoss = 194.8m,
            RealizedProfitLossPercent = 3.9m,
            EntryTradeId = Guid.NewGuid()
        };

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingTrade }, t => t.Id);

        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TradeType.Sell, result.Type);
        Assert.Equal(52000.0m, result.Price);
        Assert.Equal(5200.0m, result.Value);
        Assert.Equal(5194.8m, result.NetValue);
        Assert.Equal("Updated sell signal", result.AlgorithmReason);
        Assert.Equal(0.95m, result.AlgorithmConfidence);
        Assert.Equal(194.8m, result.RealizedProfitLoss);
        Assert.Equal(3.9m, result.RealizedProfitLossPercent);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTrade_ThrowsNotFoundException()
    {
        // Arrange
        var tradeModel = new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Type = TradeType.Buy,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 0.1m,
            Price = 50000.0m,
            Value = 5000.0m,
            Commission = 5.0m,
            NetValue = 4995.0m
        };

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<TradeEntity>(), t => t.Id);

        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(tradeModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingTrade_SoftDeletesTrade()
    {
        // Arrange
        var tradeId = Guid.NewGuid();
        var existingTrade = new TradeEntity
        {
            Id = tradeId,
            BacktestRunId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Type = (int)TradeType.Buy,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        SetupFindAsync(mockDbSet, new[] { existingTrade }, t => t.Id);

        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(tradeId);

        // Assert
        Assert.True(result);
        Assert.True(existingTrade.IsDeleted);
        Assert.NotNull(existingTrade.UpdatedAt);

        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTrade_ReturnsFalse()
    {
        // Arrange
        var tradeId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<TradeEntity>>();
        SetupFindAsync(mockDbSet, Enumerable.Empty<TradeEntity>(), t => t.Id);

        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.DeleteAsync(tradeId);

        // Assert
        Assert.False(result);
        MockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WithTrades_ReturnsAllActiveTrades()
    {
        // Arrange
        var trades = new List<TradeEntity>
        {
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = Guid.NewGuid(),
                Symbol = "BTCUSDT",
                Type = (int)TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TradeEntity
            {
                Id = Guid.NewGuid(),
                BacktestRunId = Guid.NewGuid(),
                Symbol = "ETHUSDT",
                Type = (int)TradeType.Sell,
                ExecutionTimestamp = DateTime.UtcNow,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = CreateMockDbSetWithAsync(trades.OrderBy(t => t.ExecutionTimestamp));
        MockContext.Setup(c => c.Trades).Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count);
        Assert.Contains(tradeList, t => t.Symbol == "BTCUSDT");
        Assert.Contains(tradeList, t => t.Symbol == "ETHUSDT");
    }
}