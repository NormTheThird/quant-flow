namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a trading portfolio in the QuantFlow system
/// </summary>
public class PortfolioModel : BaseModel
{
    public required string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public required decimal InitialBalance { get; set; } = 10000.0m;
    public required decimal CurrentBalance { get; set; } = 10000.0m;
    public required PortfolioStatus Status { get; set; } = PortfolioStatus.Active;
    public required Guid UserId { get; set; } = Guid.Empty;
    public decimal MaxPositionSizePercent { get; set; } = 10.0m;
    public decimal CommissionRate { get; set; } = 0.001m;
    public bool AllowShortSelling { get; set; } = false;

    // Navigation properties
    public UserModel User { get; set; } = null!;
    public List<BacktestRunModel> BacktestRuns { get; set; } = [];
}