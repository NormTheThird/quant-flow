namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service for executing trading algorithms
/// </summary>
public interface IAlgorithmExecutionService
{
    /// <summary>
    /// Executes an algorithm and returns a trade signal
    /// </summary>
    /// <param name="algorithm">Algorithm to execute</param>
    /// <param name="historicalData">Historical market data for analysis</param>
    /// <param name="currentBar">Current market data point</param>
    /// <param name="currentPosition">Current trading position, null if no position</param>
    /// <returns>Trade signal (Buy, Sell, or Hold)</returns>
    Task<TradeSignal> ExecuteAsync(AlgorithmModel algorithm, List<MarketDataModel> historicalData, MarketDataModel currentBar, PositionModel? currentPosition);
}