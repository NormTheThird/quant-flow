namespace QuantFlow.Common.Models;

public class BaseUserModel : BaseModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsSystemAdmin { get; set; } = false;
}

public class UserModel : BaseUserModel
{
    public bool IsEmailVerified { get; set; } = false;

    //// Navigation properties
    //public List<PortfolioModel> Portfolios { get; set; } = [];
    //public List<SubscriptionModel> Subscriptions { get; set; } = [];
    //public List<BacktestRunModel> BacktestRuns { get; set; } = [];
}