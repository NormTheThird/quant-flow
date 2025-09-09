namespace QuantFlow.Common.Models;

/// <summary>
/// Complete result of detecting and populating missing data
/// </summary>
public class CompleteDataEnsureResult
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public DateTime ProcessingStartTime { get; set; }
    public DateTime ProcessingEndTime { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }

    // Detection Results
    public int TotalMissingRangesFound { get; set; }
    public int TotalMissingDataPoints { get; set; }
    public List<MissingDataRange> MissingRanges { get; set; } = new();

    // Population Results
    public DataPopulationResult PopulationResult { get; set; } = new();

    // Final Status
    public bool IsDataComplete { get; set; }
    public double DataCompleteness { get; set; } // 0.0 to 1.0
    public List<string> Summary { get; set; } = new();
}