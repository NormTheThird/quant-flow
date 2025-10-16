namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for market data operations and gap detection
/// This service focuses on data retrieval and gap analysis - gap population should be handled by exchange-specific services
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
    /// <param name="limit">Optional limit on number of records to return</param>
    /// <returns>Collection of market data models ordered by timestamp</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null/empty or start date is after end date</exception>
    Task<IEnumerable<MarketDataModel>> GetMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe,        DateTime startDate, DateTime endDate, int? limit = null);

    /// <summary>
    /// Gets the most recent market data point for a symbol and timeframe
    /// </summary>
    /// <param name="exchange">Exchange to check (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <returns>Most recent market data point, or null if no data exists</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null or empty</exception>
    Task<MarketDataModel?> GetLatestMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe);

    /// <summary>
    /// Detects gaps in market data for a given time range (legacy method)
    /// This method finds gaps between existing data points
    /// </summary>
    /// <param name="exchange">Exchange to check for gaps (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <param name="startDate">Start date for gap detection</param>
    /// <param name="endDate">End date for gap detection</param>
    /// <returns>Collection of detected data gaps</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null/empty or start date is after end date</exception>
    Task<IEnumerable<DataGap>> GetDataGapsAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets ALL missing intervals for a symbol and timeframe from start date to end date
    /// This method generates expected timestamps and identifies complete missing ranges,
    /// regardless of whether any data exists (enhanced gap detection)
    /// NOTE: This method only DETECTS gaps, it does NOT populate them
    /// </summary>
    /// <param name="exchange">Exchange to check (e.g., Exchange.Kraken, Exchange.KuCoin)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe interval (e.g., Timeframe.OneMinute, Timeframe.OneHour, Timeframe.OneDay)</param>
    /// <param name="startDate">Start date to check from</param>
    /// <param name="endDate">End date to check to (defaults to now if null)</param>
    /// <returns>Collection of missing interval ranges that need to be populated by exchange services</returns>
    /// <exception cref="ArgumentException">Thrown when symbol is null/empty or start date is after end date</exception>
    Task<IEnumerable<MissingDataRange>> GetMissingIntervalsAsync(Exchange exchange, string symbol, Timeframe timeframe,        DateTime startDate, DateTime? endDate = null);

    /// <summary>
    /// Gets a summary of available market data grouped by symbol, exchange, and timeframe
    /// </summary>
    /// <returns>Collection of market data availability summaries showing date ranges and record counts</returns>
    Task<IEnumerable<MarketDataSummary>> GetDataAvailabilitySummaryAsync();

    /// <summary>
    /// Stores market data points to the database
    /// </summary>
    /// <param name="marketData">Market data to store</param>
    /// <returns>Number of records successfully stored</returns>
    /// <exception cref="ArgumentNullException">Thrown when marketData is null</exception>
    Task<int> StoreMarketDataAsync(IEnumerable<MarketDataModel> marketData);
}