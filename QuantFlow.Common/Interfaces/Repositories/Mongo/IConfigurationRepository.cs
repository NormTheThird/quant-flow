namespace QuantFlow.Common.Interfaces.Repositories.Mongo;

/// <summary>
/// Repository interface for configuration operations using business models
/// </summary>
public interface IConfigurationRepository
{
    Task<ConfigurationModel?> GetByKeyAsync(string key);
    Task<IEnumerable<ConfigurationModel>> GetByCategoryAsync(string category, string? subcategory = null);
    Task<IEnumerable<ConfigurationModel>> GetByEnvironmentAsync(string environment);
    Task<IEnumerable<ConfigurationModel>> GetAllAsync();
    Task<IEnumerable<ConfigurationModel>> GetUserConfigurableAsync();
    Task<IEnumerable<ConfigurationModel>> GetEffectiveConfigurationsAsync(DateTime? asOfDate = null);
    Task<ConfigurationModel> CreateAsync(ConfigurationModel configuration);
    Task<ConfigurationModel> UpdateAsync(ConfigurationModel configuration);
    Task<ConfigurationModel> UpsertAsync(ConfigurationModel configuration);
    Task<bool> DeleteAsync(string key);
    Task<bool> UpdateValueAsync(string key, object value);
    Task<IEnumerable<ConfigurationModel>> GetByTagsAsync(IEnumerable<string> tags);
    Task<IEnumerable<ConfigurationModel>> SearchAsync(string searchTerm);
}