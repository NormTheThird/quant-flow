namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing algorithm operations (both hard-coded and custom algorithms)
/// </summary>
public class AlgorithmService : IAlgorithmService
{
    private readonly ILogger<AlgorithmService> _logger;
    private readonly ICustomAlgorithmRepository _customAlgorithmRepository;
    private readonly IAlgorithmRepository _algorithmMetadataRepository;
    private readonly IUserService _userService;

    public AlgorithmService(ILogger<AlgorithmService> logger, ICustomAlgorithmRepository customAlgorithmRepository, IAlgorithmRepository algorithmMetadataRepository,
                            IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _customAlgorithmRepository = customAlgorithmRepository ?? throw new ArgumentNullException(nameof(customAlgorithmRepository));
        _algorithmMetadataRepository = algorithmMetadataRepository ?? throw new ArgumentNullException(nameof(algorithmMetadataRepository));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<AlgorithmModel?> GetAlgorithmByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting algorithm with ID: {AlgorithmId}", id);

        // Try custom algorithms first (MongoDB)
        var customAlgorithm = await _customAlgorithmRepository.GetByIdAsync(id);
        if (customAlgorithm != null)
            return customAlgorithm;

        // If not found in custom, check if it's a hard-coded algorithm in SQL
        var metadata = await _algorithmMetadataRepository.GetByIdAsync(id);
        if (metadata != null)
        {
            // Convert metadata to AlgorithmModel for consistency
            return new AlgorithmModel
            {
                Id = metadata.Id,
                Name = metadata.Name,
                Description = metadata.Description,
                // Hard-coded algorithms don't have code in MongoDB
                Code = string.Empty,
                ProgrammingLanguage = "CSharp",
                Status = AlgorithmStatus.Active,
                UserId = Guid.Empty, // System algorithm
                CreatedAt = metadata.CreatedAt,
                UpdatedAt = metadata.UpdatedAt,
                CreatedBy = metadata.CreatedBy,
                UpdatedBy = metadata.UpdatedBy
            };
        }

        return null;
    }

    public async Task<AlgorithmModel?> GetAlgorithmByNameAsync(Guid userId, string name)
    {
        _logger.LogInformation("Getting algorithm by name: {Name} for user: {UserId}", name, userId);
        return await _customAlgorithmRepository.GetByNameAsync(userId, name);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting custom algorithms for user: {UserId}", userId);
        return await _customAlgorithmRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetPublicAlgorithmsAsync()
    {
        _logger.LogInformation("Getting public algorithms");
        return await _customAlgorithmRepository.GetPublicAlgorithmsAsync();
    }

    public async Task<IEnumerable<AlgorithmModel>> SearchAlgorithmsAsync(string searchTerm, Guid? userId = null)
    {
        _logger.LogInformation("Searching algorithms with term: {SearchTerm}", searchTerm);
        return await _customAlgorithmRepository.SearchAlgorithmsAsync(searchTerm, userId);
    }

    public async Task<IEnumerable<AlgorithmModel>> GetAlgorithmsByStatusAsync(AlgorithmStatus status, Guid? userId = null)
    {
        _logger.LogInformation("Getting algorithms by status: {Status}", status);
        return await _customAlgorithmRepository.GetByStatusAsync(status, userId);
    }

    public async Task<AlgorithmModel> CreateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Creating custom algorithm: {Name} for user: {UserId}", algorithm.Name, algorithm.UserId);

        var existingAlgorithm = await _customAlgorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null)
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");

        algorithm.Parameters = ParseAlgorithmParameters(algorithm.Code);

        var user = await _userService.GetUserByIdAsync(algorithm.UserId);
        algorithm.Status = AlgorithmStatus.Draft;
        algorithm.CreatedAt = DateTime.UtcNow;
        algorithm.CreatedBy = user?.Username ?? "System";
        algorithm.UpdatedAt = DateTime.UtcNow;
        algorithm.UpdatedBy = user?.Username ?? "System";

        return await _customAlgorithmRepository.CreateAsync(algorithm);
    }

    public async Task<AlgorithmModel> UpdateAlgorithmAsync(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Updating custom algorithm: {AlgorithmId}", algorithm.Id);

        var existingAlgorithm = await _customAlgorithmRepository.GetByNameAsync(algorithm.UserId, algorithm.Name);
        if (existingAlgorithm != null && existingAlgorithm.Id != algorithm.Id)
            throw new InvalidOperationException($"An algorithm with the name '{algorithm.Name}' already exists.");

        algorithm.Parameters = ParseAlgorithmParameters(algorithm.Code);

        var user = await _userService.GetUserByIdAsync(algorithm.UserId);
        algorithm.UpdatedAt = DateTime.UtcNow;
        algorithm.UpdatedBy = user?.Username ?? "System";

        return await _customAlgorithmRepository.UpdateAsync(algorithm);
    }

    public async Task<bool> DeleteAlgorithmAsync(Guid id)
    {
        _logger.LogInformation("Deleting custom algorithm: {AlgorithmId}", id);
        return await _customAlgorithmRepository.DeleteAsync(id);
    }

    public async Task<long> GetAlgorithmCountByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting algorithm count for user: {UserId}", userId);
        return await _customAlgorithmRepository.CountByUserIdAsync(userId);
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