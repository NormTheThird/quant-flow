namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity for algorithm effectiveness ratings by timeframe
/// </summary>
[Table("AlgorithmEffectiveness")]
public class AlgorithmEffectivenessEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    public Guid AlgorithmId { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(10)]
    public string Timeframe { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int ReliabilityStars { get; set; } = 0;

    [Required]
    [Range(1, 5)]
    public int OpportunityStars { get; set; } = 0;

    [Required]
    [Range(1, 5)]
    public int RecommendedStars { get; set; } = 0;

    [Required]
    [MaxLength(500)]
    public string ReliabilityReason { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string OpportunityReason { get; set; } = string.Empty;

    [Precision(5, 4)]
    public decimal? AverageWinRate { get; set; }

    [Precision(6, 4)]
    public decimal? AverageReturnPerTrade { get; set; }

    public int? AverageTradesPerMonth { get; set; }

    [Precision(5, 2)]
    public decimal? AverageSharpeRatio { get; set; }

    [Precision(5, 4)]
    public decimal? AverageMaxDrawdown { get; set; }

    [Precision(5, 4)]
    public decimal? AverageStopLossPercent { get; set; }

    public string? BestFor { get; set; }

    public string? AvoidWhen { get; set; }

    public int TotalBacktestsRun { get; set; } = 0;

    [ForeignKey(nameof(AlgorithmId))]
    public virtual AlgorithmEntity Algorithm { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();
    
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime UpdatedAt { get; set; } = new();
    
    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}