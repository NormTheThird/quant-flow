namespace QuantFlow.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for mapping between AlgorithmModel and AlgorithmDocument
/// </summary>
public static class AlgorithmMappingExtensions
{
    /// <summary>
    /// Converts AlgorithmDocument to AlgorithmModel
    /// </summary>
    /// <param name="document">The document to convert</param>
    /// <returns>AlgorithmModel business object</returns>
    public static AlgorithmModel ToBusinessModel(this AlgorithmDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        return new AlgorithmModel
        {
            Id = document.Id,
            UserId = document.UserId,
            Name = document.Name,
            Description = document.Description,
            Code = document.Code,
            ProgrammingLanguage = document.ProgrammingLanguage,
            Version = document.Version,
            Status = (AlgorithmStatus)document.Status,
            Tags = document.Tags ?? [],
            Parameters = document.Parameters ?? new Dictionary<string, object>(),
            RiskSettings = ConvertBsonToObject(document.RiskSettings),
            PerformanceMetrics = ConvertBsonToObject(document.PerformanceMetrics),
            IsPublic = document.IsPublic,
            IsTemplate = document.IsTemplate,
            TemplateCategory = document.TemplateCategory,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            CreatedBy = document.CreatedBy,
            UpdatedBy = document.UpdatedBy,
            IsDeleted = document.IsDeleted
        };
    }

    /// <summary>
    /// Converts AlgorithmModel to AlgorithmDocument
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>AlgorithmDocument for database operations</returns>
    public static AlgorithmDocument ToDocument(this AlgorithmModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new AlgorithmDocument
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            Description = model.Description,
            Code = model.Code,
            ProgrammingLanguage = model.ProgrammingLanguage,
            Version = model.Version,
            Status = (int)model.Status,
            Tags = model.Tags?.ToList() ?? [],
            Parameters = model.Parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>(),
            RiskSettings = ConvertObjectToBson(model.RiskSettings),
            PerformanceMetrics = ConvertObjectToBson(model.PerformanceMetrics),
            IsPublic = model.IsPublic,
            IsTemplate = model.IsTemplate,
            TemplateCategory = model.TemplateCategory,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Converts a collection of AlgorithmDocuments to AlgorithmModels
    /// </summary>
    /// <param name="documents">Collection of documents</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<AlgorithmModel> ToBusinessModels(this IEnumerable<AlgorithmDocument> documents)
    {
        return documents?.Select(d => d.ToBusinessModel()) ?? Enumerable.Empty<AlgorithmModel>();
    }

    /// <summary>
    /// Converts a collection of AlgorithmModels to AlgorithmDocuments
    /// </summary>
    /// <param name="models">Collection of business models</param>
    /// <returns>Collection of documents</returns>
    public static IEnumerable<AlgorithmDocument> ToDocuments(this IEnumerable<AlgorithmModel> models)
    {
        return models?.Select(m => m.ToDocument()) ?? Enumerable.Empty<AlgorithmDocument>();
    }

    /// <summary>
    /// Updates an existing document with values from a business model
    /// </summary>
    /// <param name="document">Document to update</param>
    /// <param name="model">Business model with new values</param>
    /// <returns>Updated document</returns>
    public static AlgorithmDocument UpdateFromModel(this AlgorithmDocument document, AlgorithmModel model)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (model == null) throw new ArgumentNullException(nameof(model));

        document.Name = model.Name;
        document.Description = model.Description;
        document.Code = model.Code;
        document.ProgrammingLanguage = model.ProgrammingLanguage;
        document.Version = model.Version;
        document.Status = (int)model.Status;
        document.Tags = model.Tags?.ToList() ?? [];
        document.Parameters = model.Parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        document.RiskSettings = ConvertObjectToBson(model.RiskSettings);
        document.PerformanceMetrics = ConvertObjectToBson(model.PerformanceMetrics);
        document.IsPublic = model.IsPublic;
        document.IsTemplate = model.IsTemplate;
        document.TemplateCategory = model.TemplateCategory;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = model.UpdatedBy;

        return document;
    }

    /// <summary>
    /// Converts a dictionary to BsonDocument
    /// </summary>
    private static BsonDocument ConvertDictionaryToBson(IDictionary<string, object>? dictionary)
    {
        if (dictionary == null) return new BsonDocument();

        var bsonDoc = new BsonDocument();
        foreach (var kvp in dictionary)
        {
            bsonDoc[kvp.Key] = BsonValue.Create(kvp.Value);
        }
        return bsonDoc;
    }

    /// <summary>
    /// Converts a BsonDocument to dictionary
    /// </summary>
    private static Dictionary<string, object?> ConvertBsonToDictionary(BsonDocument? bsonDoc)
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
    /// Converts an object to BsonDocument
    /// </summary>
    private static BsonDocument ConvertObjectToBson(object? obj)
    {
        if (obj == null) return new BsonDocument();

        if (obj is Dictionary<string, object> dict)
            return ConvertDictionaryToBson(dict);

        return obj.ToBsonDocument();
    }

    /// <summary>
    /// Converts a BsonDocument to object
    /// </summary>
    private static object ConvertBsonToObject(BsonDocument? bsonDoc)
    {
        if (bsonDoc == null) return new Dictionary<string, object>();
        return ConvertBsonToDictionary(bsonDoc);
    }

    /// <summary>
    /// Converts a BsonValue to appropriate .NET object
    /// </summary>
    private static object? ConvertBsonValueToObject(BsonValue bsonValue)
    {
        return bsonValue.BsonType switch
        {
            BsonType.Document => ConvertBsonToDictionary(bsonValue.AsBsonDocument),
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