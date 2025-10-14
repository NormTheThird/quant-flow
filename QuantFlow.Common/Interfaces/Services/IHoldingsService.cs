namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for managing user holdings across exchanges
/// </summary>
public interface IHoldingsService
{
    /// <summary>
    /// Gets Kraken holdings with USD values for the current user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of holdings with USD values</returns>
    Task<HoldingsSummary> GetKrakenHoldingsAsync(Guid userId);
}