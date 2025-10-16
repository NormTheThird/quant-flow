namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for executing trading algorithms in C# or Python
/// </summary>
public class AlgorithmExecutionService : IAlgorithmExecutionService
{
    private readonly ILogger<AlgorithmExecutionService> _logger;

    public AlgorithmExecutionService(ILogger<AlgorithmExecutionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TradeSignal> ExecuteAsync(AlgorithmModel algorithm, List<MarketDataModel> historicalData, MarketDataModel currentBar, PositionModel? currentPosition)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(historicalData);
        ArgumentNullException.ThrowIfNull(currentBar);

        try
        {
            _logger.LogDebug("Executing algorithm {AlgorithmName} ({Language})", algorithm.Name, algorithm.ProgrammingLanguage);

            return algorithm.ProgrammingLanguage.ToLowerInvariant() switch
            {
                "csharp" => await ExecuteCSharpAlgorithmAsync(algorithm, historicalData, currentBar, currentPosition),
                "python" => await ExecutePythonAlgorithmAsync(algorithm, historicalData, currentBar, currentPosition),
                _ => throw new NotSupportedException($"Programming language '{algorithm.ProgrammingLanguage}' is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing algorithm {AlgorithmName}", algorithm.Name);
            throw;
        }
    }

    private async Task<TradeSignal> ExecuteCSharpAlgorithmAsync(AlgorithmModel algorithm, List<MarketDataModel> historicalData, MarketDataModel currentBar, PositionModel? currentPosition)
    {
        try
        {
            _logger.LogDebug("Compiling and executing C# algorithm: {AlgorithmName}", algorithm.Name);

            // Prepare the three standard parameters that every algorithm receives
            var closePrices = historicalData.Select(_ => _.Close).ToList();
            var currentPrice = currentBar.Close;

            // Get algorithm-specific parameters
            var shortPeriod = GetParameter<int>(algorithm.Parameters, "shortPeriod", 9);
            var longPeriod = GetParameter<int>(algorithm.Parameters, "longPeriod", 21);

            // Build the full C# script with necessary usings and the algorithm code
            var scriptCode = $@"using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using QuantFlow.Common.Enumerations;
                                using QuantFlow.Common.Models;

                                {algorithm.Code}

                                // Execute the algorithm method with its specific parameters
                                CheckMovingAverageCrossover({shortPeriod}, {longPeriod}, closePrices, currentPosition, currentPrice)
                                ";

            // Create globals object to pass the three standard variables to script
            var globals = new AlgorithmGlobalsModel
            {
                closePrices = closePrices,
                currentPosition = currentPosition,
                currentPrice = currentPrice
            };

            // Compile and execute
            var options = ScriptOptions.Default
                .AddReferences(typeof(TradeSignal).Assembly, typeof(PositionModel).Assembly)
                .AddImports("System", "System.Collections.Generic", "System.Linq", "QuantFlow.Common.Enumerations", "QuantFlow.Common.Models");

            var result = await CSharpScript.EvaluateAsync<TradeSignal>(scriptCode, options, globals);

            _logger.LogDebug("Algorithm {AlgorithmName} returned signal: {Signal}", algorithm.Name, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing C# algorithm {AlgorithmName}", algorithm.Name);
            throw new InvalidOperationException($"Failed to execute algorithm: {ex.Message}", ex);
        }
    }

    private async Task<TradeSignal> ExecutePythonAlgorithmAsync(AlgorithmModel algorithm, List<MarketDataModel> historicalData, MarketDataModel currentBar, PositionModel? currentPosition)
    {
        await Task.Delay(0); // Placeholder for async method
        throw new NotImplementedException("Python algorithm execution is not implemented yet");
    }

    private T GetParameter<T>(IDictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.Deserialize<T>() ?? defaultValue;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}