namespace QuantFlow.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for mapping between UserPreferencesModel and UserPreferencesDocument
/// </summary>
public static class UserPreferencesMappingExtensions
{
    /// <summary>
    /// Converts UserPreferencesDocument to UserPreferencesModel
    /// </summary>
    /// <param name="document">The document to convert</param>
    /// <returns>UserPreferencesModel business object</returns>
    public static UserPreferencesModel ToBusinessModel(this UserPreferencesDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        return new UserPreferencesModel
        {
            Id = document.Id,
            UserId = document.UserId,
            Theme = document.Theme,
            Language = document.Language,
            Timezone = document.Timezone,
            CurrencyDisplay = document.CurrencyDisplay,
            DashboardLayout = ConvertBsonToObject(document.DashboardLayout),
            MarketOverviewCards = ConvertBsonToObject(document.MarketOverviewCards),
            ChartSettings = ConvertBsonToObject(document.ChartSettings),
            NotificationSettings = ConvertBsonToObject(document.NotificationSettings),
            TradingSettings = ConvertBsonToObject(document.TradingSettings),
            RiskPreferences = ConvertBsonToObject(document.RiskPreferences),
            FavoriteSymbols = document.FavoriteSymbols ?? [],
            FavoriteExchanges = document.FavoriteExchanges ?? [],
            CustomAlerts = ConvertBsonArrayToObjectList(document.CustomAlerts),
            QuickActions = document.QuickActions ?? [],
            WorkspaceSettings = ConvertBsonToObject(document.WorkspaceSettings),
            ApiSettings = ConvertBsonToObject(document.ApiSettings),
            PrivacySettings = ConvertBsonToObject(document.PrivacySettings),
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsDeleted = document.IsDeleted
        };
    }

    /// <summary>
    /// Converts UserPreferencesModel to UserPreferencesDocument
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>UserPreferencesDocument for database operations</returns>
    public static UserPreferencesDocument ToDocument(this UserPreferencesModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new UserPreferencesDocument
        {
            Id = model.Id,
            UserId = model.UserId,
            Theme = model.Theme,
            Language = model.Language,
            Timezone = model.Timezone,
            CurrencyDisplay = model.CurrencyDisplay,
            DashboardLayout = ConvertObjectToBson(model.DashboardLayout),
            MarketOverviewCards = ConvertObjectToBson(model.MarketOverviewCards),
            ChartSettings = ConvertObjectToBson(model.ChartSettings),
            NotificationSettings = ConvertObjectToBson(model.NotificationSettings),
            TradingSettings = ConvertObjectToBson(model.TradingSettings),
            RiskPreferences = ConvertObjectToBson(model.RiskPreferences),
            FavoriteSymbols = model.FavoriteSymbols?.ToList() ?? [],
            FavoriteExchanges = model.FavoriteExchanges?.ToList() ?? [],
            CustomAlerts = ConvertObjectListToBsonArray(model.CustomAlerts),
            QuickActions = model.QuickActions?.ToList() ?? [],
            WorkspaceSettings = ConvertObjectToBson(model.WorkspaceSettings),
            ApiSettings = ConvertObjectToBson(model.ApiSettings),
            PrivacySettings = ConvertObjectToBson(model.PrivacySettings),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Converts a collection of UserPreferencesDocuments to UserPreferencesModels
    /// </summary>
    /// <param name="documents">Collection of documents</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<UserPreferencesModel> ToBusinessModels(this IEnumerable<UserPreferencesDocument> documents)
    {
        return documents?.Select(d => d.ToBusinessModel()) ?? Enumerable.Empty<UserPreferencesModel>();
    }

    /// <summary>
    /// Updates an existing document with values from a business model
    /// </summary>
    /// <param name="document">Document to update</param>
    /// <param name="model">Business model with new values</param>
    /// <returns>Updated document</returns>
    public static UserPreferencesDocument UpdateFromModel(this UserPreferencesDocument document, UserPreferencesModel model)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (model == null) throw new ArgumentNullException(nameof(model));

        document.Theme = model.Theme;
        document.Language = model.Language;
        document.Timezone = model.Timezone;
        document.CurrencyDisplay = model.CurrencyDisplay;
        document.DashboardLayout = ConvertObjectToBson(model.DashboardLayout);
        document.MarketOverviewCards = ConvertObjectToBson(model.MarketOverviewCards);
        document.ChartSettings = ConvertObjectToBson(model.ChartSettings);
        document.NotificationSettings = ConvertObjectToBson(model.NotificationSettings);
        document.TradingSettings = ConvertObjectToBson(model.TradingSettings);
        document.RiskPreferences = ConvertObjectToBson(model.RiskPreferences);
        document.FavoriteSymbols = model.FavoriteSymbols?.ToList() ?? [];
        document.FavoriteExchanges = model.FavoriteExchanges?.ToList() ?? [];
        document.CustomAlerts = ConvertObjectListToBsonArray(model.CustomAlerts);
        document.QuickActions = model.QuickActions?.ToList() ?? [];
        document.WorkspaceSettings = ConvertObjectToBson(model.WorkspaceSettings);
        document.ApiSettings = ConvertObjectToBson(model.ApiSettings);
        document.PrivacySettings = ConvertObjectToBson(model.PrivacySettings);
        document.UpdatedAt = DateTime.UtcNow;

        return document;
    }

    /// <summary>
    /// Converts an object to BsonDocument
    /// </summary>
    private static BsonDocument ConvertObjectToBson(object? obj)
    {
        if (obj == null) return new BsonDocument();

        if (obj is Dictionary<string, object?> dictNullable)
        {
            var bsonDoc = new BsonDocument();
            foreach (var kvp in dictNullable)
            {
                bsonDoc[kvp.Key] = BsonValue.Create(kvp.Value);
            }
            return bsonDoc;
        }

        if (obj is Dictionary<string, object> dict)
        {
            var bsonDoc = new BsonDocument();
            foreach (var kvp in dict)
            {
                bsonDoc[kvp.Key] = BsonValue.Create(kvp.Value);
            }
            return bsonDoc;
        }

        return obj.ToBsonDocument();
    }

    /// <summary>
    /// Converts a BsonDocument to object
    /// </summary>
    private static object ConvertBsonToObject(BsonDocument? bsonDoc)
    {
        if (bsonDoc == null) return new Dictionary<string, object?>();

        var dictionary = new Dictionary<string, object?>();
        foreach (var element in bsonDoc.Elements)
        {
            dictionary[element.Name] = ConvertBsonValueToObject(element.Value);
        }
        return dictionary;
    }

    /// <summary>
    /// Converts a list of objects to BsonDocument list
    /// </summary>
    private static List<BsonDocument> ConvertObjectListToBsonArray(IEnumerable<object>? objects)
    {
        if (objects == null) return [];

        return objects.Select(obj => ConvertObjectToBson(obj)).ToList();
    }

    /// <summary>
    /// Converts a BsonDocument list to list of objects
    /// </summary>
    private static List<object> ConvertBsonArrayToObjectList(IEnumerable<BsonDocument>? bsonArray)
    {
        if (bsonArray == null) return [];

        return bsonArray.Select(bson => ConvertBsonToObject(bson)).ToList();
    }

    /// <summary>
    /// Converts a BsonValue to appropriate .NET object
    /// </summary>
    private static object? ConvertBsonValueToObject(BsonValue bsonValue)
    {
        return bsonValue.BsonType switch
        {
            BsonType.Document => ConvertBsonToObject(bsonValue.AsBsonDocument),
            BsonType.Array => bsonValue.AsBsonArray.Select(ConvertBsonValueToObject).ToList(),
            BsonType.String => bsonValue.AsString,
            BsonType.Int32 => bsonValue.AsInt32,
            BsonType.Int64 => bsonValue.AsInt64,
            BsonType.Double => bsonValue.AsDouble,
            BsonType.Decimal128 => bsonValue.AsDecimal,
            BsonType.Boolean => bsonValue.AsBoolean,
            BsonType.DateTime => bsonValue.ToUniversalTime(),
            BsonType.Null => null,
            _ => bsonValue.ToString()
        };
    }
}