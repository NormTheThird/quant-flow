namespace QuantFlow.Common.Models;

public class PositionModel
{
    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public DateTime EntryTime { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedPnL { get; set; }
}

/// <summary>
/// Represents an algorithm position within a portfolio.
/// Each position allocates a percentage of the portfolio to a specific trading algorithm.
/// </summary>
public class AlgorithmPositionModel : BaseModel
{
    public required Guid AlgorithmId { get; set; } = Guid.Empty;
    public required Guid UserId { get; set; } = Guid.Empty;
    public Guid? PortfolioId { get; set; } = null;
    public required string PositionName { get; set; } = string.Empty;
    public required string Symbol { get; set; } = string.Empty;
    public required decimal AllocatedPercent { get; set; } = 0.0m;
    public Status Status { get; set; } = Status.Unknown;

    // Risk Management Settings (moved from Portfolio)
    public decimal MaxPositionSizePercent { get; set; } = 10.0m;
    public decimal ExchangeFees { get; set; } = 0.001m;
    public bool AllowShortSelling { get; set; } = false;
    public string AlgorithmParameters { get; set; } = "{}";
    // Performance Tracking
    public decimal CurrentValue { get; set; } = 0.0m;
    public DateTime? ActivatedAt { get; set; } = null;

    // Navigation properties
    public PortfolioModel Portfolio { get; set; } = null!;
    public AlgorithmModel Algorithm { get; set; } = null!;
    public List<TradeModel> Trades { get; set; } = [];
}

/// <summary>
/// Display model for position with algorithm name
/// </summary>
public class PositionDisplayModel
{
    public AlgorithmPositionModel Position { get; set; } = null!;
    public string AlgorithmName { get; set; } = string.Empty;
    public string PortfolioName { get; set; } = string.Empty;
}