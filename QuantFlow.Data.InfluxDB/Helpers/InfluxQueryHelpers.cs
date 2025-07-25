namespace QuantFlow.Data.InfluxDB.Helpers;

/// <summary>
/// Helper methods for building InfluxDB Flux queries
/// </summary>
public static class InfluxQueryHelpers
{
    /// <summary>
    /// Formats a DateTime for use in Flux queries
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Formatted datetime string for Flux</returns>
    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Builds a basic range filter for Flux queries
    /// </summary>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <returns>Flux range filter</returns>
    public static string BuildRangeFilter(DateTime start, DateTime end)
    {
        return $"range(start: {FormatDateTime(start)}, stop: {FormatDateTime(end)})";
    }

    /// <summary>
    /// Builds a measurement filter for Flux queries
    /// </summary>
    /// <param name="measurement">Measurement name</param>
    /// <returns>Flux measurement filter</returns>
    public static string BuildMeasurementFilter(string measurement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurement);
        return $"filter(fn: (r) => r._measurement == \"{measurement}\")";
    }

    /// <summary>
    /// Builds a tag filter for Flux queries
    /// </summary>
    /// <param name="tagName">Tag name</param>
    /// <param name="tagValue">Tag value</param>
    /// <returns>Flux tag filter</returns>
    public static string BuildTagFilter(string tagName, string tagValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tagValue);
        return $"filter(fn: (r) => r.{tagName} == \"{tagValue}\")";
    }

    /// <summary>
    /// Builds multiple tag filters with AND logic
    /// </summary>
    /// <param name="tagFilters">Dictionary of tag name/value pairs</param>
    /// <returns>Flux tag filters</returns>
    public static string BuildTagFilters(Dictionary<string, string> tagFilters)
    {
        ArgumentNullException.ThrowIfNull(tagFilters);

        if (!tagFilters.Any())
            return string.Empty;

        var filters = tagFilters
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"r.{kv.Key} == \"{kv.Value}\"");

        var filterExpression = string.Join(" and ", filters);
        return $"filter(fn: (r) => {filterExpression})";
    }

    /// <summary>
    /// Builds a field filter for Flux queries
    /// </summary>
    /// <param name="fieldNames">Field names to include</param>
    /// <returns>Flux field filter</returns>
    public static string BuildFieldFilter(params string[] fieldNames)
    {
        ArgumentNullException.ThrowIfNull(fieldNames);

        if (!fieldNames.Any())
            return string.Empty;

        var fieldConditions = fieldNames
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => $"r._field == \"{f}\"");

        var filterExpression = string.Join(" or ", fieldConditions);
        return $"filter(fn: (r) => {filterExpression})";
    }

    /// <summary>
    /// Builds an aggregation window for Flux queries
    /// </summary>
    /// <param name="window">Window duration (e.g., "1h", "5m", "1d")</param>
    /// <param name="aggregationFunction">Aggregation function (e.g., "mean", "max", "min", "sum")</param>
    /// <returns>Flux aggregation window</returns>
    public static string BuildAggregationWindow(string window, string aggregationFunction = "mean")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregationFunction);

        return $"aggregateWindow(every: {window}, fn: {aggregationFunction})";
    }

    /// <summary>
    /// Builds a complete basic query for time-series data
    /// </summary>
    /// <param name="bucket">Bucket name</param>
    /// <param name="measurement">Measurement name</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="tagFilters">Optional tag filters</param>
    /// <param name="fieldNames">Optional field names to include</param>
    /// <returns>Complete Flux query</returns>
    public static string BuildBasicQuery(
        string bucket,
        string measurement,
        DateTime start,
        DateTime end,
        Dictionary<string, string>? tagFilters = null,
        string[]? fieldNames = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(measurement);

        var query = new StringBuilder();

        // Base query
        query.AppendLine($"from(bucket: \"{bucket}\")");
        query.AppendLine($"|> {BuildRangeFilter(start, end)}");
        query.AppendLine($"|> {BuildMeasurementFilter(measurement)}");

        // Add tag filters if provided
        if (tagFilters?.Any() == true)
        {
            query.AppendLine($"|> {BuildTagFilters(tagFilters)}");
        }

        // Add field filters if provided
        if (fieldNames?.Any() == true)
        {
            query.AppendLine($"|> {BuildFieldFilter(fieldNames)}");
        }

        // Pivot and sort
        query.AppendLine("|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")");
        query.AppendLine("|> sort(columns: [\"_time\"])");

        return query.ToString();
    }

    /// <summary>
    /// Builds a query for getting latest data
    /// </summary>
    /// <param name="bucket">Bucket name</param>
    /// <param name="measurement">Measurement name</param>
    /// <param name="lookbackPeriod">Lookback period (e.g., "1h", "1d")</param>
    /// <param name="tagFilters">Optional tag filters</param>
    /// <param name="limit">Number of records to return (default: 1)</param>
    /// <returns>Flux query for latest data</returns>
    public static string BuildLatestDataQuery(
        string bucket,
        string measurement,
        string lookbackPeriod = "1h",
        Dictionary<string, string>? tagFilters = null,
        int limit = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(measurement);
        ArgumentException.ThrowIfNullOrWhiteSpace(lookbackPeriod);

        var query = new StringBuilder();

        // Base query with lookback
        query.AppendLine($"from(bucket: \"{bucket}\")");
        query.AppendLine($"|> range(start: -{lookbackPeriod})");
        query.AppendLine($"|> {BuildMeasurementFilter(measurement)}");

        // Add tag filters if provided
        if (tagFilters?.Any() == true)
        {
            query.AppendLine($"|> {BuildTagFilters(tagFilters)}");
        }

        // Pivot, sort, and limit
        query.AppendLine("|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")");
        query.AppendLine("|> sort(columns: [\"_time\"], desc: true)");
        query.AppendLine($"|> limit(n: {limit})");

        return query.ToString();
    }

    /// <summary>
    /// Builds a query for aggregated statistics
    /// </summary>
    /// <param name="bucket">Bucket name</param>
    /// <param name="measurement">Measurement name</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="fieldName">Field to aggregate</param>
    /// <param name="aggregationFunction">Aggregation function</param>
    /// <param name="groupByTags">Tags to group by</param>
    /// <param name="window">Aggregation window (optional)</param>
    /// <returns>Flux aggregation query</returns>
    public static string BuildAggregationQuery(
        string bucket,
        string measurement,
        DateTime start,
        DateTime end,
        string fieldName,
        string aggregationFunction = "mean",
        string[]? groupByTags = null,
        string? window = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(measurement);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregationFunction);

        var query = new StringBuilder();

        // Base query
        query.AppendLine($"from(bucket: \"{bucket}\")");
        query.AppendLine($"|> {BuildRangeFilter(start, end)}");
        query.AppendLine($"|> {BuildMeasurementFilter(measurement)}");
        query.AppendLine($"|> {BuildFieldFilter(fieldName)}");

        // Add grouping if specified
        if (groupByTags?.Any() == true)
        {
            var groupColumns = string.Join("\", \"", groupByTags);
            query.AppendLine($"|> group(columns: [\"{groupColumns}\"])");
        }

        // Add windowing if specified
        if (!string.IsNullOrWhiteSpace(window))
        {
            query.AppendLine($"|> {BuildAggregationWindow(window, aggregationFunction)}");
        }
        else
        {
            query.AppendLine($"|> {aggregationFunction}()");
        }

        query.AppendLine("|> yield(name: \"aggregated\")");

        return query.ToString();
    }

    /// <summary>
    /// Escapes special characters in Flux strings
    /// </summary>
    /// <param name="value">Value to escape</param>
    /// <returns>Escaped string</returns>
    public static string EscapeFluxString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}