namespace QuantFlow.Common.Models;

/// <summary>
/// Business model representing system configuration
/// </summary>
public class ConfigurationModel : BaseModel
{
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; } = null;
    public object Value { get; set; } = new();
    public string DataType { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public object DefaultValue { get; set; } = new();
    public object ValidationRules { get; set; } = new();
    public bool IsEncrypted { get; set; } = false;
    public bool IsSystem { get; set; } = false;
    public bool IsReadonly { get; set; } = false;
    public bool IsUserConfigurable { get; set; } = true;
    public string Environment { get; set; } = "production";
    public DateTime? EffectiveDate { get; set; } = null;
    public DateTime? ExpiryDate { get; set; } = null;
    public IEnumerable<string> Tags { get; set; } = [];
    public object Metadata { get; set; } = new();
    public int Version { get; set; } = 1;
}