namespace QuantFlow.Common.Models;

/// <summary>
/// Defines a parameter for algorithm configuration
/// </summary>
public class ParameterDefinition
{
    /// <summary>
    /// Internal parameter name (e.g., "FastPeriod")
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Display name for UI (e.g., "Fast Period")
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Type of parameter
    /// </summary>
    public ParameterType Type { get; set; }

    /// <summary>
    /// Default value for this parameter
    /// </summary>
    public required object DefaultValue { get; set; }

    /// <summary>
    /// Minimum allowed value (for numeric types)
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Maximum allowed value (for numeric types)
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Help text describing the parameter
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Order for display in UI
    /// </summary>
    public int DisplayOrder { get; set; }
}