public class AlgorithmModel : BaseModel
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ProgrammingLanguage { get; set; } = "csharp";
    public string Version { get; set; } = "1.0.0";
    public AlgorithmStatus Status { get; set; } = AlgorithmStatus.Draft;
    public IEnumerable<string> Tags { get; set; } = [];
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    public object RiskSettings { get; set; } = new();
    public object PerformanceMetrics { get; set; } = new();
    public bool IsPublic { get; set; } = false;
    public bool IsTemplate { get; set; } = false;
    public string? TemplateCategory { get; set; } = null;
}

public class AlgorithmPerformanceModel
{
    public Guid AlgorithmId { get; set; } = Guid.Empty;
    public string AlgorithmName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public decimal PnL { get; set; } = 0.0m;
    public decimal CumulativePnL { get; set; } = 0.0m;
    public decimal ReturnPercentage { get; set; } = 0.0m;
    public decimal CumulativeReturn { get; set; } = 0.0m;
    public decimal Drawdown { get; set; } = 0.0m;
    public decimal MaxDrawdown { get; set; } = 0.0m;
    public decimal? SharpeRatio { get; set; } = null;
    public decimal? WinRate { get; set; } = null;
    public decimal? ProfitFactor { get; set; } = null;
    public int TradeCount { get; set; } = 0;
    public int WinningTrades { get; set; } = 0;
    public int LosingTrades { get; set; } = 0;
    public int? AvgTradeDurationMinutes { get; set; } = null;
    public decimal PortfolioValue { get; set; } = 0.0m;
    public decimal PositionSize { get; set; } = 0.0m;
    public decimal? Leverage { get; set; } = null;
    public decimal? Volatility { get; set; } = null;
    public int ExecutionTimeMs { get; set; } = 0;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AlgorithmGlobalsModel
{
    public List<decimal> closePrices { get; set; } = [];
    public PositionModel? currentPosition { get; set; }
    public decimal currentPrice { get; set; }
}

/// <summary>
/// Business model for algorithm metadata
/// </summary>
public class AlgorithmMetadataModel : BaseModel
{
    public required string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public required string Description { get; set; } = string.Empty;
    public AlgorithmType AlgorithmType { get; set; } = AlgorithmType.Unknown;
    public AlgorithmSource AlgorithmSource { get; set; } = AlgorithmSource.Unknown;
    public bool IsEnabled { get; set; } = true;
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Business model for algorithm effectiveness ratings
/// </summary>
public class AlgorithmEffectivenessModel : BaseModel
{
    public Guid AlgorithmId { get; set; } = Guid.Empty;
    public required string Timeframe { get; set; } = string.Empty;
    public int ReliabilityStars { get; set; } = 0;
    public int OpportunityStars { get; set; } = 0;
    public int RecommendedStars { get; set; } = 0;
    public required string ReliabilityReason { get; set; } = string.Empty;
    public required string OpportunityReason { get; set; } = string.Empty;
    public decimal? AverageWinRate { get; set; }
    public decimal? AverageReturnPerTrade { get; set; }
    public int? AverageTradesPerMonth { get; set; }
    public decimal? AverageSharpeRatio { get; set; }
    public decimal? AverageMaxDrawdown { get; set; }
    public decimal? AverageStopLossPercent { get; set; }
    public string? BestFor { get; set; }
    public string? AvoidWhen { get; set; }
    public int TotalBacktestsRun { get; set; } = 0;
}