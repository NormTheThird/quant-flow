namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for Add/Edit Algorithm Position dialog
/// Supports both standalone positions and portfolio-assigned positions
/// </summary>
public partial class AlgorithmPositionDialogViewModel : ObservableObject
{
    private readonly ILogger<AlgorithmPositionDialogViewModel> _logger;
    private readonly IAlgorithmPositionService _algorithmPositionService;
    private readonly IAlgorithmService _algorithmService;
    private readonly ISymbolService _symbolService;
    private readonly IUserSessionService _userSessionService;
    private readonly Guid? _portfolioId; // Now nullable
    private readonly AlgorithmPositionModel? _existingPosition;

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
    private bool _showAllocatedPercent; // Show only when assigned to portfolio

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

    [ObservableProperty]
    private bool _isLoadingAlgorithms = true;

    [ObservableProperty]
    private bool _isLoadingSymbols = true;

    public event EventHandler<bool>? SaveCompleted;

    public AlgorithmPositionDialogViewModel(
        ILogger<AlgorithmPositionDialogViewModel> logger,
        IAlgorithmPositionService algorithmPositionService,
        IAlgorithmService algorithmService,
        ISymbolService symbolService,
        IUserSessionService userSessionService,
        Guid? portfolioId = null, // Now optional
        AlgorithmPositionModel? existingPosition = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _symbolService = symbolService ?? throw new ArgumentNullException(nameof(symbolService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _portfolioId = portfolioId;
        _existingPosition = existingPosition;

        // Show allocated percent only if assigning to portfolio
        ShowAllocatedPercent = _portfolioId.HasValue;

        if (_existingPosition != null)
        {
            DialogTitle = "Edit Position";
            LoadExistingPosition();
        }
        else
        {
            DialogTitle = _portfolioId.HasValue ? "Add Position to Portfolio" : "Create Position";
        }

        var currentUserId = userSessionService.CurrentUserId;
        _ = LoadDataAsync(currentUserId);
    }

    private async Task LoadDataAsync(Guid userId)
    {
        await Task.WhenAll(
            LoadAlgorithmsAsync(userId),
            LoadSymbolsAsync()
        );
    }

    private async Task LoadAlgorithmsAsync(Guid userId)
    {
        try
        {
            IsLoadingAlgorithms = true;
            _logger.LogInformation("Loading algorithms for user: {UserId}", userId);

            var algorithms = await _algorithmService.GetAlgorithmsByUserIdAsync(userId);
            AvailableAlgorithms = algorithms
                .Where(a => a.Status == AlgorithmStatus.Active || a.Status == AlgorithmStatus.Testing)
                .OrderBy(a => a.Name)
                .ToList();

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

            var position = new AlgorithmPositionModel
            {
                Id = _existingPosition?.Id ?? Guid.NewGuid(),
                UserId = _existingPosition?.UserId ?? _userSessionService.CurrentUserId,
                PortfolioId = _portfolioId, // Can be null for standalone positions
                AlgorithmId = SelectedAlgorithm!.Id,
                Symbol = SelectedSymbol!.Symbol,
                PositionName = PositionName,
                AllocatedPercent = _portfolioId.HasValue ? decimal.Parse(AllocatedPercent) : 0,
                MaxPositionSizePercent = decimal.Parse(MaxPositionSizePercent),
                ExchangeFees = decimal.Parse(ExchangeFees) / 100m,
                AllowShortSelling = AllowShortSelling,
                Status = Status.Inactive,
                CurrentValue = 0
            };

            if (_existingPosition == null)
            {
                await _algorithmPositionService.CreatePositionAsync(position);
                _logger.LogInformation("Position created successfully");
            }
            else
            {
                await _algorithmPositionService.UpdatePositionAsync(position);
                _logger.LogInformation("Position updated successfully");
            }

            SaveCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving position");
            ValidationMessage = $"Error saving position: {ex.Message}";
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
            ValidationMessage = "Please select an algorithm";
            HasValidationError = true;
            return false;
        }

        if (SelectedSymbol == null)
        {
            ValidationMessage = "Please select a symbol";
            HasValidationError = true;
            return false;
        }

        // Only validate allocated percent if assigning to portfolio
        if (_portfolioId.HasValue)
        {
            if (!decimal.TryParse(AllocatedPercent, out var allocatedPercent) || allocatedPercent <= 0 || allocatedPercent > 100)
            {
                ValidationMessage = "Allocated % must be between 0 and 100";
                HasValidationError = true;
                return false;
            }
        }

        if (!decimal.TryParse(MaxPositionSizePercent, out var maxPositionSize) || maxPositionSize <= 0 || maxPositionSize > 100)
        {
            ValidationMessage = "Max Position Size % must be between 0 and 100";
            HasValidationError = true;
            return false;
        }

        if (!decimal.TryParse(ExchangeFees, out var fees) || fees < 0)
        {
            ValidationMessage = "Exchange Fees must be 0 or greater";
            HasValidationError = true;
            return false;
        }

        return true;
    }
}