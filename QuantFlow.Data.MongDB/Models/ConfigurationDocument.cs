namespace QuantFlow.Data.MongoDB.Models;

/// <summary>
/// MongoDB document representing system and application configurations
/// </summary>
[BsonCollection("configurations")]
public class ConfigurationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("subcategory")]
    public string? Subcategory { get; set; } = null;

    [BsonElement("value")]
    public BsonValue Value { get; set; } = BsonNull.Value;

    [BsonElement("data_type")]
    public string DataType { get; set; } = "string";

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("default_value")]
    public BsonValue DefaultValue { get; set; } = BsonNull.Value;

    [BsonElement("validation_rules")]
    public BsonDocument ValidationRules { get; set; } = new();

    [BsonElement("is_encrypted")]
    public bool IsEncrypted { get; set; } = false;

    [BsonElement("is_system")]
    public bool IsSystem { get; set; } = false;

    [BsonElement("is_readonly")]
    public bool IsReadonly { get; set; } = false;

    [BsonElement("is_user_configurable")]
    public bool IsUserConfigurable { get; set; } = true;

    [BsonElement("environment")]
    public string Environment { get; set; } = "production";

    [BsonElement("effective_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? EffectiveDate { get; set; } = null;

    [BsonElement("expiry_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ExpiryDate { get; set; } = null;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();

    [BsonElement("version")]
    public int Version { get; set; } = 1;

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_by")]
    public string UpdatedBy { get; set; } = string.Empty;
}