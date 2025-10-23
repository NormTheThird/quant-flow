namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for Add/Edit Algorithm Position dialog
/// Supports both standalone positions and portfolio-assigned positions
/// </summary>
public partial class AlgorithmPositionDialogViewModel : ObservableObject
{
    private readonly ILogger<AlgorithmPositionDialogViewModel> _logger;
    private readonly IAlgorithmPositionService _algorithmPositionService;
    private readonly IAlgorithmRegistryService _algorithmRegistryService;
    private readonly ISymbolService _symbolService;
    private readonly IUserSessionService _userSessionService;
    private readonly Guid? _portfolioId;
    private readonly AlgorithmPositionModel? _existingPosition;
    private bool _isInitializing = true;

    [ObservableProperty]
    private string _dialogTitle = "Add Position";

    [ObservableProperty]
    private string _positionName = string.Empty;

    [ObservableProperty]
    private List<AlgorithmModel> _availableAlgorithms = [];

    [ObservableProperty]
    private AlgorithmModel? _selectedAlgorithm;

    [ObservableProperty]
    private List<SymbolModel> _availableSymbols = [];

    [ObservableProperty]
    private SymbolModel? _selectedSymbol;

    [ObservableProperty]
    private string _allocatedPercent = "10.0";

    [ObservableProperty]
    private string _maxPositionSizePercent = "10.0";

    [ObservableProperty]
    private string _exchangeFees = "0.1";

    [ObservableProperty]
    private bool _allowShortSelling;

    [ObservableProperty]
    private bool _showAllocatedPercent;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

    [ObservableProperty]
    private bool _isLoadingAlgorithms = true;

    [ObservableProperty]
    private bool _isLoadingSymbols = true;

    [ObservableProperty]
    private List<ParameterViewModel> _parameterViewModels = [];

    [ObservableProperty]
    private bool _hasParameters;

    [ObservableProperty]
    private bool _isEditMode;

    public event EventHandler<bool>? SaveCompleted;

    public AlgorithmPositionDialogViewModel(
        ILogger<AlgorithmPositionDialogViewModel> logger,
        IAlgorithmPositionService algorithmPositionService,
        IAlgorithmRegistryService algorithmRegistryService,
        ISymbolService symbolService,
        IUserSessionService userSessionService,
        Guid? portfolioId = null,
        AlgorithmPositionModel? existingPosition = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));
        _algorithmRegistryService = algorithmRegistryService ?? throw new ArgumentNullException(nameof(algorithmRegistryService));
        _symbolService = symbolService ?? throw new ArgumentNullException(nameof(symbolService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _portfolioId = portfolioId;
        _existingPosition = existingPosition;

        ShowAllocatedPercent = _portfolioId.HasValue;
        IsEditMode = _existingPosition != null;
        DialogTitle = _existingPosition != null 
            ? "Edit Position"
            : _portfolioId.HasValue ? "Add Position to Portfolio" : "Create Position";

        var currentUserId = userSessionService.CurrentUserId;
        _ = LoadDataAsync(currentUserId);
    }

    private async Task LoadDataAsync(Guid userId)
    {
        try
        {
            _isInitializing = true;

            await LoadAlgorithmsAsync(userId);
            await LoadSymbolsAsync();

            // Load existing position data AFTER dropdowns are populated
            if (_existingPosition != null)
            {
                LoadExistingPosition();
            }

            _isInitializing = false;

            // Now trigger parameter loading if algorithm is selected
            if (SelectedAlgorithm != null)
            {
                await LoadAlgorithmParametersAsync(SelectedAlgorithm.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dialog data");
            ValidationMessage = $"Error loading data: {ex.Message}";
            HasValidationError = true;
            _isInitializing = false;
        }
    }

    private async Task LoadAlgorithmsAsync(Guid userId)
    {
        try
        {
            IsLoadingAlgorithms = true;
            _logger.LogInformation("Loading algorithms for user: {UserId}", userId);

            var algorithms = await _algorithmRegistryService.GetAllAvailableAlgorithmsAsync(userId);

            AvailableAlgorithms = algorithms.Select(_ => new AlgorithmModel
            {
                Id = _.Id,
                Name = _.Name,
                Description = _.Description,
                Status = AlgorithmStatus.Active
            }).ToList();

            if (_existingPosition != null && AvailableAlgorithms.Any())
            {
                SelectedAlgorithm = AvailableAlgorithms.FirstOrDefault(a => a.Id == _existingPosition.AlgorithmId);
            }

            _logger.LogInformation("Loaded {Count} available algorithms", AvailableAlgorithms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading algorithms");
            ValidationMessage = $"Error loading algorithms: {ex.Message}";
            HasValidationError = true;
        }
        finally
        {
            IsLoadingAlgorithms = false;
        }
    }

    private async Task LoadSymbolsAsync()
    {
        try
        {
            IsLoadingSymbols = true;
            _logger.LogInformation("Loading symbols");

            var symbols = await _symbolService.GetAllAsync();
            AvailableSymbols = symbols.OrderBy(s => s.Symbol).ToList();

            if (_existingPosition != null && AvailableSymbols.Any())
            {
                SelectedSymbol = AvailableSymbols.FirstOrDefault(s => s.Symbol == _existingPosition.Symbol);
            }

            _logger.LogInformation("Loaded {Count} symbols", AvailableSymbols.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading symbols");
            ValidationMessage = $"Error loading symbols: {ex.Message}";
            HasValidationError = true;
        }
        finally
        {
            IsLoadingSymbols = false;
        }
    }

    partial void OnSelectedAlgorithmChanged(AlgorithmModel? value)
    {
        // Don't load parameters during initial dialog setup
        if (_isInitializing)
            return;

        if (value != null)
        {
            _logger.LogInformation("Algorithm changed to: {AlgorithmId} - {AlgorithmName}", value.Id, value.Name);
            _ = LoadAlgorithmParametersAsync(value.Id);
        }
        else
        {
            _logger.LogWarning("Algorithm selection cleared");
            HasParameters = false;
            ParameterViewModels = new List<ParameterViewModel>();
        }
    }

    private async Task LoadAlgorithmParametersAsync(Guid algorithmId)
    {
        try
        {
            _logger.LogInformation("Loading parameters for algorithm: {AlgorithmId}", algorithmId);

            var parameters = await _algorithmRegistryService.GetParameterDefinitionsAsync(algorithmId);

            HasParameters = parameters.Any();

            // Initialize parameter values - leave blank for user to fill
            var viewModels = new List<ParameterViewModel>();

            foreach (var param in parameters.OrderBy(_ => _.DisplayOrder))
            {
                var vm = new ParameterViewModel
                {
                    Name = param.Name,
                    DisplayName = param.DisplayName,
                    Description = param.Description,
                    Type = (int)param.Type,
                    Value = GetEmptyValueForType((ParameterType)param.Type)
                };

                viewModels.Add(vm);
            }

            // If editing existing position, load saved parameter values
            if (_existingPosition != null && !string.IsNullOrWhiteSpace(_existingPosition.AlgorithmParameters))
            {
                try
                {
                    var savedParams = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(_existingPosition.AlgorithmParameters);
                    if (savedParams != null)
                    {
                        foreach (var vm in viewModels)
                        {
                            if (savedParams.TryGetValue(vm.Name, out var savedValue))
                            {
                                vm.Value = ConvertJsonElement(savedValue, (ParameterType)vm.Type);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse saved algorithm parameters");
                }
            }

            ParameterViewModels = viewModels;

            _logger.LogInformation("Loaded {Count} parameters", ParameterViewModels.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading algorithm parameters");
        }
    }

    private static object GetEmptyValueForType(ParameterType type)
    {
        return type switch
        {
            ParameterType.Integer => 0,
            ParameterType.Decimal => 0m,
            ParameterType.Boolean => false,
            ParameterType.Enum => 0,
            _ => string.Empty
        };
    }

    private static object ConvertJsonElement(JsonElement element, ParameterType type)
    {
        return type switch
        {
            ParameterType.Integer => element.ValueKind == JsonValueKind.Number ? element.GetInt32() : 0,
            ParameterType.Decimal => element.ValueKind == JsonValueKind.Number ? element.GetDecimal() : 0m,
            ParameterType.Boolean => element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False
                ? element.GetBoolean()
                : false,
            ParameterType.String => element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : string.Empty,
            ParameterType.Enum => element.ValueKind == JsonValueKind.Number ? element.GetInt32() : 0,
            _ => element.ToString() ?? string.Empty
        };
    }

    private void LoadExistingPosition()
    {
        if (_existingPosition == null) return;

        PositionName = _existingPosition.PositionName;
        AllocatedPercent = _existingPosition.AllocatedPercent.ToString("F2");
        MaxPositionSizePercent = _existingPosition.MaxPositionSizePercent.ToString("F2");
        ExchangeFees = (_existingPosition.ExchangeFees * 100).ToString("F3");
        AllowShortSelling = _existingPosition.AllowShortSelling;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateInput())
            return;

        try
        {
            _logger.LogInformation("Saving algorithm position");

            // Convert ParameterViewModels to Dictionary and serialize
            var parameterDict = ParameterViewModels.ToDictionary(_ => _.Name, _ => _.Value);
            var parametersJson = JsonSerializer.Serialize(parameterDict);

            var position = new AlgorithmPositionModel
            {
                Id = _existingPosition?.Id ?? Guid.NewGuid(),
                UserId = _existingPosition?.UserId ?? _userSessionService.CurrentUserId,
                PortfolioId = _portfolioId,
                AlgorithmId = SelectedAlgorithm!.Id,
                Symbol = SelectedSymbol!.Symbol,
                PositionName = PositionName,
                AllocatedPercent = _portfolioId.HasValue
                    ? decimal.Parse(AllocatedPercent)
                    : 0m,
                Status = Status.Inactive,
                MaxPositionSizePercent = decimal.Parse(MaxPositionSizePercent),
                ExchangeFees = decimal.Parse(ExchangeFees) / 100m,
                AllowShortSelling = AllowShortSelling,
                CurrentValue = 0m,
                AlgorithmParameters = parametersJson,
                CreatedAt = _existingPosition?.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _existingPosition?.CreatedBy ?? _userSessionService.CurrentUserId.ToString(),
                UpdatedBy = _userSessionService.CurrentUserId.ToString()
            };

            if (_existingPosition != null)
            {
                await _algorithmPositionService.UpdatePositionAsync(position);
                _logger.LogInformation("Position updated successfully");
            }
            else
            {
                await _algorithmPositionService.CreatePositionAsync(position);
                _logger.LogInformation("Position created successfully");
            }

            SaveCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving position");

            // Get the most relevant error message (innermost exception)
            var errorMessage = ex.InnerException?.Message ?? ex.Message;

            ValidationMessage = $"Error saving position: {errorMessage}";
            HasValidationError = true;
        }
    }

    private bool ValidateInput()
    {
        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(PositionName))
        {
            ValidationMessage = "Position name is required";
            HasValidationError = true;
            return false;
        }

        if (SelectedAlgorithm == null)
        {
            ValidationMessage = "Algorithm selection is required";
            HasValidationError = true;
            return false;
        }

        if (SelectedSymbol == null)
        {
            ValidationMessage = "Symbol selection is required";
            HasValidationError = true;
            return false;
        }

        // Validate algorithm parameters
        if (ParameterViewModels.Any())
        {
            foreach (var param in ParameterViewModels)
            {
                // Check for empty/invalid values based on type
                if (param.Type == 1 || param.Type == 2) // Integer or Decimal
                {

                    if (param.Value == null ||
                        string.IsNullOrWhiteSpace(param.DisplayValue) ||
                        param.DisplayValue == "0")
                    {
                        ValidationMessage = $"{param.DisplayName} is required and must be greater than 0";
                        HasValidationError = true;
                        return false;
                    }

                    // Additional validation for specific parameters
                    if (param.Type == 1) // Integer
                    {
                        if (!int.TryParse(param.DisplayValue, out var intValue) || intValue <= 0)
                        {
                            ValidationMessage = $"{param.DisplayName} must be a valid positive number";
                            HasValidationError = true;
                            return false;
                        }
                    }
                    else if (param.Type == 2) // Decimal
                    {
                        if (!decimal.TryParse(param.DisplayValue, out var decValue) || decValue <= 0)
                        {
                            ValidationMessage = $"{param.DisplayName} must be a valid positive number";
                            HasValidationError = true;
                            return false;
                        }
                    }
                }
            }
        }

        if (_portfolioId.HasValue)
        {
            if (!decimal.TryParse(AllocatedPercent, out var allocatedPct) || allocatedPct <= 0 || allocatedPct > 100)
            {
                ValidationMessage = "Allocated percent must be between 0 and 100";
                HasValidationError = true;
                return false;
            }
        }

        if (!decimal.TryParse(MaxPositionSizePercent, out var maxPosSizePct) || maxPosSizePct <= 0 || maxPosSizePct > 100)
        {
            ValidationMessage = "Max position size percent must be between 0 and 100";
            HasValidationError = true;
            return false;
        }

        if (!decimal.TryParse(ExchangeFees, out var fees) || fees < 0)
        {
            ValidationMessage = "Exchange fees must be 0 or greater";
            HasValidationError = true;
            return false;
        }

        return true;
    }
}