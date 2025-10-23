namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Type of algorithm parameter
/// </summary>
public enum ParameterType
{
    /// <summary>
    /// Unknown or unspecified type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Integer value
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Decimal/floating point value
    /// </summary>
    Decimal = 2,

    /// <summary>
    /// Boolean (true/false)
    /// </summary>
    Boolean = 3,

    /// <summary>
    /// String value
    /// </summary>
    String = 4,

    /// <summary>
    /// Enum selection
    /// </summary>
    Enum = 5
}