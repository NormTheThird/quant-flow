namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for market data operations using business models
/// </summary>
public interface IMarketDataRepository
{
    /// <summary>
    /// Gets summary of available market data grouped by symbol, exchange, and timeframe
    /// </summary>
    /// <returns>Collection of data availability summaries</returns>
    Task<IEnumerable<MarketDataSummary>> GetDataAvailabilitySummaryAsync();

    /// <summary>
    /// Gets price data for a specific exchange and symbol within a time range
    /// </summary>
    /// <param name="exchange">Exchange to retrieve data from (e.g., Exchange.Kraken, Exchange.KuCoin, Exchange.Binance)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe string (e.g., "OneMinute", "OneHour", "OneDay")</param>
    /// <param name="start">Start date and time for data retrieval</param>
    /// <param name="end">End date and time for data retrieval</param>
    /// <returns>Collection of market data models ordered by timestamp</returns>
    Task<IEnumerable<MarketDataModel>> GetPriceDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime start, DateTime end);

    /// <summary>
    /// Gets the latest market data for a specific exchange and symbol
    /// </summary>
    /// <param name="exchange">Exchange to retrieve data from (e.g., Exchange.Kraken, Exchange.KuCoin, Exchange.Binance)</param>
    /// <param name="symbol">Trading symbol (e.g., "BTCUSDT", "ETHUSDT")</param>
    /// <param name="timeframe">Timeframe string (e.g., "OneMinute", "OneHour", "OneDay")</param>
    /// <returns>Latest market data model or null if not found</returns>
    Task<MarketDataModel?> GetLatestPriceAsync(Exchange exchange, string symbol, Timeframe timeframe);

    /// <summary>
    /// Writes market data to the time-series database
    /// </summary>
    /// <param name="marketData">Market data to store</param>
    Task WritePriceDataAsync(MarketDataModel marketData);

    /// <summary>
    /// Writes multiple market data points in a batch operation
    /// </summary>
    /// <param name="marketDataList">Collection of market data to store</param>
    Task WritePriceDataBatchAsync(IEnumerable<MarketDataModel> marketDataList);

    /// <summary>
    /// Deletes market data for a specific exchange and symbol within a time range
    /// </summary>
    /// <param name="exchange">Exchange to delete data from</param>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="timeframe">Timeframe interval</param>
    /// <param name="startDate">Start date for deletion</param>
    /// <param name="endDate">End date for deletion</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteMarketDataAsync(Exchange exchange, string symbol, Timeframe timeframe, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Deletes all market data for a specific exchange and symbol (use with caution)
    /// </summary>
    /// <param name="exchange">Exchange to delete data from</param>
    /// <param name="symbol">Trading symbol</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteAllMarketDataAsync(Exchange exchange, string symbol);
}