using QuantFlow.Common.Models;

namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for Add/Edit Algorithm dialog
/// </summary>
public partial class AlgorithmDialogViewModel : ObservableObject
{
    private readonly ILogger<AlgorithmDialogViewModel> _logger;
    private readonly IAlgorithmService _algorithmService;
    private readonly Guid _currentUserId;
    private readonly AlgorithmModel? _existingAlgorithm;

    [ObservableProperty]
    private string _dialogTitle = "Create Algorithm";

    [ObservableProperty]
    private string _saveButtonText = "Create";

    [ObservableProperty]
    private string _algorithmName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private List<string> _availableLanguages = ["csharp", "python"];

    [ObservableProperty]
    private string _selectedProgrammingLanguage = "csharp";

    [ObservableProperty]
    private List<string> _availableStatuses;

    [ObservableProperty]
    private string _selectedStatus = "Draft";

    [ObservableProperty]
    private string _algorithmCode = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

    public event EventHandler<bool>? SaveCompleted;

    public AlgorithmDialogViewModel(
        ILogger<AlgorithmDialogViewModel> logger,
        IAlgorithmService algorithmService,
        IUserSessionService userSessionService,
        AlgorithmModel? existingAlgorithm = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));

        _currentUserId = userSessionService.CurrentUserId;
        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        _existingAlgorithm = existingAlgorithm;

        // Initialize available statuses from enum
        _availableStatuses = Enum.GetNames(typeof(AlgorithmStatus)).ToList();

        if (_existingAlgorithm != null)
        {
            DialogTitle = "Edit Algorithm";
            SaveButtonText = "Save";
            LoadExistingAlgorithm();
        }
    }

    private void LoadExistingAlgorithm()
    {
        if (_existingAlgorithm == null) return;

        AlgorithmName = _existingAlgorithm.Name;
        Description = _existingAlgorithm.Description;
        Version = _existingAlgorithm.Version;
        SelectedProgrammingLanguage = _existingAlgorithm.ProgrammingLanguage;
        SelectedStatus = _existingAlgorithm.Status.ToString();
        AlgorithmCode = _existingAlgorithm.Code;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateInput())
            return;

        try
        {
            _logger.LogInformation("Saving algorithm");

            var algorithm = new AlgorithmModel
            {
                Id = _existingAlgorithm?.Id ?? Guid.NewGuid(),
                UserId = _currentUserId,
                Name = AlgorithmName,
                Description = Description,
                Code = AlgorithmCode,
                ProgrammingLanguage = SelectedProgrammingLanguage,
                Version = Version,
                Status = Enum.Parse<AlgorithmStatus>(SelectedStatus),
                Tags = [],
                Parameters = new Dictionary<string, object>(),
                RiskSettings = new Dictionary<string, object>(),     
                PerformanceMetrics = new Dictionary<string, object>(),
                IsPublic = false,
                IsTemplate = false,
                TemplateCategory = null
            };

            if (_existingAlgorithm == null)
            {
                await _algorithmService.CreateAlgorithmAsync(algorithm);
                _logger.LogInformation("Algorithm created successfully");
            }
            else
            {
                await _algorithmService.UpdateAlgorithmAsync(algorithm);
                _logger.LogInformation("Algorithm updated successfully");
            }

            SaveCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving algorithm");
            ValidationMessage = $"Error saving algorithm: {ex.Message}";
            HasValidationError = true;
        }
    }

    private bool ValidateInput()
    {
        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(AlgorithmName))
        {
            ValidationMessage = "Algorithm name is required";
            HasValidationError = true;
            return false;
        }

        if (string.IsNullOrWhiteSpace(AlgorithmCode))
        {
            ValidationMessage = "Algorithm code is required";
            HasValidationError = true;
            return false;
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            ValidationMessage = "Version is required";
            HasValidationError = true;
            return false;
        }

        return true;
    }
}