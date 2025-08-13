namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for market data operations and validation
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Gets market data for a symbol within a specified time range
    /// </summary>
    /// <param name="exchange">Exchange to retrieve data from (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <param name="limit">Optional limit on number of records</param>
    /// <returns>Collection of market data models ordered by timestamp</returns>
    Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate, int? limit = null);

    /// <summary>
    /// Gets the latest market data point for a symbol
    /// </summary>
    /// <param name="exchange">Exchange to retrieve data from (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <returns>Latest market data model or null if not found</returns>
    Task<MarketDataModel?> GetLatestMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe);

    /// <summary>
    /// Validates market data quality and detects gaps
    /// </summary>
    /// <param name="marketData">Collection of market data to validate</param>
    /// <param name="expectedTimeframe">Expected timeframe interval</param>
    /// <returns>Data quality report with validation results</returns>
    Task<MarketDataQualityReport> ValidateDataQualityAsync(IEnumerable<MarketDataModel> marketData, Timeframe expectedTimeframe);

    /// <summary>
    /// Gets data gaps for a given time range
    /// </summary>
    /// <param name="exchange">Exchange to check for gaps (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <param name="startDate">Start date for gap detection</param>
    /// <param name="endDate">End date for gap detection</param>
    /// <returns>Collection of detected data gaps</returns>
    Task<IEnumerable<DataGap>> GetDataGapsAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets data availability summary for a symbol
    /// </summary>
    /// <param name="exchange">Exchange to check availability for (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <returns>Data availability information</returns>
    Task<DataAvailabilityInfo> GetDataAvailabilityAsync(Exchange exchange, string symbol);

    /// <summary>
    /// Stores market data points to the database
    /// </summary>
    /// <param name="marketData">Market data to store</param>
    /// <returns>Number of records successfully stored</returns>
    Task<int> StoreMarketDataAsync(IEnumerable<MarketDataModel> marketData);

    /// <summary>
    /// Normalizes market data timestamps to ensure consistent intervals
    /// </summary>
    /// <param name="marketData">Market data to normalize</param>
    /// <param name="timeframe">Target timeframe for normalization</param>
    /// <returns>Normalized market data collection</returns>
    Task<IEnumerable<MarketDataModel>> NormalizeTimestampsAsync(IEnumerable<MarketDataModel> marketData, Timeframe timeframe);

    /// <summary>
    /// Deletes market data for testing purposes
    /// </summary>
    /// <param name="exchange">Exchange to delete data from (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <param name="startDate">Start date for deletion</param>
    /// <param name="endDate">End date for deletion</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Deletes all market data for a symbol (use with extreme caution - for testing only)
    /// </summary>
    /// <param name="exchange">Exchange to delete data from (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteAllMarketDataAsync(Exchange exchange, string symbol);
}