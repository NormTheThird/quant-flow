namespace QuantFlow.Data.MongoDB.Models;

/// <summary>
/// MongoDB document representing a trading algorithm
/// </summary>
[BsonCollection("algorithms")]
public class AlgorithmDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; } = Guid.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = "csharp";

    [BsonElement("version")]
    public string Version { get; set; } = "1.0.0";

    [BsonElement("status")]
    public int Status { get; set; } = 1; // AlgorithmStatus enum

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();

    [BsonElement("risk_settings")]
    public BsonDocument RiskSettings { get; set; } = new();

    [BsonElement("performance_metrics")]
    public BsonDocument PerformanceMetrics { get; set; } = new();

    [BsonElement("is_public")]
    public bool IsPublic { get; set; } = false;

    [BsonElement("is_template")]
    public bool IsTemplate { get; set; } = false;

    [BsonElement("template_category")]
    public string? TemplateCategory { get; set; } = null;

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