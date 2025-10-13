namespace QuantFlow.Common.Models;

/// <summary>
/// Represents a trading portfolio in the QuantFlow system
/// </summary>
public class PortfolioModel : BaseModel
{
    public required Guid UserId { get; set; } = Guid.Empty;
    public required string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public required decimal InitialBalance { get; set; } = 0.0m;
    public required decimal CurrentBalance { get; set; } = 0.0m;
    public required Status Status { get; set; } = Status.Unknown;
    public required PortfolioMode Mode { get; set; } = PortfolioMode.Unknown;
    public Exchange Exchange { get; set; } = Exchange.Unknown;
    public BaseCurrency BaseCurrency { get; set; } = BaseCurrency.USDT; 
    public Guid? UserExchangeDetailsId { get; set; } = null;

    // Navigation properties
    public UserModel User { get; set; } = null!;
    public List<BacktestRunModel> BacktestRuns { get; set; } = [];
    public List<AlgorithmPositionModel> AlgorithmPositions { get; set; } = [];
}