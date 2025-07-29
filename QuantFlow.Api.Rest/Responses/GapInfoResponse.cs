namespace QuantFlow.Api.Rest.Responses;

/// <summary>
/// Data gap information response model
/// </summary>
public class GapInfoResponse
{
    public DateTime StartTime { get; set; } = new();
    public DateTime EndTime { get; set; } = new();
    public string Timeframe { get; set; } = string.Empty;
    public int MissingPoints { get; set; } = 0;
}