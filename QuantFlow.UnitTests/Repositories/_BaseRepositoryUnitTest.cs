namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Base class for repository unit tests using in-memory database
/// </summary>
public abstract class BaseRepositoryUnitTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<ILogger> MockLogger;
    private bool _disposed = false;

    protected BaseRepositoryUnitTest()
    {
        // Create in-memory database with unique name for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging() // Helpful for debugging
            .Options;

        Context = new ApplicationDbContext(options);
        MockLogger = new Mock<ILogger>();

        // Ensure the database is created
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Seeds a test user and returns the user ID
    /// </summary>
    /// <param name="userName">Optional user name</param>
    /// <returns>User ID</returns>
    protected static async Task<Guid> SeedTestUserAsync(string userName = "TestUser")
    {
        var userId = Guid.NewGuid();
        await Task.Delay(1); // Simulate async operation
        // If you have a User entity, add it here
        // For now, just return the ID as many tests just need the foreign key
        return userId;
    }

    /// <summary>
    /// Seeds a test portfolio and returns the portfolio ID
    /// </summary>
    /// <param name="userId">User ID for the portfolio</param>
    /// <param name="portfolioName">Optional portfolio name</param>
    /// <returns>Portfolio ID</returns>
    protected static async Task<Guid> SeedTestPortfolioAsync(Guid userId, string portfolioName = "TestPortfolio")
    {
        var portfolioId = Guid.NewGuid();
        await Task.Delay(1); // Simulate async operation
        // If you have a Portfolio entity, add it here
        // For now, just return the ID as many tests just need the foreign key
        return portfolioId;
    }

    /// <summary>
    /// Seeds a test algorithm and returns the algorithm ID
    /// </summary>
    /// <param name="algorithmName">Optional algorithm name</param>
    /// <returns>Algorithm ID</returns>
    protected static async Task<Guid> SeedTestAlgorithmAsync(string algorithmName = "TestAlgorithm")
    {
        var algorithmId = Guid.NewGuid();
        await Task.Delay(1); // Simulate async operation
        // If you have an Algorithm entity, add it here
        // For now, just return the ID as many tests just need the foreign key
        return algorithmId;
    }

    /// <summary>
    /// Clears all data from the database context
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        // Remove all BacktestRuns
        Context.BacktestRuns.RemoveRange(Context.BacktestRuns);

        // Add other entities as needed
        // Context.Users.RemoveRange(Context.Users);
        // Context.Portfolios.RemoveRange(Context.Portfolios);

        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the database context
    /// </summary>
    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    /// <param name="disposing">Whether we're disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Context?.Database.EnsureDeleted(); // Clean up the in-memory database
            Context?.Dispose();
            _disposed = true;
        }
    }
}