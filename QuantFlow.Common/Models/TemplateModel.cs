namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing algorithm templates
/// </summary>
public class TemplateModel : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; } = null;
    public string DifficultyLevel { get; set; } = "beginner";
    public string CodeTemplate { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
    public string Framework { get; set; } = string.Empty;
    public object DefaultParameters { get; set; } = new();
    public object ParameterSchema { get; set; } = new();
    public object Documentation { get; set; } = new();
    public IEnumerable<object> Examples { get; set; } = [];
    public IEnumerable<string> Dependencies { get; set; } = [];
    public IEnumerable<string> Tags { get; set; } = [];
    public IEnumerable<string> Strategies { get; set; } = [];
    public IEnumerable<string> MarketTypes { get; set; } = [];
    public IEnumerable<string> Timeframes { get; set; } = [];
    public string RiskLevel { get; set; } = "medium";
    public object ExpectedReturns { get; set; } = new();
    public object PerformanceBenchmarks { get; set; } = new();
    public bool IsPublic { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsPremium { get; set; } = false;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public string License { get; set; } = "MIT";
    public long DownloadCount { get; set; } = 0;
    public decimal Rating { get; set; } = 0.0m;
    public int ReviewCount { get; set; } = 0;
}