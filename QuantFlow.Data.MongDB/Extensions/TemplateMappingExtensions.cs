namespace QuantFlow.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for mapping between TemplateModel and TemplateDocument
/// </summary>
public static class TemplateMappingExtensions2
{
    /// <summary>
    /// Converts TemplateDocument to TemplateModel
    /// </summary>
    /// <param name="document">The document to convert</param>
    /// <returns>TemplateModel business object</returns>
    public static TemplateModel ToBusinessModel(this TemplateDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        return new TemplateModel
        {
            Id = document.Id,
            Name = document.Name,
            DisplayName = document.DisplayName,
            Description = document.Description,
            Category = document.Category,
            Subcategory = document.Subcategory,
            DifficultyLevel = document.DifficultyLevel,
            CodeTemplate = document.CodeTemplate,
            Language = document.Language,
            Framework = document.Framework,
            DefaultParameters = ConvertBsonToObject(document.DefaultParameters),
            ParameterSchema = ConvertBsonToObject(document.ParameterSchema),
            Documentation = ConvertBsonToObject(document.Documentation),
            Examples = ConvertBsonArrayToObjectList(document.Examples),
            Dependencies = document.Dependencies ?? [],
            Tags = document.Tags ?? [],
            Strategies = document.Strategies ?? [],
            MarketTypes = document.MarketTypes ?? [],
            Timeframes = document.Timeframes ?? [],
            RiskLevel = document.RiskLevel,
            ExpectedReturns = ConvertBsonToObject(document.ExpectedReturns),
            PerformanceBenchmarks = ConvertBsonToObject(document.PerformanceBenchmarks),
            IsPublic = document.IsPublic,
            IsFeatured = document.IsFeatured,
            IsPremium = document.IsPremium,
            Version = document.Version,
            Author = document.Author,
            License = document.License,
            DownloadCount = document.DownloadCount,
            Rating = document.Rating,
            ReviewCount = document.ReviewCount,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            CreatedBy = document.CreatedBy,
            UpdatedBy = document.UpdatedBy,
            IsDeleted = document.IsDeleted
        };
    }

    /// <summary>
    /// Converts TemplateModel to TemplateDocument
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>TemplateDocument for database operations</returns>
    public static TemplateDocument ToDocument(this TemplateModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new TemplateDocument
        {
            Id = model.Id,
            Name = model.Name,
            DisplayName = model.DisplayName,
            Description = model.Description,
            Category = model.Category,
            Subcategory = model.Subcategory,
            DifficultyLevel = model.DifficultyLevel,
            CodeTemplate = model.CodeTemplate,
            Language = model.Language,
            Framework = model.Framework,
            DefaultParameters = ConvertObjectToBson(model.DefaultParameters),
            ParameterSchema = ConvertObjectToBson(model.ParameterSchema),
            Documentation = ConvertObjectToBson(model.Documentation),
            Examples = ConvertObjectListToBsonArray(model.Examples),
            Dependencies = model.Dependencies?.ToList() ?? [],
            Tags = model.Tags?.ToList() ?? [],
            Strategies = model.Strategies?.ToList() ?? [],
            MarketTypes = model.MarketTypes?.ToList() ?? [],
            Timeframes = model.Timeframes?.ToList() ?? [],
            RiskLevel = model.RiskLevel,
            ExpectedReturns = ConvertObjectToBson(model.ExpectedReturns),
            PerformanceBenchmarks = ConvertObjectToBson(model.PerformanceBenchmarks),
            IsPublic = model.IsPublic,
            IsFeatured = model.IsFeatured,
            IsPremium = model.IsPremium,
            Version = model.Version,
            Author = model.Author,
            License = model.License,
            DownloadCount = model.DownloadCount,
            Rating = model.Rating,
            ReviewCount = model.ReviewCount,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Converts a collection of TemplateDocuments to TemplateModels
    /// </summary>
    /// <param name="documents">Collection of documents</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<TemplateModel> ToBusinessModels(this IEnumerable<TemplateDocument> documents)
    {
        return documents?.Select(d => d.ToBusinessModel()) ?? Enumerable.Empty<TemplateModel>();
    }

    /// <summary>
    /// Converts a collection of TemplateModels to TemplateDocuments
    /// </summary>
    /// <param name="models">Collection of business models</param>
    /// <returns>Collection of documents</returns>
    public static IEnumerable<TemplateDocument> ToDocuments(this IEnumerable<TemplateModel> models)
    {
        return models?.Select(m => m.ToDocument()) ?? Enumerable.Empty<TemplateDocument>();
    }

    /// <summary>
    /// Updates an existing document with values from a business model
    /// </summary>
    /// <param name="document">Document to update</param>
    /// <param name="model">Business model with new values</param>
    /// <returns>Updated document</returns>
    public static TemplateDocument UpdateFromModel(this TemplateDocument document, TemplateModel model)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (model == null) throw new ArgumentNullException(nameof(model));

        document.Name = model.Name;
        document.DisplayName = model.DisplayName;
        document.Description = model.Description;
        document.Category = model.Category;
        document.Subcategory = model.Subcategory;
        document.DifficultyLevel = model.DifficultyLevel;
        document.CodeTemplate = model.CodeTemplate;
        document.Language = model.Language;
        document.Framework = model.Framework;
        document.DefaultParameters = ConvertObjectToBson(model.DefaultParameters);
        document.ParameterSchema = ConvertObjectToBson(model.ParameterSchema);
        document.Documentation = ConvertObjectToBson(model.Documentation);
        document.Examples = ConvertObjectListToBsonArray(model.Examples);
        document.Dependencies = model.Dependencies?.ToList() ?? [];
        document.Tags = model.Tags?.ToList() ?? [];
        document.Strategies = model.Strategies?.ToList() ?? [];
        document.MarketTypes = model.MarketTypes?.ToList() ?? [];
        document.Timeframes = model.Timeframes?.ToList() ?? [];
        document.RiskLevel = model.RiskLevel;
        document.ExpectedReturns = ConvertObjectToBson(model.ExpectedReturns);
        document.PerformanceBenchmarks = ConvertObjectToBson(model.PerformanceBenchmarks);
        document.IsPublic = model.IsPublic;
        document.IsFeatured = model.IsFeatured;
        document.IsPremium = model.IsPremium;
        document.Version = model.Version;
        document.Author = model.Author;
        document.License = model.License;
        document.DownloadCount = model.DownloadCount;
        document.Rating = model.Rating;
        document.ReviewCount = model.ReviewCount;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = model.UpdatedBy;

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