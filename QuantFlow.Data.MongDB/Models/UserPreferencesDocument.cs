namespace QuantFlow.Data.MongoDB.Models;

/// <summary>
/// MongoDB document representing user preferences and settings
/// </summary>
[BsonCollection("user_preferences")]
public class UserPreferencesDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; } = Guid.Empty;

    [BsonElement("theme")]
    public string Theme { get; set; } = "Light";

    [BsonElement("language")]
    public string Language { get; set; } = "en-US";

    [BsonElement("timezone")]
    public string Timezone { get; set; } = "UTC";

    [BsonElement("currency_display")]
    public string CurrencyDisplay { get; set; } = "USD";

    [BsonElement("dashboard_layout")]
    public BsonDocument DashboardLayout { get; set; } = new();

    [BsonElement("chart_settings")]
    public BsonDocument ChartSettings { get; set; } = new();

    [BsonElement("notification_settings")]
    public BsonDocument NotificationSettings { get; set; } = new();

    [BsonElement("trading_settings")]
    public BsonDocument TradingSettings { get; set; } = new();

    [BsonElement("risk_preferences")]
    public BsonDocument RiskPreferences { get; set; } = new();

    [BsonElement("favorite_symbols")]
    public List<string> FavoriteSymbols { get; set; } = [];

    [BsonElement("favorite_exchanges")]
    public List<string> FavoriteExchanges { get; set; } = [];

    [BsonElement("custom_alerts")]
    public List<BsonDocument> CustomAlerts { get; set; } = [];

    [BsonElement("quick_actions")]
    public List<string> QuickActions { get; set; } = [];

    [BsonElement("workspace_settings")]
    public BsonDocument WorkspaceSettings { get; set; } = new();

    [BsonElement("api_settings")]
    public BsonDocument ApiSettings { get; set; } = new();

    [BsonElement("privacy_settings")]
    public BsonDocument PrivacySettings { get; set; } = new();
    
    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}