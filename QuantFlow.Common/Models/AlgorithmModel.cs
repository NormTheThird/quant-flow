namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing a trading algorithm
/// </summary>
public class AlgorithmModel : BaseModel
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
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