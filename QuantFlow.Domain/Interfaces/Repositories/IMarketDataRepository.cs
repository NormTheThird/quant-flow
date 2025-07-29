namespace QuantFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for market data operations using business models
/// </summary>
public interface IMarketDataRepository
{
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
    /// Gets market data for a symbol within a time range
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="timeframe">Timeframe string (e.g., "1m", "5m", "1h")</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <returns>Collection of market data models</returns>
    Task<IEnumerable<MarketDataModel>> GetPriceDataAsync(
        string symbol,
        string timeframe,
        DateTime start,
        DateTime end,
        string? exchange = null);

    /// <summary>
    /// Gets the latest market data for a symbol
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <returns>Latest market data model or null</returns>
    Task<MarketDataModel?> GetLatestPriceAsync(string symbol, string? exchange = null);

    /// <summary>
    /// Writes trade execution data to the time-series database
    /// </summary>
    /// <param name="trade">Trade data to store</param>
    Task WriteTradeDataAsync(TradeModel trade);

    /// <summary>
    /// Gets trade execution data for a symbol within a time range
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="exchange">Optional exchange filter</param>
    /// <returns>Collection of trade models</returns>
    Task<IEnumerable<TradeModel>> GetTradeDataAsync(
        string symbol,
        DateTime start,
        DateTime end,
        string? exchange = null);

    /// <summary>
    /// Deletes market data for a symbol within a time range
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="timeframe">Timeframe interval</param>
    /// <param name="startDate">Start date for deletion</param>
    /// <param name="endDate">End date for deletion</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteMarketDataAsync(string symbol, string timeframe, DateTime startDate, DateTime endDate, string? dataSource = null);


    /// <summary>
    /// Deletes all market data for a symbol (use with caution)
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <param name="dataSource">Optional data source filter</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteAllMarketDataAsync(string symbol, string? dataSource = null);
}