namespace QuantFlow.Test.Integration.Repositories;

/// <summary>
/// Integration tests for TradeRepository with in-memory database
/// </summary>
public class TradeRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly TradeRepository _repository;

    public TradeRepositoryIntegrationTests()
    {
        var logger = Substitute.For<ILogger<TradeRepository>>();
        _repository = new TradeRepository(Context, logger);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTrade_ReturnsTradeModel()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);
        var tradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "BTCUSDT");

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
        Assert.Equal("Test trade", result.AlgorithmReason);
        Assert.Equal(0.85m, result.AlgorithmConfidence);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTrade_ReturnsNull()
    {
        // Arrange
        var tradeId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(tradeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedTrade_ReturnsNull()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);
        var tradeId = await SeedTestTradeAsync(backtestRunId);

        // Soft delete the trade
        var trade = await Context.Trades.FindAsync(tradeId);
        trade!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tradeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBacktestRunIdAsync_ExistingBacktestRun_ReturnsTrades()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);
        var otherBacktestRunId = await SeedTestBacktestRunAsync(userId, portfolioId, "Other Backtest");

        var trade1Id = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "BTCUSDT");
        var trade2Id = await SeedTestTradeAsync(backtestRunId, TradeType.Sell, "BTCUSDT");
        var otherTradeId = await SeedTestTradeAsync(otherBacktestRunId, TradeType.Buy, "ETHUSDT");

        // Act
        var result = await _repository.GetByBacktestRunIdAsync(backtestRunId);

        // Assert
        var trades = result.ToList();
        Assert.Equal(2, trades.Count);
        Assert.All(trades, t => Assert.Equal(backtestRunId, t.BacktestRunId));
        Assert.Contains(trades, t => t.Type == TradeType.Buy);
        Assert.Contains(trades, t => t.Type == TradeType.Sell);
        Assert.DoesNotContain(trades, t => t.Symbol == "ETHUSDT");
    }

    [Fact]
    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolTrades()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);

        var btcTradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "BTCUSDT");
        var ethTradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "ETHUSDT");

        // Act
        var result = await _repository.GetBySymbolAsync(backtestRunId, "BTCUSDT");

        // Assert
        var trades = result.ToList();
        Assert.Single(trades);
        Assert.Equal("BTCUSDT", trades[0].Symbol);
        Assert.Equal(backtestRunId, trades[0].BacktestRunId);
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsTypedTrades()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);

        var buyTradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "BTCUSDT");
        var sellTradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Sell, "BTCUSDT");

        // Act
        var result = await _repository.GetByTypeAsync(backtestRunId, TradeType.Buy);

        // Assert
        var trades = result.ToList();
        Assert.Single(trades);
        Assert.Equal(TradeType.Buy, trades[0].Type);
        Assert.Equal(backtestRunId, trades[0].BacktestRunId);
    }

    [Fact]
    public async Task CreateAsync_ValidTrade_ReturnsCreatedTrade()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);

        var tradeModel = new TradeModel
        {
            Id = Guid.NewGuid(),
            BacktestRunId = backtestRunId,
            Symbol = "ETHUSDT",
            Type = TradeType.Buy,
            ExecutionTimestamp = DateTime.UtcNow,
            Quantity = 2.0m,
            Price = 3500.0m,
            Value = 7000.0m,
            Commission = 7.0m,
            NetValue = 6993.0m,
            PortfolioBalanceBefore = 10000.0m,
            PortfolioBalanceAfter = 3000.0m,
            AlgorithmReason = "Strong buy signal on ETH",
            AlgorithmConfidence = 0.92m
        };

        // Act
        var result = await _repository.CreateAsync(tradeModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tradeModel.BacktestRunId, result.BacktestRunId);
        Assert.Equal(tradeModel.Symbol, result.Symbol);
        Assert.Equal(tradeModel.Type, result.Type);
        Assert.Equal(tradeModel.Quantity, result.Quantity);
        Assert.Equal(tradeModel.Price, result.Price);
        Assert.Equal(tradeModel.Value, result.Value);
        Assert.Equal(tradeModel.Commission, result.Commission);
        Assert.Equal(tradeModel.NetValue, result.NetValue);
        Assert.Equal(tradeModel.AlgorithmReason, result.AlgorithmReason);
        Assert.Equal(tradeModel.AlgorithmConfidence, result.AlgorithmConfidence);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbTrade = await Context.Trades.FindAsync(result.Id);
        Assert.NotNull(dbTrade);
        Assert.Equal(tradeModel.Symbol, dbTrade.Symbol);
        Assert.Equal((int)tradeModel.Type, dbTrade.Type);
    }

    [Fact]
    public async Task CreateBatchAsync_ValidTrades_ReturnsCreatedTrades()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);

        var tradeModels = new List<TradeModel>
        {
            new TradeModel
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "BTCUSDT",
                Type = TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow.AddMinutes(-30),
                Quantity = 0.1m,
                Price = 50000.0m,
                Value = 5000.0m,
                Commission = 5.0m,
                NetValue = 4995.0m,
                PortfolioBalanceBefore = 10000.0m,
                PortfolioBalanceAfter = 5000.0m,
                AlgorithmReason = "Buy signal 1"
            },
            new TradeModel
            {
                Id = Guid.NewGuid(),
                BacktestRunId = backtestRunId,
                Symbol = "ETHUSDT",
                Type = TradeType.Buy,
                ExecutionTimestamp = DateTime.UtcNow,
                Quantity = 1.0m,
                Price = 3000.0m,
                Value = 3000.0m,
                Commission = 3.0m,
                NetValue = 2997.0m,
                PortfolioBalanceBefore = 5000.0m,
                PortfolioBalanceAfter = 2000.0m,
                AlgorithmReason = "Buy signal 2"
            }
        };

        // Act
        var result = await _repository.CreateBatchAsync(tradeModels);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, t => t.Symbol == "BTCUSDT");
        Assert.Contains(resultList, t => t.Symbol == "ETHUSDT");

        // Verify in database
        var dbTrades = await Context.Trades
            .Where(t => t.BacktestRunId == backtestRunId)
            .ToListAsync();
        Assert.Equal(2, dbTrades.Count);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTrade_ReturnsUpdatedTrade()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);
        var tradeId = await SeedTestTradeAsync(backtestRunId);

        var updatedModel = new TradeModel
        {
            Id = tradeId,
            BacktestRunId = backtestRunId,
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
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbTrade = await Context.Trades.FindAsync(tradeId);
        Assert.NotNull(dbTrade);
        Assert.Equal((int)TradeType.Sell, dbTrade.Type);
        Assert.Equal(52000.0m, dbTrade.Price);
        Assert.Equal("Updated sell signal", dbTrade.AlgorithmReason);
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

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(tradeModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingTrade_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);
        var tradeId = await SeedTestTradeAsync(backtestRunId);

        // Act
        var result = await _repository.DeleteAsync(tradeId);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbTrade = await Context.Trades.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tradeId);
        Assert.NotNull(dbTrade);
        Assert.True(dbTrade.IsDeleted);
        Assert.NotNull(dbTrade.UpdatedAt);

        // Verify trade is not returned by normal queries
        var tradeModel = await _repository.GetByIdAsync(tradeId);
        Assert.Null(tradeModel);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTrade_ReturnsFalse()
    {
        // Arrange
        var tradeId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(tradeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_WithTrades_ReturnsAllActiveTrades()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        var portfolioId = await SeedTestPortfolioAsync(userId);
        var backtestRunId = await SeedTestBacktestRunAsync(userId, portfolioId);

        var trade1Id = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "BTCUSDT");
        var trade2Id = await SeedTestTradeAsync(backtestRunId, TradeType.Sell, "ETHUSDT");
        var deletedTradeId = await SeedTestTradeAsync(backtestRunId, TradeType.Buy, "ADAUSDT");

        // Soft delete one trade
        var deletedTrade = await Context.Trades.FindAsync(deletedTradeId);
        deletedTrade!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var trades = result.ToList();
        Assert.Equal(2, trades.Count); // Only active trades
        Assert.Contains(trades, t => t.Symbol == "BTCUSDT");
        Assert.Contains(trades, t => t.Symbol == "ETHUSDT");
        Assert.DoesNotContain(trades, t => t.Symbol == "ADAUSDT");
    }

    [Fact]
    public async Task GetAllAsync_NoTrades_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }
}