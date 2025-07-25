namespace QuantFlow.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for mapping between ConfigurationModel and ConfigurationDocument
/// </summary>
public static class ConfigurationMappingExtensions
{
    /// <summary>
    /// Converts ConfigurationDocument to ConfigurationModel
    /// </summary>
    /// <param name="document">The document to convert</param>
    /// <returns>ConfigurationModel business object</returns>
    public static ConfigurationModel ToBusinessModel(this ConfigurationDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        return new ConfigurationModel
        {
            Id = document.Id,
            Key = document.Key,
            Category = document.Category,
            Subcategory = document.Subcategory,
            Value = ConvertBsonValueToObject(document.Value) ?? new object(),
            DataType = document.DataType,
            Description = document.Description,
            DefaultValue = ConvertBsonValueToObject(document.DefaultValue) ?? new object(),
            ValidationRules = ConvertBsonToObject(document.ValidationRules),
            IsEncrypted = document.IsEncrypted,
            IsSystem = document.IsSystem,
            IsReadonly = document.IsReadonly,
            IsUserConfigurable = document.IsUserConfigurable,
            Environment = document.Environment,
            EffectiveDate = document.EffectiveDate,
            ExpiryDate = document.ExpiryDate,
            Tags = document.Tags ?? [],
            Metadata = ConvertBsonToObject(document.Metadata),
            Version = document.Version,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            CreatedBy = document.CreatedBy,
            UpdatedBy = document.UpdatedBy,
            IsDeleted = document.IsDeleted
        };
    }

    /// <summary>
    /// Converts ConfigurationModel to ConfigurationDocument
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>ConfigurationDocument for database operations</returns>
    public static ConfigurationDocument ToDocument(this ConfigurationModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new ConfigurationDocument
        {
            Id = model.Id,
            Key = model.Key,
            Category = model.Category,
            Subcategory = model.Subcategory,
            Value = ConvertObjectToBsonValue(model.Value),
            DataType = model.DataType,
            Description = model.Description,
            DefaultValue = ConvertObjectToBsonValue(model.DefaultValue),
            ValidationRules = ConvertObjectToBson(model.ValidationRules),
            IsEncrypted = model.IsEncrypted,
            IsSystem = model.IsSystem,
            IsReadonly = model.IsReadonly,
            IsUserConfigurable = model.IsUserConfigurable,
            Environment = model.Environment,
            EffectiveDate = model.EffectiveDate,
            ExpiryDate = model.ExpiryDate,
            Tags = model.Tags?.ToList() ?? [],
            Metadata = ConvertObjectToBson(model.Metadata),
            Version = model.Version,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Converts a collection of ConfigurationDocuments to ConfigurationModels
    /// </summary>
    /// <param name="documents">Collection of documents</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<ConfigurationModel> ToBusinessModels(this IEnumerable<ConfigurationDocument> documents)
    {
        return documents?.Select(d => d.ToBusinessModel()) ?? Enumerable.Empty<ConfigurationModel>();
    }

    /// <summary>
    /// Converts a collection of ConfigurationModels to ConfigurationDocuments
    /// </summary>
    /// <param name="models">Collection of business models</param>
    /// <returns>Collection of documents</returns>
    public static IEnumerable<ConfigurationDocument> ToDocuments(this IEnumerable<ConfigurationModel> models)
    {
        return models?.Select(m => m.ToDocument()) ?? Enumerable.Empty<ConfigurationDocument>();
    }

    /// <summary>
    /// Updates an existing document with values from a business model
    /// </summary>
    /// <param name="document">Document to update</param>
    /// <param name="model">Business model with new values</param>
    /// <returns>Updated document</returns>
    public static ConfigurationDocument UpdateFromModel(this ConfigurationDocument document, ConfigurationModel model)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (model == null) throw new ArgumentNullException(nameof(model));

        document.Key = model.Key;
        document.Category = model.Category;
        document.Subcategory = model.Subcategory;
        document.Value = ConvertObjectToBsonValue(model.Value);
        document.DataType = model.DataType;
        document.Description = model.Description;
        document.DefaultValue = ConvertObjectToBsonValue(model.DefaultValue);
        document.ValidationRules = ConvertObjectToBson(model.ValidationRules);
        document.IsEncrypted = model.IsEncrypted;
        document.IsSystem = model.IsSystem;
        document.IsReadonly = model.IsReadonly;
        document.IsUserConfigurable = model.IsUserConfigurable;
        document.Environment = model.Environment;
        document.EffectiveDate = model.EffectiveDate;
        document.ExpiryDate = model.ExpiryDate;
        document.Tags = model.Tags?.ToList() ?? [];
        document.Metadata = ConvertObjectToBson(model.Metadata);
        document.Version = model.Version;
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
    /// Converts an object to BsonValue
    /// </summary>
    private static BsonValue ConvertObjectToBsonValue(object? obj)
    {
        if (obj == null) return BsonNull.Value;
        return BsonValue.Create(obj);
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
    /// Converts a BsonValue to appropriate .NET object
    /// </summary>
    private static object? ConvertBsonValueToObject(BsonValue? bsonValue)
    {
        if (bsonValue == null || bsonValue.IsBsonNull) return null;

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
            BsonType.ObjectId => bsonValue.AsObjectId.ToString(),
            BsonType.Null => null,
            _ => bsonValue.ToString()
        };
    }
}