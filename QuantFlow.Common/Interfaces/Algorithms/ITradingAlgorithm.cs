namespace QuantFlow.Common.Interfaces.Algorithms;

/// <summary>
/// Core interface that all trading algorithms (hard-coded and custom) must implement
/// </summary>
public interface ITradingAlgorithm
{
    /// <summary>
    /// Unique identifier for this algorithm
    /// </summary>
    Guid AlgorithmId { get; }

    /// <summary>
    /// Display name of the algorithm
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Detailed description of the algorithm's strategy
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Type/category of the algorithm
    /// </summary>
    AlgorithmType Type { get; }

    /// <summary>
    /// Source/origin of the algorithm
    /// </summary>
    AlgorithmSource Source { get; }

    /// <summary>
    /// Analyzes market data and returns a trading signal
    /// </summary>
    /// <param name="data">Historical market data including current bar</param>
    /// <param name="currentPosition">Current open position, if any</param>
    /// <param name="parameters">Algorithm-specific parameters</param>
    /// <returns>Trade signal with action and risk management levels</returns>
    TradeSignalModel Analyze(
        MarketDataModel[] data,
        PositionModel? currentPosition,
        AlgorithmParameters parameters);

    /// <summary>
    /// Gets the default parameters for this algorithm
    /// </summary>
    /// <returns>Default parameter configuration</returns>
    AlgorithmParameters GetDefaultParameters();

    /// <summary>
    /// Gets parameter definitions for UI generation
    /// </summary>
    /// <returns>List of parameter definitions</returns>
    List<ParameterDefinition> GetParameterDefinitions();

    /// <summary>
    /// Validates that the provided parameters are acceptable
    /// </summary>
    /// <param name="parameters">Parameters to validate</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateParameters(AlgorithmParameters parameters, out string errorMessage);
}