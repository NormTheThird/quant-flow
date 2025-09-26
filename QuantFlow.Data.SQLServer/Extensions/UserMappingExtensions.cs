namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between UserModel and UserEntity
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Converts UserEntity to UserModel WITH password hash (for authentication only)
    /// </summary>
    public static UserModel ToBusinessModel(this UserEntity entity)
    {
        return new UserModel
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            IsEmailVerified = entity.IsEmailVerified,
            IsSystemAdmin = entity.IsSystemAdmin,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
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

    /// <summary>
    /// Converts UserModel to UserEntity (password hash should be set separately)
    /// </summary>
    public static UserEntity ToEntity(this UserModel model)
    {
        return new UserEntity
        {
            Id = model.Id,
            Username = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            IsEmailVerified = model.IsEmailVerified,
            IsSystemAdmin = model.IsSystemAdmin,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            CreatedBy = model.CreatedBy ?? "System",
            UpdatedAt = model.UpdatedAt,
            UpdatedBy = model.UpdatedBy
        };
    }
}