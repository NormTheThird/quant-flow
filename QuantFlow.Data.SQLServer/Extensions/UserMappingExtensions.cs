namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between UserModel and UserEntity
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Converts UserEntity to UserModel
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>UserModel business object</returns>
    public static UserModel ToBusinessModel(this UserEntity entity)
    {
        return new UserModel
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            PasswordHash = entity.PasswordHash,
            IsEmailVerified = entity.IsEmailVerified,
            IsSystemAdmin = entity.IsSystemAdmin,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts UserModel to UserEntity
    /// </summary>
    /// <param name="model">The business model to convert</param>
    /// <returns>UserEntity for database operations</returns>
    public static UserEntity ToEntity(this UserModel model)
    {
        return new UserEntity
        {
            Id = model.Id,
            Username = model.Username,
            Email = model.Email,
            PasswordHash = model.PasswordHash,
            IsEmailVerified = model.IsEmailVerified,
            IsSystemAdmin = model.IsSystemAdmin,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy,
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of UserEntities to UserModels
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Collection of business models</returns>
    public static IEnumerable<UserModel> ToBusinessModels(this IEnumerable<UserEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}