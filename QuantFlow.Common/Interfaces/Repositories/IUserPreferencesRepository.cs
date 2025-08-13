namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for user preferences operations using business models
/// </summary>
public interface IUserPreferencesRepository
{
    Task<UserPreferencesModel?> GetByUserIdAsync(Guid userId);
    Task<UserPreferencesModel?> GetByIdAsync(Guid id);
    Task<UserPreferencesModel> CreateAsync(UserPreferencesModel preferences);
    Task<UserPreferencesModel> UpdateAsync(UserPreferencesModel preferences);
    Task<UserPreferencesModel> UpsertAsync(UserPreferencesModel preferences);
    Task<bool> DeleteByUserIdAsync(Guid userId);
    Task<bool> UpdateSectionAsync(Guid userId, string section, object value);
    Task<bool> AddFavoriteSymbolAsync(Guid userId, string symbol);
    Task<bool> RemoveFavoriteSymbolAsync(Guid userId, string symbol);
    Task<IEnumerable<UserPreferencesModel>> GetByThemeAsync(string theme);
    Task<UserPreferencesModel> GetDefaultPreferencesAsync(Guid userId);
}