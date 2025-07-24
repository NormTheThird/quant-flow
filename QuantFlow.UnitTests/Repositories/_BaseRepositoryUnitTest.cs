namespace QuantFlow.Test.Unit.Repositories;

/// <summary>
/// Base class for repository unit tests with mocked dependencies
/// </summary>
public abstract class BaseRepositoryUnitTest
{
    protected readonly Mock<ApplicationDbContext> MockContext;
    protected readonly Mock<ILogger> MockLogger;

    protected BaseRepositoryUnitTest()
    {
        MockContext = new Mock<ApplicationDbContext>(Mock.Of<DbContextOptions<ApplicationDbContext>>());
        MockLogger = new Mock<ILogger>();
    }

    /// <summary>
    /// Creates a mock DbSet with the provided data
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="data">Test data</param>
    /// <returns>Mocked DbSet</returns>
    protected Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

        return mockDbSet;
    }

    /// <summary>
    /// Creates a mock DbSet that supports async operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="data">Test data</param>
    /// <returns>Mocked DbSet with async support</returns>
    protected Mock<DbSet<T>> CreateMockDbSetWithAsync<T>(IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = CreateMockDbSet(data);

        // Setup async enumerable
        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));

        // Setup async queryable
        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));

        return mockDbSet;
    }

    /// <summary>
    /// Sets up FindAsync for a mock DbSet
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <param name="mockDbSet">Mock DbSet</param>
    /// <param name="data">Test data</param>
    /// <param name="keySelector">Key selector function</param>
    protected void SetupFindAsync<T, TKey>(Mock<DbSet<T>> mockDbSet, IEnumerable<T> data, Func<T, TKey> keySelector) where T : class
    {
        mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(ids =>
            {
                var id = (TKey)ids[0];
                var nonNullData = data.OfType<T>(); // Filters out nulls and changes type
                var entity = nonNullData.FirstOrDefault(e => keySelector(e).Equals(id));
                return new ValueTask<T?>(entity);
            });
    }
}


/// <summary>
/// Test async enumerator for mocking async operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;
}

/// <summary>
/// Test async query provider for mocking async LINQ operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }
    public object Execute(Expression expression)
    {
        return _inner.Execute(expression) ?? throw new InvalidOperationException("Query execution returned null");
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }
}

/// <summary>
/// Test async enumerable for mocking async LINQ operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}