namespace QuantFlow.Data.InfluxDB.Builders;

/// <summary>
/// Type-safe Flux query builder for InfluxDB operations
/// </summary>
public class FluxQueryBuilder
{
    private readonly List<string> _operations = new();

    /// <summary>
    /// Starts a query from the specified bucket
    /// </summary>
    public static FluxQueryBuilder From(string bucket)
    {
        var builder = new FluxQueryBuilder();
        builder._operations.Add($"from(bucket: \"{EscapeString(bucket)}\")");
        return builder;
    }

    /// <summary>
    /// Adds a time range filter with start and stop times
    /// </summary>
    public FluxQueryBuilder Range(DateTime start, DateTime stop)
    {
        var startStr = FormatDateTime(start);
        var stopStr = FormatDateTime(stop);
        _operations.Add($"|> range(start: {startStr}, stop: {stopStr})");
        return this;
    }

    /// <summary>
    /// Adds a relative time range filter (e.g., "-24h", "-1d")
    /// </summary>
    public FluxQueryBuilder Range(string duration)
    {
        _operations.Add($"|> range(start: {duration})");
        return this;
    }

    /// <summary>
    /// Adds a measurement filter
    /// </summary>
    public FluxQueryBuilder FilterMeasurement(string measurement)
    {
        _operations.Add($"|> filter(fn: (r) => r._measurement == \"{EscapeString(measurement)}\")");
        return this;
    }

    /// <summary>
    /// Adds a tag filter
    /// </summary>
    public FluxQueryBuilder FilterTag(string tagName, string tagValue)
    {
        _operations.Add($"|> filter(fn: (r) => r.{tagName} == \"{EscapeString(tagValue)}\")");
        return this;
    }

    /// <summary>
    /// Adds a field filter
    /// </summary>
    public FluxQueryBuilder FilterField(string fieldName, object value)
    {
        var valueStr = value switch
        {
            string s => $"\"{EscapeString(s)}\"",
            bool b => b.ToString().ToLowerInvariant(),
            _ => value.ToString()
        };
        _operations.Add($"|> filter(fn: (r) => r._field == {valueStr})");
        return this;
    }

    /// <summary>
    /// Adds multiple tag filters with AND logic
    /// </summary>
    public FluxQueryBuilder FilterTags(Dictionary<string, string> tags)
    {
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t.Value)))
        {
            FilterTag(tag.Key, tag.Value);
        }
        return this;
    }

    /// <summary>
    /// Adds a conditional tag filter (only adds if value is not null/empty)
    /// </summary>
    public FluxQueryBuilder FilterTagIfNotEmpty(string tagName, string? tagValue)
    {
        if (!string.IsNullOrWhiteSpace(tagValue))
        {
            FilterTag(tagName, tagValue);
        }
        return this;
    }

    /// <summary>
    /// Adds pivot operation to transform data
    /// </summary>
    public FluxQueryBuilder Pivot(string[]? rowKeys = null, string[]? columnKeys = null, string? valueColumn = null)
    {
        rowKeys ??= new[] { "_time" };
        columnKeys ??= new[] { "_field" };
        valueColumn ??= "_value";

        var rowKeyStr = string.Join(", ", rowKeys.Select(k => $"\"{k}\""));
        var columnKeyStr = string.Join(", ", columnKeys.Select(k => $"\"{k}\""));

        _operations.Add($"|> pivot(rowKey:[{rowKeyStr}], columnKey: [{columnKeyStr}], valueColumn: \"{valueColumn}\")");
        return this;
    }

    /// <summary>
    /// Adds sort operation
    /// </summary>
    public FluxQueryBuilder Sort(string[] columns, bool descending = false)
    {
        var columnStr = string.Join(", ", columns.Select(c => $"\"{c}\""));
        var descStr = descending ? ", desc: true" : "";
        _operations.Add($"|> sort(columns: [{columnStr}]{descStr})");
        return this;
    }

    /// <summary>
    /// Adds limit operation
    /// </summary>
    public FluxQueryBuilder Limit(int count)
    {
        _operations.Add($"|> limit(n: {count})");
        return this;
    }

    /// <summary>
    /// Adds group operation
    /// </summary>
    public FluxQueryBuilder GroupBy(params string[] columns)
    {
        if (columns.Any())
        {
            var columnStr = string.Join(", ", columns.Select(c => $"\"{c}\""));
            _operations.Add($"|> group(columns: [{columnStr}])");
        }
        return this;
    }

    /// <summary>
    /// Adds aggregation operation
    /// </summary>
    public FluxQueryBuilder Aggregate(string function, string? column = null)
    {
        var columnPart = !string.IsNullOrEmpty(column) ? $"(column: \"{column}\")" : "()";
        _operations.Add($"|> {function}{columnPart}");
        return this;
    }

    /// <summary>
    /// Builds the final Flux query string
    /// </summary>
    public string Build()
    {
        return string.Join(Environment.NewLine, _operations);
    }

    /// <summary>
    /// Formats DateTime for Flux queries
    /// </summary>
    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Escapes strings for safe use in Flux queries
    /// </summary>
    private static string EscapeString(string input)
    {
        return input?.Replace("\"", "\\\"") ?? string.Empty;
    }
}