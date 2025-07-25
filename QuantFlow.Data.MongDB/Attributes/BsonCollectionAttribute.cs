namespace QuantFlow.Data.MongoDB.Attributes;

/// <summary>
/// Attribute to specify MongoDB collection name for documents
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute : Attribute
{
    public string CollectionName { get; }

    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
    }
}