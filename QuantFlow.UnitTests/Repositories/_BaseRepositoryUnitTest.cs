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