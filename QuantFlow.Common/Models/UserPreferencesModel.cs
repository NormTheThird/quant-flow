namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing user preferences and settings
/// </summary>
public class UserPreferencesModel : BaseModel
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public string Timezone { get; set; } = "UTC";
    public string CurrencyDisplay { get; set; } = "USD";
    public object DashboardLayout { get; set; } = new();
    public object MarketOverviewCards { get; set; } = new();
    public object ChartSettings { get; set; } = new();
    public object NotificationSettings { get; set; } = new();
    public object TradingSettings { get; set; } = new();
    public object RiskPreferences { get; set; } = new();
    public IEnumerable<string> FavoriteSymbols { get; set; } = [];
    public IEnumerable<string> FavoriteExchanges { get; set; } = [];
    public IEnumerable<object> CustomAlerts { get; set; } = [];
    public IEnumerable<string> QuickActions { get; set; } = [];
    public object WorkspaceSettings { get; set; } = new();
    public object ApiSettings { get; set; } = new();
    public object PrivacySettings { get; set; } = new();
}