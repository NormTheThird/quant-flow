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
        var user = UserModelFixture.CreateDefault();
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        Context.Users.Add(user.ToEntity());
        Context.Portfolios.Add(portfolio.ToEntity());
        await Context.SaveChangesAsync();

        var backtestModel = BacktestRunModelFixture.CreateCompletedBacktestRun(user.Id, portfolio.Id, "Integration Test Backtest");

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
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(portfolio.Id, result.PortfolioId);
        Assert.Equal("BTCUSDT", result.Symbol);
        Assert.Equal(Exchange.Kraken, result.Exchange);
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
        var user = UserModelFixture.CreateDefault();
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        Context.Users.Add(user.ToEntity());
        Context.Portfolios.Add(portfolio.ToEntity());
        await Context.SaveChangesAsync();

        var backtestModel = BacktestRunModelFixture.CreateDefault();
        backtestModel.UserId = user.Id;
        backtestModel.PortfolioId = portfolio.Id;

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
        var user = UserModelFixture.CreateDefault();
        var otherUser = UserModelFixture.CreateDefault("otheruser", "other@example.com");
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        var otherPortfolio = PortfolioModelFixture.CreateDefault(otherUser.Id, "Other Portfolio");

        Context.Users.AddRange(user.ToEntity(), otherUser.ToEntity());
        Context.Portfolios.AddRange(portfolio.ToEntity(), otherPortfolio.ToEntity());
        await Context.SaveChangesAsync();

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "User Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "User Backtest 2");
        var otherBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(otherUser.Id, otherPortfolio.Id, "Other User Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(backtest1.ToEntity(), backtest2.ToEntity(), otherBacktest.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count);
        Assert.All(backtestRuns, b => Assert.Equal(user.Id, b.UserId));
        Assert.Contains(backtestRuns, b => b.Name == "User Backtest 1");
        Assert.Contains(backtestRuns, b => b.Name == "User Backtest 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Other User Backtest");
    }

    [Fact]
    public async Task GetByPortfolioIdAsync_ExistingPortfolio_ReturnsPortfolioBacktestRuns()
    {
        // Arrange
        var user1 = UserModelFixture.CreateDefault("user1", "user1@example.com");
        var user2 = UserModelFixture.CreateDefault("user2", "user2@example.com");
        var portfolio1 = PortfolioModelFixture.CreateDefault(user1.Id, "Portfolio 1");
        var portfolio2 = PortfolioModelFixture.CreateDefault(user2.Id, "Portfolio 2");

        Context.Users.AddRange(user1.ToEntity(), user2.ToEntity());
        Context.Portfolios.AddRange(portfolio1.ToEntity(), portfolio2.ToEntity());
        await Context.SaveChangesAsync();

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user1.Id, portfolio1.Id, "Portfolio 1 Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user1.Id, portfolio1.Id, "Portfolio 1 Backtest 2");
        var backtest3 = BacktestRunModelFixture.CreateCustomBacktestRun(user2.Id, portfolio2.Id, "Portfolio 2 Backtest");

        // Add to context using mapping extensions
        Context.BacktestRuns.AddRange(
            backtest1.ToEntity(),
            backtest2.ToEntity(),
            backtest3.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPortfolioIdAsync(portfolio1.Id);

        // Assert
        var backtestRuns = result.ToList();
        Assert.Equal(2, backtestRuns.Count);
        Assert.All(backtestRuns, b => Assert.Equal(portfolio1.Id, b.PortfolioId));
        Assert.Contains(backtestRuns, b => b.Name == "Portfolio 1 Backtest 1");
        Assert.Contains(backtestRuns, b => b.Name == "Portfolio 1 Backtest 2");
        Assert.DoesNotContain(backtestRuns, b => b.Name == "Portfolio 2 Backtest");
    }

    [Fact]
    public async Task GetByStatusAsync_ExistingStatus_ReturnsBacktestRunsWithStatus()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        Context.Users.Add(user.ToEntity());
        Context.Portfolios.Add(portfolio.ToEntity());
        await Context.SaveChangesAsync();

        // Create backtest runs with different statuses
        var completedBacktest1 = BacktestRunModelFixture.CreateCompletedBacktestRun(user.Id, portfolio.Id, "Completed 1");
        var completedBacktest2 = BacktestRunModelFixture.CreateCompletedBacktestRun(user.Id, portfolio.Id, "Completed 2");
        var runningBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "Running Backtest", status: BacktestStatus.Running);

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
        var user = UserModelFixture.CreateDefault();
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        Context.Users.Add(user.ToEntity());
        Context.Portfolios.Add(portfolio.ToEntity());
        await Context.SaveChangesAsync();

        var backtestModel = BacktestRunModelFixture.CreatePendingBacktestRun(user.Id, portfolio.Id, "New Integration Backtest");

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
        var user = UserModelFixture.CreateDefault();
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        Context.Users.Add(user.ToEntity());
        Context.Portfolios.Add(portfolio.ToEntity());
        await Context.SaveChangesAsync();

        var backtestModel = BacktestRunModelFixture.CreateDefault();
        backtestModel.UserId = user.Id;
        backtestModel.PortfolioId = portfolio.Id;

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
        var user = UserModelFixture.CreateDefault();
        var otherUser = UserModelFixture.CreateDefault("otheruser", "other@example.com");
        var portfolio = PortfolioModelFixture.CreateDefault(user.Id);
        var otherPortfolio = PortfolioModelFixture.CreateDefault(otherUser.Id, "Other Portfolio");

        Context.Users.AddRange(user.ToEntity(), otherUser.ToEntity());
        Context.Portfolios.AddRange(portfolio.ToEntity(), otherPortfolio.ToEntity());
        await Context.SaveChangesAsync();

        // Create backtest runs for the user
        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "Backtest 2");
        var deletedBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(user.Id, portfolio.Id, "Deleted Backtest");
        var otherBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(otherUser.Id, otherPortfolio.Id, "Other User Backtest");

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
        var result = await _repository.CountByUserIdAsync(user.Id);

        // Assert
        Assert.Equal(2, result); // Only active backtest runs for the user
    }

    [Fact]
    public async Task CountByUserIdAsync_UserWithNoBacktestRuns_ReturnsZero()
    {
        // Arrange
        var user = UserModelFixture.CreateDefault();
        Context.Users.Add(user.ToEntity());
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.CountByUserIdAsync(user.Id);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetAllAsync_WithBacktestRuns_ReturnsAllActiveBacktestRuns()
    {
        // Arrange
        var user1 = UserModelFixture.CreateDefault("user1", "user1@example.com");
        var user2 = UserModelFixture.CreateDefault("user2", "user2@example.com");
        var portfolio1 = PortfolioModelFixture.CreateDefault(user1.Id, "Portfolio 1");
        var portfolio2 = PortfolioModelFixture.CreateDefault(user2.Id, "Portfolio 2");

        Context.Users.AddRange(user1.ToEntity(), user2.ToEntity());
        Context.Portfolios.AddRange(portfolio1.ToEntity(), portfolio2.ToEntity());
        await Context.SaveChangesAsync();

        var backtest1 = BacktestRunModelFixture.CreateCustomBacktestRun(user1.Id, portfolio1.Id, "Backtest 1");
        var backtest2 = BacktestRunModelFixture.CreateCustomBacktestRun(user2.Id, portfolio2.Id, "Backtest 2");
        var deletedBacktest = BacktestRunModelFixture.CreateCustomBacktestRun(user1.Id, portfolio1.Id, "Deleted Backtest");

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