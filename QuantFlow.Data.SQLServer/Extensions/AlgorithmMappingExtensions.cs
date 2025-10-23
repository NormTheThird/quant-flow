namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between AlgorithmEntity and AlgorithmMetadataModel
/// </summary>
public static class AlgorithmMappingExtensions
{
    /// <summary>
    /// Converts AlgorithmEntity to business model
    /// </summary>
    public static AlgorithmMetadataModel ToBusinessModel(this AlgorithmEntity entity)
    {
        return new AlgorithmMetadataModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Abbreviation = entity.Abbreviation,
            Description = entity.Description,
            AlgorithmType = (AlgorithmType)entity.AlgorithmType,
            AlgorithmSource = (AlgorithmSource)entity.AlgorithmSource,
            IsEnabled = entity.IsEnabled,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts business model to AlgorithmEntity
    /// </summary>
    public static AlgorithmEntity ToEntity(this AlgorithmMetadataModel model)
    {
        return new AlgorithmEntity
        {
            Id = model.Id,
            Name = model.Name,
            Abbreviation = model.Abbreviation,
            Description = model.Description,
            AlgorithmType = (int)model.AlgorithmType,
            AlgorithmSource = (int)model.AlgorithmSource,
            IsEnabled = model.IsEnabled,
            Version = model.Version,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts collection of entities to business models
    /// </summary>
    public static IEnumerable<AlgorithmMetadataModel> ToBusinessModels(this IEnumerable<AlgorithmEntity> entities)
    {
        return entities.Select(_ => _.ToBusinessModel());
    }
}