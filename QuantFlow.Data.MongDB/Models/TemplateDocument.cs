namespace QuantFlow.Data.MongoDB.Models;

/// <summary>
/// MongoDB document representing algorithm templates and presets
/// </summary>
[BsonCollection("templates")]
public class TemplateDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("subcategory")]
    public string? Subcategory { get; set; } = null;

    [BsonElement("difficulty_level")]
    public string DifficultyLevel { get; set; } = "beginner";

    [BsonElement("code_template")]
    public string CodeTemplate { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = "csharp";

    [BsonElement("framework")]
    public string Framework { get; set; } = string.Empty;

    [BsonElement("default_parameters")]
    public BsonDocument DefaultParameters { get; set; } = new();

    [BsonElement("parameter_schema")]
    public BsonDocument ParameterSchema { get; set; } = new();

    [BsonElement("documentation")]
    public BsonDocument Documentation { get; set; } = new();

    [BsonElement("examples")]
    public List<BsonDocument> Examples { get; set; } = [];

    [BsonElement("dependencies")]
    public List<string> Dependencies { get; set; } = [];

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("strategies")]
    public List<string> Strategies { get; set; } = [];

    [BsonElement("market_types")]
    public List<string> MarketTypes { get; set; } = [];

    [BsonElement("timeframes")]
    public List<string> Timeframes { get; set; } = [];

    [BsonElement("risk_level")]
    public string RiskLevel { get; set; } = "medium";

    [BsonElement("expected_returns")]
    public BsonDocument ExpectedReturns { get; set; } = new();

    [BsonElement("performance_benchmarks")]
    public BsonDocument PerformanceBenchmarks { get; set; } = new();

    [BsonElement("is_public")]
    public bool IsPublic { get; set; } = true;

    [BsonElement("is_featured")]
    public bool IsFeatured { get; set; } = false;

    [BsonElement("is_premium")]
    public bool IsPremium { get; set; } = false;

    [BsonElement("version")]
    public string Version { get; set; } = "1.0.0";

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    [BsonElement("license")]
    public string License { get; set; } = "MIT";

    [BsonElement("download_count")]
    public long DownloadCount { get; set; } = 0;

    [BsonElement("rating")]
    public decimal Rating { get; set; } = 0.0m;

    [BsonElement("review_count")]
    public int ReviewCount { get; set; } = 0;

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? UpdatedAt { get; set; } = null;

    [BsonElement("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("updated_by")]
    public string? UpdatedBy { get; set; } = null;

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}