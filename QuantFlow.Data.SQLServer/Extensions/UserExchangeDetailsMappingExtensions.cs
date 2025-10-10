namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between UserExchangeDetailsModel and UserExchangeDetailsEntity
/// </summary>
public static class UserExchangeDetailsMappingExtensions
{
    /// <summary>
    /// Converts UserExchangeDetailsEntity to UserExchangeDetailsModel
    /// </summary>
    public static UserExchangeDetailsModel ToBusinessModel(this UserExchangeDetailsEntity entity)
    {
        return new UserExchangeDetailsModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Exchange = entity.Exchange,
            KeyName = entity.KeyName,
            KeyValue = entity.KeyValue,
            IsEncrypted = entity.IsEncrypted,
            IsActive = entity.IsActive,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts UserExchangeDetailsModel to UserExchangeDetailsEntity
    /// </summary>
    public static UserExchangeDetailsEntity ToEntity(this UserExchangeDetailsModel model)
    {
        return new UserExchangeDetailsEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            Exchange = model.Exchange,
            KeyName = model.KeyName,
            KeyValue = model.KeyValue,
            IsEncrypted = model.IsEncrypted,
            IsActive = model.IsActive,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy,
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of UserExchangeDetailsEntities to UserExchangeDetailsModels
    /// </summary>
    public static IEnumerable<UserExchangeDetailsModel> ToBusinessModels(this IEnumerable<UserExchangeDetailsEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}