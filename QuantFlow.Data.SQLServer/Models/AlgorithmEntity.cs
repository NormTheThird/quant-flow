namespace QuantFlow.Data.SQLServer.Models;

/// <summary>
/// SQL Server entity for algorithm metadata
/// </summary>
[Table("Algorithms")]
public class AlgorithmEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Abbreviation { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int AlgorithmType { get; set; } = 0;

    [Required]
    public int AlgorithmSource { get; set; } = 0;

    [Required]
    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0";

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new();

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = new();

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}