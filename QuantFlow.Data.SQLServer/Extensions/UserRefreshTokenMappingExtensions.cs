namespace QuantFlow.Data.SQLServer.Extensions;

/// <summary>
/// Extension methods for mapping between RefreshTokenModel and RefreshTokenEntity
/// </summary>
public static class UserRefreshTokenMappingExtensions
{
    /// <summary>
    /// Converts RefreshTokenEntity to RefreshTokenModel
    /// </summary>
    public static UserRefreshTokenModel ToBusinessModel(this UserRefreshTokenEntity entity)
    {
        return new UserRefreshTokenModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Token = entity.Token,
            ExpiresAt = entity.ExpiresAt,
            IsRevoked = entity.IsRevoked,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// Converts RefreshTokenModel to RefreshTokenEntity
    /// </summary>
    public static UserRefreshTokenEntity ToEntity(this UserRefreshTokenModel model)
    {
        return new UserRefreshTokenEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            Token = model.Token,
            ExpiresAt = model.ExpiresAt,
            IsRevoked = model.IsRevoked,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            CreatedBy = model.CreatedBy ?? "System",
            UpdatedBy = model.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a collection of RefreshTokenEntities to RefreshTokenModels
    /// </summary>
    public static IEnumerable<UserRefreshTokenModel> ToBusinessModels(this IEnumerable<UserRefreshTokenEntity> entities)
    {
        return entities.Select(e => e.ToBusinessModel());
    }
}