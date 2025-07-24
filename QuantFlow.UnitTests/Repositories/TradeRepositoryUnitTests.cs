namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Unit tests for TradeRepository using in-memory database
/// </summary>
public class TradeRepositoryUnitTests : BaseRepositoryUnitTest, IDisposable
{
    private readonly Mock<ILogger<TradeRepository>> _mockLogger;
    private readonly TradeRepository _repository;

    public TradeRepositoryUnitTests()
    {
        _mockLogger = new Mock<ILogger<TradeRepository>>();
        _repository = new TradeRepository(Context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTrade_ReturnsTradeModel()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var tradeModel = TradeModelFixture.CreateDefaultBuyTrade(backtestRunId, "BTCUSDT");
        var tradeEntity = tradeModel.ToEntity();

        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tradeEntity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tradeEntity.Id, result.Id);
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

        var (buyTrade, sellTrade) = TradeModelFixture.CreateProfitableTradePair(backtestRunId, "BTCUSDT");
        var otherTrade = TradeModelFixture.CreateDefaultBuyTrade(otherBacktestRunId, "ETHUSDT");

        var tradeEntities = new[] { buyTrade.ToEntity(), sellTrade.ToEntity(), otherTrade.ToEntity() };
        Context.Trades.AddRange(tradeEntities);
        await Context.SaveChangesAsync();

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

        var trades = TradeModelFixture.CreateMultiSymbolTradeBatch(backtestRunId);
        var tradeEntities = trades.Select(t => t.ToEntity());
        Context.Trades.AddRange(tradeEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySymbolAsync(backtestRunId, symbol);

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count); // Should only return BTCUSDT trades
        Assert.All(tradeList, t => Assert.Equal(symbol, t.Symbol));
        Assert.All(tradeList, t => Assert.Equal(backtestRunId, t.BacktestRunId));
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsTypedTrades()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var tradeType = TradeType.Buy;

        var trades = TradeModelFixture.CreateTradeBatch(backtestRunId, 5, "BTCUSDT", alternateTypes: true);
        var tradeEntities = trades.Select(t => t.ToEntity());
        Context.Trades.AddRange(tradeEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(backtestRunId, tradeType);

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(3, tradeList.Count); // Should be 3 buy trades out of 5 total (alternating)
        Assert.All(tradeList, t => Assert.Equal(TradeType.Buy, t.Type));
        Assert.All(tradeList, t => Assert.Equal(backtestRunId, t.BacktestRunId));
    }

    [Fact]
    public async Task CreateAsync_ValidTrade_CallsAddAndSaveChanges()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var tradeModel = TradeModelFixture.CreateCustomTrade(
            backtestRunId: backtestRunId,
            symbol: "BTCUSDT",
            type: TradeType.Buy,
            quantity: 0.05m,
            price: 45000.0m,
            algorithmReason: "Strong buy signal",
            confidence: 0.92m
        );

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

        // Verify it was actually saved to the database
        var savedEntity = await Context.Trades.FindAsync(result.Id);
        Assert.NotNull(savedEntity);
        Assert.Equal(result.Symbol, savedEntity.Symbol);
    }

    [Fact]
    public async Task CreateBatchAsync_ValidTrades_CallsAddRangeAndSaveChanges()
    {
        // Arrange
        var backtestRunId1 = Guid.NewGuid();
        var backtestRunId2 = Guid.NewGuid();

        var tradeModels = new List<TradeModel>
        {
            TradeModelFixture.CreateCustomTrade(backtestRunId1, "BTCUSDT", TradeType.Buy, 0.1m, 50000.0m),
            TradeModelFixture.CreateCustomTrade(backtestRunId2, "ETHUSDT", TradeType.Buy, 1.0m, 3000.0m)
        };

        // Act
        var result = await _repository.CreateBatchAsync(tradeModels);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(tradeModels[0].Symbol, resultList[0].Symbol);
        Assert.Equal(tradeModels[1].Symbol, resultList[1].Symbol);

        // Verify they were actually saved to the database
        var savedTrades = Context.Trades.ToList();
        Assert.Equal(2, savedTrades.Count);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTrade_UpdatesAndSaves()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var originalTrade = TradeModelFixture.CreateDefaultBuyTrade(backtestRunId, "BTCUSDT");
        var tradeEntity = originalTrade.ToEntity();

        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Get the saved trade and create update model
        var savedTrade = await _repository.GetByIdAsync(tradeEntity.Id);
        var updatedModel = TradeModelFixture.CreateTradeForUpdate(
            savedTrade.Id,
            backtestRunId,
            TradeType.Sell,
            52000.0m,
            "Updated sell signal"
        );

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

        // Verify the changes were persisted
        var updatedEntity = await Context.Trades.FindAsync(result.Id);
        Assert.NotNull(updatedEntity);
        Assert.Equal((int)TradeType.Sell, updatedEntity.Type);
        Assert.Equal(52000.0m, updatedEntity.Price);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTrade_ThrowsNotFoundException()
    {
        // Arrange
        var tradeModel = TradeModelFixture.CreateDefaultBuyTrade();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(tradeModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingTrade_SoftDeletesTrade()
    {
        // Arrange
        var tradeModel = TradeModelFixture.CreateDefaultBuyTrade();
        var tradeEntity = tradeModel.ToEntity();

        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(tradeEntity.Id);

        // Assert
        Assert.True(result);

        // Verify soft delete was applied
        var deletedEntity = await Context.Trades.FindAsync(tradeEntity.Id);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.UpdatedAt);
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
        var trades = TradeModelFixture.CreateTradeBatch(count: 2, alternateTypes: false);
        var tradeEntities = trades.Select(t => t.ToEntity());

        Context.Trades.AddRange(tradeEntities);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var tradeList = result.ToList();
        Assert.Equal(2, tradeList.Count);
        Assert.All(tradeList, t => Assert.Equal("BTCUSDT", t.Symbol));
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