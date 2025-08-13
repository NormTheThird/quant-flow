namespace QuantFlow.Data.InfluxDB.Extensions;

/// <summary>
/// Extension methods for InfluxDbContext to use the query builder
/// </summary>
public static class InfluxDbContextExtensions
{
    /// <summary>
    /// Creates a new query builder for this context
    /// </summary>
    public static FluxQueryBuilder NewQuery(this InfluxDbContext context, string? bucket = null)
    {
        return FluxQueryBuilder.From(bucket ?? context.Bucket);
    }

    /// <summary>
    /// Executes a query builder and returns typed results
    /// </summary>
    public static async Task<IEnumerable<T>> ExecuteAsync<T>(this FluxQueryBuilder builder, InfluxDbContext context) where T : class
    {
        var query = builder.Build();
        return await context.QueryAsync<T>(query);
    }
}