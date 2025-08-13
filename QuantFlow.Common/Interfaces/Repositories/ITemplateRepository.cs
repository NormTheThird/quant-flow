namespace QuantFlow.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for template operations using business models
/// </summary>
public interface ITemplateRepository
{
    Task<TemplateModel?> GetByIdAsync(Guid id);
    Task<TemplateModel?> GetByNameAsync(string name);
    Task<IEnumerable<TemplateModel>> GetByCategoryAsync(string category, string? subcategory = null);
    Task<IEnumerable<TemplateModel>> GetPublicTemplatesAsync();
    Task<IEnumerable<TemplateModel>> GetFeaturedTemplatesAsync();
    Task<IEnumerable<TemplateModel>> SearchTemplatesAsync(string searchTerm);
    Task<IEnumerable<TemplateModel>> GetByTagsAsync(IEnumerable<string> tags);
    Task<IEnumerable<TemplateModel>> GetByDifficultyLevelAsync(string difficultyLevel);
    Task<IEnumerable<TemplateModel>> GetByRiskLevelAsync(string riskLevel);
    Task<TemplateModel> CreateAsync(TemplateModel template);
    Task<TemplateModel> UpdateAsync(TemplateModel template);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IncrementDownloadCountAsync(Guid id);
    Task<bool> UpdateRatingAsync(Guid id, decimal rating, int reviewCount);
}