namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for managing and retrieving all algorithms (hard-coded and custom)
/// </summary>
public class AlgorithmRegistryService : IAlgorithmRegistryService
{
    private readonly ILogger<AlgorithmRegistryService> _logger;
    private readonly IAlgorithmRepository _algorithmRepository;
    private readonly IAlgorithmEffectivenessRepository _effectivenessRepository;
    private readonly IAlgorithmService _algorithmService;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, Type> _hardCodedAlgorithmMap = new();

    public AlgorithmRegistryService(ILogger<AlgorithmRegistryService> logger, IAlgorithmRepository algorithmRepository, IAlgorithmEffectivenessRepository effectivenessRepository,
                                    IAlgorithmService algorithmService, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmRepository = algorithmRepository ?? throw new ArgumentNullException(nameof(algorithmRepository));
        _effectivenessRepository = effectivenessRepository ?? throw new ArgumentNullException(nameof(effectivenessRepository));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        InitializeHardCodedAlgorithmMap();
    }

    public async Task<IEnumerable<AlgorithmMetadataModel>> GetAllAvailableAlgorithmsAsync(Guid userId)
    {
        _logger.LogInformation("Getting all available algorithms for user: {UserId}", userId);

        var allAlgorithms = new List<AlgorithmMetadataModel>();

        // Get enabled hard-coded algorithms from SQL Server
        var hardCodedAlgorithms = await _algorithmRepository.GetBySourceAsync(AlgorithmSource.HardCoded);
        allAlgorithms.AddRange(hardCodedAlgorithms.Where(_ => _.IsEnabled));

        // Get custom algorithms from MongoDB for this user
        var customAlgorithms = await _algorithmService.GetAlgorithmsByUserIdAsync(userId);

        foreach (var customAlgo in customAlgorithms)
        {
            allAlgorithms.Add(new AlgorithmMetadataModel
            {
                Id = customAlgo.Id,
                Name = customAlgo.Name,
                Description = customAlgo.Description,
                AlgorithmType = AlgorithmType.Unknown, // Custom algorithms don't have type classification yet
                AlgorithmSource = customAlgo.ProgrammingLanguage.ToLower() == "csharp"
                    ? AlgorithmSource.CustomCSharp
                    : AlgorithmSource.CustomPython,
                IsEnabled = true,
                Version = "1.0",
                CreatedAt = customAlgo.CreatedAt,
                UpdatedAt = customAlgo.UpdatedAt,
                CreatedBy = customAlgo.CreatedBy,
                UpdatedBy = customAlgo.UpdatedBy
            });
        }

        _logger.LogInformation("Found {Count} total algorithms ({HardCoded} hard-coded, {Custom} custom)",
            allAlgorithms.Count,
            hardCodedAlgorithms.Count(_ => _.IsEnabled),
            customAlgorithms.Count());

        return allAlgorithms.OrderBy(_ => _.Name);
    }

    public async Task<ITradingAlgorithm?> GetHardCodedAlgorithmAsync(Guid algorithmId)
    {
        _logger.LogInformation("Getting hard-coded algorithm: {AlgorithmId}", algorithmId);

        var metadata = await _algorithmRepository.GetByIdAsync(algorithmId);

        if (metadata == null || metadata.AlgorithmSource != AlgorithmSource.HardCoded)
        {
            _logger.LogWarning("Algorithm {AlgorithmId} not found or not hard-coded", algorithmId);
            return null;
        }

        if (!metadata.IsEnabled)
        {
            _logger.LogWarning("Algorithm {AlgorithmId} is disabled", algorithmId);
            return null;
        }

        // Map algorithm name to C# type
        if (!_hardCodedAlgorithmMap.TryGetValue(metadata.Name, out var algorithmType))
        {
            _logger.LogWarning("No implementation found for hard-coded algorithm: {Name}", metadata.Name);
            return null;
        }

        var algorithm = _serviceProvider.GetService(algorithmType) as ITradingAlgorithm;

        if (algorithm == null)
        {
            _logger.LogError("Failed to resolve algorithm instance for type: {Type}", algorithmType.Name);
            return null;
        }

        // Set the AlgorithmId from the database
        algorithm.AlgorithmId = metadata.Id;

        _logger.LogInformation("Successfully retrieved hard-coded algorithm: {Name}", metadata.Name);
        return algorithm;
    }

    public async Task<bool> IsAlgorithmEnabledAsync(Guid algorithmId)
    {
        var metadata = await _algorithmRepository.GetByIdAsync(algorithmId);
        return metadata?.IsEnabled ?? false;
    }

    public async Task<IEnumerable<AlgorithmEffectivenessModel>> GetEffectivenessAsync(Guid algorithmId)
    {
        _logger.LogInformation("Getting effectiveness ratings for algorithm: {AlgorithmId}", algorithmId);
        return await _effectivenessRepository.GetByAlgorithmIdAsync(algorithmId);
    }

    public async Task<AlgorithmEffectivenessModel?> GetEffectivenessAsync(Guid algorithmId, string timeframe)
    {
        _logger.LogInformation("Getting effectiveness rating for algorithm: {AlgorithmId}, timeframe: {Timeframe}",
            algorithmId, timeframe);
        return await _effectivenessRepository.GetByAlgorithmAndTimeframeAsync(algorithmId, timeframe);
    }

    public async Task<List<ParameterDefinition>> GetParameterDefinitionsAsync(Guid algorithmId)
    {
        _logger.LogInformation("Getting parameter definitions for algorithm: {AlgorithmId}", algorithmId);

        var algorithm = await GetHardCodedAlgorithmAsync(algorithmId);

        if (algorithm == null)
        {
            _logger.LogWarning("Algorithm {AlgorithmId} not found or not hard-coded", algorithmId);
            return new List<ParameterDefinition>();
        }

        var definitions = algorithm.GetParameterDefinitions();
        _logger.LogInformation("Retrieved {Count} parameter definitions", definitions.Count);

        return definitions;
    }

    private void InitializeHardCodedAlgorithmMap()
    {
        _hardCodedAlgorithmMap = new Dictionary<string, Type>
        {
            { "Moving Average Crossover", typeof(MovingAverageCrossoverAlgorithm) },
            { "RSI Mean Reversion", typeof(RsiMeanReversionAlgorithm) },
            { "Bollinger Bands Breakout", typeof(BollingerBandsBreakoutAlgorithm) },
            { "MACD Crossover", typeof(MacdCrossoverAlgorithm) },
            { "VWAP Mean Reversion", typeof(VwapAlgorithm) }
        };

        _logger.LogInformation("Initialized hard-coded algorithm map with {Count} algorithms",
            _hardCodedAlgorithmMap.Count);
    }
}