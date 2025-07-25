namespace QuantFlow.Data.InfluxDB.Configurations;

/// <summary>
/// InfluxDB configuration settings
/// </summary>
public class InfluxDbConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "InfluxDb";

    /// <summary>
    /// InfluxDB server URL
    /// </summary>
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// InfluxDB authentication token
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// InfluxDB bucket name
    /// </summary>
    [Required]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// InfluxDB organization name
    /// </summary>
    [Required]
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Connection timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable debug logging for InfluxDB operations
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Batch size for bulk write operations (default: 1000)
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Flush interval in milliseconds for write operations (default: 1000)
    /// </summary>
    public int FlushIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Maximum number of retries for failed operations (default: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>Validation results</returns>
    public IEnumerable<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(this);

        Validator.TryValidateObject(this, context, results, true);

        return results;
    }

    /// <summary>
    /// Gets a formatted connection string for logging (without sensitive data)
    /// </summary>
    /// <returns>Safe connection string</returns>
    public string GetSafeConnectionString()
    {
        return $"URL: {Url}, Bucket: {Bucket}, Organization: {Organization}";
    }
}