namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service interface for portfolio operations
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Gets a portfolio by its unique identifier
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>Portfolio model if found, null otherwise</returns>
    Task<PortfolioModel?> GetPortfolioByIdAsync(Guid id);

    /// <summary>
    /// Gets all portfolios for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of portfolio models</returns>
    Task<IEnumerable<PortfolioModel>> GetPortfoliosByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio model to create</param>
    /// <returns>Created portfolio model</returns>
    Task<PortfolioModel> CreatePortfolioAsync(PortfolioModel portfolio);

    /// <summary>
    /// Updates an existing portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio model with updates</param>
    /// <returns>Updated portfolio model</returns>
    Task<PortfolioModel> UpdatePortfolioAsync(PortfolioModel portfolio);

    /// <summary>
    /// Deletes a portfolio
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeletePortfolioAsync(Guid id);
}