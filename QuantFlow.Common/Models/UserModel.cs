namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a user in the QuantFlow system
/// </summary>
public class UserModel : BaseModel
{
    public required string Username { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsSystemAdmin { get; set; } = false;

    // Navigation properties
    public List<PortfolioModel> Portfolios { get; set; } = [];
    public List<SubscriptionModel> Subscriptions { get; set; } = [];
    public List<BacktestRunModel> BacktestRuns { get; set; } = [];
}