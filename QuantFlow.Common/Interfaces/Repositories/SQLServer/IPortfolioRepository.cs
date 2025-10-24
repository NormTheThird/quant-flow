namespace QuantFlow.Common.Interfaces.Repositories.SQLServer;

/// <summary>
/// Repository interface for portfolio operations using business models
/// </summary>
public interface IPortfolioRepository
{
    /// <summary>
    /// Gets a portfolio by its unique identifier
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>Portfolio business model if found, null otherwise</returns>
    Task<PortfolioModel?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all portfolios for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Collection of portfolio business models</returns>
    Task<IEnumerable<PortfolioModel>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all portfolios
    /// </summary>
    /// <returns>Collection of portfolio business models</returns>
    Task<IEnumerable<PortfolioModel>> GetAllAsync();

    /// <summary>
    /// Creates a new portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio business model to create</param>
    /// <returns>Created portfolio business model</returns>
    Task<PortfolioModel> CreateAsync(PortfolioModel portfolio);

    /// <summary>
    /// Updates an existing portfolio
    /// </summary>
    /// <param name="portfolio">Portfolio business model with updates</param>
    /// <returns>Updated portfolio business model</returns>
    Task<PortfolioModel> UpdateAsync(PortfolioModel portfolio);

    /// <summary>
    /// Soft deletes a portfolio
    /// </summary>
    /// <param name="id">The portfolio's unique identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Counts the number of active portfolios for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>Number of active portfolios</returns>
    Task<int> CountByUserIdAsync(Guid userId);
}