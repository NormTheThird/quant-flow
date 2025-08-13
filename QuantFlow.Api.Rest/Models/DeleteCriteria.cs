namespace QuantFlow.Api.Rest.Models;

/// <summary>
/// Criteria for deleting specific market data
/// </summary>
public class DeleteCriteria
{
    [Required]
    public string Timeframe { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; } = new();

    [Required]
    public DateTime EndDate { get; set; } = new();
}