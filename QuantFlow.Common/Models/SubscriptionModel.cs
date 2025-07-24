namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a user subscription in the QuantFlow system
/// </summary>
public class SubscriptionModel : BaseModel
{
    public required Guid UserId { get; set; } = Guid.Empty;
    public required SubscriptionType Type { get; set; } = SubscriptionType.Free;
    public DateTime StartDate { get; set; } = new();
    public DateTime EndDate { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public int MaxPortfolios { get; set; } = 1;
    public int MaxAlgorithms { get; set; } = 5;
    public int MaxBacktestRuns { get; set; } = 10;

    // Navigation property
    public UserModel User { get; set; } = null!;
}