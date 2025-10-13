namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for Add/Edit Algorithm Position dialog
/// </summary>
public partial class AlgorithmPositionDialogViewModel : ObservableObject
{
    private readonly ILogger<AlgorithmPositionDialogViewModel> _logger;
    private readonly IAlgorithmPositionService _algorithmPositionService;
    private readonly Guid _portfolioId;
    private readonly AlgorithmPositionModel? _existingPosition;

    [ObservableProperty]
    private string _dialogTitle = "Add Position";

    [ObservableProperty]
    private string _positionName = string.Empty;

    [ObservableProperty]
    private string _allocatedPercent = "10.0";

    [ObservableProperty]
    private string _maxPositionSizePercent = "10.0";

    [ObservableProperty]
    private string _exchangeFees = "0.1";

    [ObservableProperty]
    private bool _allowShortSelling;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

    public event EventHandler<bool>? SaveCompleted;

    public AlgorithmPositionDialogViewModel(
        ILogger<AlgorithmPositionDialogViewModel> logger,
        IAlgorithmPositionService algorithmPositionService,
        Guid portfolioId,
        AlgorithmPositionModel? existingPosition = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));
        _portfolioId = portfolioId;
        _existingPosition = existingPosition;

        if (_existingPosition != null)
        {
            DialogTitle = "Edit Position";
            LoadExistingPosition();
        }
    }

    private void LoadExistingPosition()
    {
        if (_existingPosition == null) return;

        PositionName = _existingPosition.PositionName;
        AllocatedPercent = _existingPosition.AllocatedPercent.ToString("F2");
        MaxPositionSizePercent = _existingPosition.MaxPositionSizePercent.ToString("F2");
        ExchangeFees = _existingPosition.ExchangeFees.ToString("F3");
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
                PortfolioId = _portfolioId,
                AlgorithmId = Guid.NewGuid(), // TODO: Will be selected from dropdown later
                PositionName = PositionName,
                AllocatedPercent = decimal.Parse(AllocatedPercent),
                MaxPositionSizePercent = decimal.Parse(MaxPositionSizePercent),
                ExchangeFees = decimal.Parse(ExchangeFees) / 100m, // Convert from percentage to decimal
                AllowShortSelling = AllowShortSelling,
                Status = Status.Inactive
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

        if (!decimal.TryParse(AllocatedPercent, out var allocatedPercent) || allocatedPercent <= 0 || allocatedPercent > 100)
        {
            ValidationMessage = "Allocated % must be between 0 and 100";
            HasValidationError = true;
            return false;
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