namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing algorithm operations
/// </summary>
public class AlgorithmService : IAlgorithmService
{
    private readonly ILogger<AlgorithmService> _logger;
    private readonly ICustomAlgorithmRepository _algorithmRepository;
    private readonly IUserService _userService;

    public AlgorithmService(ILogger<AlgorithmService> logger, ICustomAlgorithmRepository algorithmRepository, IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmRepository = algorithmRepository ?? throw new ArgumentNullException(nameof(algorithmRepository));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<AlgorithmModel?> GetAlgorithmByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm with ID: {AlgorithmId}", id);
        return await _algorithmRepository.GetByIdAsync(id);
    }

    public async Task<AlgorithmModel?> GetAlgorithmByNameAsync(Guid userId, string name)
    {
        _logger.LogInformation("Getting algorithm by name: {Name} for user: {UserId}", name, userId);
        return await _algorithmRepository.GetByNameAsync(userId, name);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithms for user: {UserId}", userId);
        return await _algorithmRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync()
    {
        _logger.LogInformation("Getting public algorithms");
        return await _algorithmRepository.GetPublicAlgorithmsAsync();
    }

    public async Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null)
    {
        _logger.LogInformation("Searching algorithms with term: {SearchTerm}", searchTerm);
        return await _algorithmRepository.SearchAlgorithmsAsync(searchTerm, userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByStatusAsync(AlgorithmStatus status, Guid? userId = null)
    {
        _logger.LogInformation("Getting algorithms by status: {Status}", status);
        return await _algorithmRepository.GetByStatusAsync(status, userId);
    }

    public async Task<AlgorithmModel> CreateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Creating algorithm: {Name} for user: {UserId}", algorithm.Name, algorithm.UserId);

        var existingAlgorithm = await _algorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null)
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");

        algorithm.Parameters = ParseAlgorithmParameters(algorithm.Code);

        var user = await _userService.GetUserByIdAsync(algorithm.UserId);
        algorithm.Status = AlgorithmStatus.Draft;
        algorithm.CreatedAt = DateTime.UtcNow;
        algorithm.CreatedBy = user?.Username ?? "System";
        algorithm.UpdatedAt = DateTime.UtcNow;
        algorithm.UpdatedBy = user?.Username ?? "System";

        return await _algorithmRepository.CreateAsync(algorithm);
    }

    public async Task<AlgorithmModel> UpdateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Updating algorithm: {AlgorithmId}", algorithm.Id);

        var existingAlgorithm = await _algorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null && existingAlgorithm.Id != algorithm.Id)
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");

        algorithm.Parameters = ParseAlgorithmParameters(algorithm.Code);

        var user = await _userService.GetUserByIdAsync(algorithm.UserId);
        algorithm.UpdatedAt = DateTime.UtcNow;
        algorithm.UpdatedBy = user?.Username ?? "System";

        return await _algorithmRepository.UpdateAsync(algorithm);
    }

    public async Task<bool> DeleteAlgorithmAsync(Guid id)
    {
        _logger.LogInformation("Deleting algorithm: {AlgorithmId}", id);
        return await _algorithmRepository.DeleteAsync(id);
    }

    public async Task<long> GetAlgorithmCountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithm count for user: {UserId}", userId);
        return await _algorithmRepository.CountByUserIdAsync(userId);
    }



    private Dictionary<string, object> ParseAlgorithmParameters(string code)
    {
        // Remove all newlines and extra whitespace for easier parsing
        var normalizedCode = Regex.Replace(code, @"\s+", " ");

        // Extract method signature
        var regex = new Regex(@"public\s+TradeSignal\s+\w+\s*\((.*?)\)");
        var match = regex.Match(normalizedCode);

        if (!match.Success)
            return new Dictionary<string, object>();

        var parameters = new Dictionary<string, object>();
        var paramString = match.Groups[1].Value;
        var paramPairs = paramString.Split(',');

        // System-provided parameters to exclude
        var systemParams = new[] { "closePrices", "currentPosition", "currentPrice" };

        foreach (var param in paramPairs)
        {
            var trimmed = param.Trim();
            var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                var type = parts[0];
                var name = parts[1];

                // Skip system parameters
                if (!systemParams.Contains(name))
                {
                    parameters[name] = new { type, defaultValue = GetDefaultValue(type) };
                }
            }
        }

        return parameters;
    }

    private object? GetDefaultValue(string type)
    {
        return type switch
        {
            "int" => 0,
            "decimal" => 0.0m,
            "bool" => false,
            _ => null
        };
    }
}