//namespace QuantFlow.Test.Integration.Repositories;

///// <summary>
///// Base class for repository integration tests with in-memory database
///// </summary>
//public abstract class BaseRepositoryIntegrationTest : IDisposable
//{
//    protected readonly ApplicationDbContext Context;
//    protected readonly ILogger MockLogger;
//    private readonly string _databaseName;

//    protected BaseRepositoryIntegrationTest()
//    {
//        _databaseName = Guid.NewGuid().ToString();

//        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//            .UseInMemoryDatabase(databaseName: _databaseName)
//            .EnableSensitiveDataLogging()
//            .Options;

//        Context = new ApplicationDbContext(options);
//        MockLogger = Substitute.For<ILogger>();

//        // Ensure database is created
//        Context.Database.EnsureCreated();
//    }

//    /// <summary>
//    /// Clears all data from the context
//    /// </summary>
//    protected void ClearDatabase()
//    {
//        Context.Trades.RemoveRange(Context.Trades);
//        Context.BacktestRuns.RemoveRange(Context.BacktestRuns);
//        Context.Portfolios.RemoveRange(Context.Portfolios);
//        Context.Subscriptions.RemoveRange(Context.Subscriptions);
//        Context.ExchangeSymbols.RemoveRange(Context.ExchangeSymbols);
//        Context.Symbols.RemoveRange(Context.Symbols);
//        Context.Users.RemoveRange(Context.Users);
//        Context.SaveChanges();
//    }

//    public virtual void Dispose()
//    {
//        Context.Dispose();
//    }
//}