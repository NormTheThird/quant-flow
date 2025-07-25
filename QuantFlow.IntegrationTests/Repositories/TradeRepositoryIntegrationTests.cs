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
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var tradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "BTCUSDT");
        var tradeEntity = tradeModel.ToEntity();
        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tradeEntity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tradeEntity.Id, result.Id);
        Assert.Equal(backtestRunEntity.Id, result.BacktestRunId);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal(TradeType.Buy, result.Type);
        Assert.Equal(tradeModel.Quantity, result.Quantity);
        Assert.Equal(tradeModel.Price, result.Price);
        Assert.Equal(tradeModel.Value, result.Value);
        Assert.Equal(tradeModel.Commission, result.Commission);
        Assert.Equal(tradeModel.NetValue, result.NetValue);
        Assert.Equal(tradeModel.PortfolioBalanceBefore, result.PortfolioBalanceBefore);
        Assert.Equal(tradeModel.PortfolioBalanceAfter, result.PortfolioBalanceAfter);
        Assert.Equal(tradeModel.AlgorithmReason, result.AlgorithmReason);
        Assert.Equal(tradeModel.AlgorithmConfidence, result.AlgorithmConfidence);
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
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var tradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id);
        var tradeEntity = tradeModel.ToEntity();
        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Soft delete the trade
        var trade = await Context.Trades.FindAsync(tradeEntity.Id);
        trade!.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tradeEntity.Id);

        // Assert
        Assert.Null(result);
    }
    [Fact]
    public async Task GetByBacktestRunIdAsync_ExistingBacktestRun_ReturnsTrades()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();

        var otherBacktestRunModel = BacktestRunModelFixture.CreateDefault(Guid.NewGuid(), portfolioEntity.Id, "Other Backtest");
        var otherBacktestRunEntity = otherBacktestRunModel.ToEntity();

        Context.BacktestRuns.AddRange(backtestRunEntity, otherBacktestRunEntity);
        await Context.SaveChangesAsync(); // Save here to get the IDs persisted

        var trade1Model = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "BTCUSDT");
        var trade1Entity = trade1Model.ToEntity();

        var trade2Model = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Sell, "BTCUSDT");
        var trade2Entity = trade2Model.ToEntity();

        var otherTradeModel = TradeModelFixture.CreateDefault(otherBacktestRunEntity.Id, TradeType.Buy, "ETHUSDT");
        var otherTradeEntity = otherTradeModel.ToEntity();

        Context.Trades.AddRange(trade1Entity, trade2Entity, otherTradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBacktestRunIdAsync(backtestRunEntity.Id);

        // Assert
        var trades = result.ToList();
        Assert.Equal(2, trades.Count);
        Assert.All(trades, t => Assert.Equal(backtestRunEntity.Id, t.BacktestRunId));
        Assert.Contains(trades, t => t.Type == TradeType.Buy);
        Assert.Contains(trades, t => t.Type == TradeType.Sell);
        Assert.DoesNotContain(trades, t => t.Symbol == "ETHUSDT");
    }

    [Fact]
    public async Task GetBySymbolAsync_ExistingSymbol_ReturnsSymbolTrades()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var btcTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "BTCUSDT");
        var btcTradeEntity = btcTradeModel.ToEntity();

        var ethTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "ETHUSDT");
        var ethTradeEntity = ethTradeModel.ToEntity();

        Context.Trades.AddRange(btcTradeEntity, ethTradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySymbolAsync(backtestRunEntity.Id, "BTCUSDT");

        // Assert
        var trades = result.ToList();
        Assert.Single(trades);
        Assert.Equal("BTCUSDT", trades[0].Symbol);
        Assert.Equal(backtestRunEntity.Id, trades[0].BacktestRunId);
    }

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsTypedTrades()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var buyTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "BTCUSDT");
        var buyTradeEntity = buyTradeModel.ToEntity();

        var sellTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Sell, "BTCUSDT");
        var sellTradeEntity = sellTradeModel.ToEntity();

        Context.Trades.AddRange(buyTradeEntity, sellTradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(backtestRunEntity.Id, TradeType.Buy);

        // Assert
        var trades = result.ToList();
        Assert.Single(trades);
        Assert.Equal(TradeType.Buy, trades[0].Type);
        Assert.Equal(backtestRunEntity.Id, trades[0].BacktestRunId);
    }

    [Fact]
    public async Task CreateAsync_ValidTrade_ReturnsCreatedTrade()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);
        await Context.SaveChangesAsync();

        var tradeModel = TradeModelFixture.CreateCustomTrade(
            backtestRunId: backtestRunEntity.Id,
            symbol: "ETHUSDT",
            type: TradeType.Buy,
            quantity: 2.0m,
            price: 3500.0m,
            algorithmReason: "Strong buy signal on ETH",
            confidence: 0.92m
        );

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
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);
        await Context.SaveChangesAsync();

        var tradeModels = TradeModelFixture.CreateTradeBatch(backtestRunEntity.Id, 2);

        // Act
        var result = await _repository.CreateBatchAsync(tradeModels);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, t => Assert.Equal(backtestRunEntity.Id, t.BacktestRunId));

        // Verify in database
        var dbTrades = await Context.Trades
            .Where(t => t.BacktestRunId == backtestRunEntity.Id)
            .ToListAsync();
        Assert.Equal(2, dbTrades.Count);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTrade_ReturnsUpdatedTrade()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var originalTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id);
        var tradeEntity = originalTradeModel.ToEntity();
        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        var updatedModel = TradeModelFixture.CreateTradeForUpdate(
            tradeId: tradeEntity.Id,
            backtestRunId: backtestRunEntity.Id,
            newType: TradeType.Sell,
            newPrice: 52000.0m,
            newReason: "Updated sell signal"
        );
        updatedModel.AlgorithmConfidence = 0.95m;
        updatedModel.RealizedProfitLoss = 194.8m;
        updatedModel.RealizedProfitLossPercent = 3.9m;

        // Act
        var result = await _repository.UpdateAsync(updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TradeType.Sell, result.Type);
        Assert.Equal(52000.0m, result.Price);
        Assert.Equal("Updated sell signal", result.AlgorithmReason);
        Assert.Equal(0.95m, result.AlgorithmConfidence);
        Assert.Equal(194.8m, result.RealizedProfitLoss);
        Assert.Equal(3.9m, result.RealizedProfitLossPercent);
        Assert.NotNull(result.UpdatedAt);

        // Verify in database
        var dbTrade = await Context.Trades.FindAsync(tradeEntity.Id);
        Assert.NotNull(dbTrade);
        Assert.Equal((int)TradeType.Sell, dbTrade.Type);
        Assert.Equal(52000.0m, dbTrade.Price);
        Assert.Equal("Updated sell signal", dbTrade.AlgorithmReason);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTrade_ThrowsNotFoundException()
    {
        // Arrange
        var tradeModel = TradeModelFixture.CreateDefault(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateAsync(tradeModel));
    }

    [Fact]
    public async Task DeleteAsync_ExistingTrade_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var tradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id);
        var tradeEntity = tradeModel.ToEntity();
        Context.Trades.Add(tradeEntity);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(tradeEntity.Id);

        // Assert
        Assert.True(result);

        // Verify soft delete
        var dbTrade = await Context.Trades.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tradeEntity.Id);
        Assert.NotNull(dbTrade);
        Assert.True(dbTrade.IsDeleted);
        Assert.NotNull(dbTrade.UpdatedAt);

        // Verify trade is not returned by normal queries
        var tradeModelResult = await _repository.GetByIdAsync(tradeEntity.Id);
        Assert.Null(tradeModelResult);
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
        var userModel = UserModelFixture.CreateDefault();
        var userEntity = userModel.ToEntity();
        Context.Users.Add(userEntity);

        var portfolioModel = PortfolioModelFixture.CreateDefault(userEntity.Id);
        var portfolioEntity = portfolioModel.ToEntity();
        Context.Portfolios.Add(portfolioEntity);

        var backtestRunModel = BacktestRunModelFixture.CreateDefault(userEntity.Id, portfolioEntity.Id);
        var backtestRunEntity = backtestRunModel.ToEntity();
        Context.BacktestRuns.Add(backtestRunEntity);

        var trade1Model = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "BTCUSDT");
        var trade1Entity = trade1Model.ToEntity();

        var trade2Model = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Sell, "ETHUSDT");
        var trade2Entity = trade2Model.ToEntity();

        var deletedTradeModel = TradeModelFixture.CreateDefault(backtestRunEntity.Id, TradeType.Buy, "ADAUSDT");
        var deletedTradeEntity = deletedTradeModel.ToEntity();

        Context.Trades.AddRange(trade1Entity, trade2Entity, deletedTradeEntity);
        await Context.SaveChangesAsync();

        // Soft delete one trade
        var deletedTrade = await Context.Trades.FindAsync(deletedTradeEntity.Id);
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