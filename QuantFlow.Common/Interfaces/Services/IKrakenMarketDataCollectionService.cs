namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for Kraken-specific market data collection operations
/// </summary>
public interface IKrakenMarketDataCollectionService
{
    /// <summary>
    /// Gets market data for DataCollectionOrchestrator - string timeframe version
    /// </summary>
    Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(string symbol, string timeframe, DateTime startTime, DateTime endTime);

    /// <summary>
    /// Gets market data for DataCollectionOrchestrator - enum timeframe version
    /// </summary>
    Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(string symbol, Timeframe timeframe, DateTime startTime, DateTime endTime);

    /// <summary>
    /// Collects data for a specific symbol and timeframe
    /// </summary>
    Task CollectDataAsync(string symbol, Timeframe timeframe, int intervalsBack = 100);

    /// <summary>
    /// Collects data for multiple symbols and timeframes
    /// </summary>
    Task CollectDataAsync(List<SymbolConfig> symbolConfigs);

    /// <summary>
    /// Collects recent data to fill gaps
    /// </summary>
    Task CollectRecentDataAsync(string symbol, Timeframe timeframe);

    /// <summary>
    /// Populates missing data ranges specifically from Kraken API
    /// </summary>
    Task<DataPopulationResult> PopulateMissingDataAsync(string symbol, Timeframe timeframe, IEnumerable<MissingDataRange> missingRanges);
}