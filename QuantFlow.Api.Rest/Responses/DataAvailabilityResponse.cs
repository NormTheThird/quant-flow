namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Data availability response model
/// </summary>
public class DataAvailabilityResponse
{
    public string Symbol { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public DateTime? EarliestData { get; set; } = null;
    public DateTime? LatestData { get; set; } = null;
    public int TotalRecords { get; set; } = 0;
    public List<string> AvailableTimeframes { get; set; } = [];
    public List<GapInfoResponse> DataGaps { get; set; } = [];
}