namespace QuantFlow.Common.Models;

/// <summary>
/// Result of attempting to populate missing data from an exchange API
/// </summary>
public class DataPopulationResult
{
    public int TotalRangesProcessed { get; set; }
    public int SuccessfulRanges { get; set; }
    public int FailedRanges { get; set; }
    public int NewDataPointsAdded { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessingStartTime { get; set; }
    public DateTime ProcessingEndTime { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
    public List<MissingDataRange> RemainingGaps { get; set; } = new();
}