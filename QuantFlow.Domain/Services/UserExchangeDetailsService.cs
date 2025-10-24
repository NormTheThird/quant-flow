using QuantFlow.Common.Interfaces.Repositories.SQLServer;

namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing user exchange details with encryption support
/// </summary>
public class UserExchangeDetailsService : IUserExchangeDetailsService
{
    private readonly ILogger<UserExchangeDetailsService> _logger;
    private readonly IUserExchangeDetailsRepository _repository;
    private readonly IEncryptionService _encryptionService;

    public UserExchangeDetailsService(
        ILogger<UserExchangeDetailsService> logger,
        IUserExchangeDetailsRepository repository,
        IEncryptionService encryptionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    public async Task<UserExchangeDetailsModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting exchange details by ID: {Id}", id);
        var model = await _repository.GetByIdAsync(id);

        if (model != null && model.IsEncrypted)
        {
            model.KeyValue = _encryptionService.Decrypt(model.KeyValue);
        }

        return model;
    }

    public async Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAndExchangeAsync(Guid userId, Exchange exchange)
    {
        _logger.LogInformation("Getting exchange details for user {UserId} and exchange {Exchange}", userId, exchange);

        var allDetails = await _repository.GetByUserIdAsync(userId);

        return allDetails.Where(d => d.Exchange == exchange.ToString());
    }

    public async Task<IEnumerable<UserExchangeDetailsModel>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting all exchange details for user: {UserId}", userId);
        var models = await _repository.GetByUserIdAsync(userId);

        return models.Select(DecryptIfNeeded).ToList();
    }

    public async Task<UserExchangeDetailsModel> CreateAsync(UserExchangeDetailsModel model)
    {
        _logger.LogInformation("Creating exchange detail for user: {UserId}, exchange: {Exchange}, key: {KeyName}",
            model.UserId, model.Exchange, model.KeyName);

        if (model.IsEncrypted)
        {
            model.KeyValue = _encryptionService.Encrypt(model.KeyValue);
        }

        return await _repository.CreateAsync(model);
    }

    public async Task<UserExchangeDetailsModel> UpdateAsync(UserExchangeDetailsModel model)
    {
        _logger.LogInformation("Updating exchange detail: {Id}", model.Id);

        if (model.IsEncrypted)
        {
            model.KeyValue = _encryptionService.Encrypt(model.KeyValue);
        }

        return await _repository.UpdateAsync(model);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting exchange detail: {Id}", id);
        return await _repository.DeleteAsync(id);
    }

    private UserExchangeDetailsModel DecryptIfNeeded(UserExchangeDetailsModel model)
    {
        if (model.IsEncrypted)
        {
            model.KeyValue = _encryptionService.Decrypt(model.KeyValue);
        }
        return model;
    }
}