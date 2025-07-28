namespace QuantFlow.Common.Enumerations;

/// <summary>
/// How stop loss functionality is integrated with algorithm execution
/// </summary>
public enum StopLossExecutionMode
{
    /// <summary>
    /// Stop losses managed entirely by C# system, transparent to algorithms
    /// </summary>
    SystemLevel = 1,

    /// <summary>
    /// Algorithms are aware of and can see stop loss executions
    /// </summary>
    AlgorithmAware = 2,

    /// <summary>
    /// Combination of system protection with algorithm awareness
    /// </summary>
    Hybrid = 3
}