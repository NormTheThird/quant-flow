namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for Backtest Configuration dialog
/// </summary>
public partial class BacktestConfigurationDialogViewModel : ObservableObject
{
    private readonly ILogger<BacktestConfigurationDialogViewModel> _logger;
    private readonly IBacktestService _backtestService;
    private readonly IAlgorithmRegistryService _algorithmRegistryService;
    private readonly Guid _currentUserId;
    private readonly AlgorithmPositionModel _position;

    [ObservableProperty]
    private string _dialogTitle = "Configure Backtest";

    [ObservableProperty]
    private string _backtestName = string.Empty;

    [ObservableProperty]
    private string _algorithmName = "Loading...";

    [ObservableProperty]
    private DateTime? _startDate = DateTime.UtcNow.AddMonths(-3);

    [ObservableProperty]
    private DateTime? _endDate = DateTime.UtcNow;

    [ObservableProperty]
    private List<string> _availableTimeframes = [.. Enum.GetNames<Timeframe>()
        .Where(_ => _ != "Unknown")
        .Select(_ => ConvertTimeframeToDisplay(Enum.Parse<Timeframe>(_)))];

    [ObservableProperty]
    private string _selectedTimeframe = "1h";

    [ObservableProperty]
    private decimal _initialBalance = 10000m;

    [ObservableProperty]
    private decimal _commissionRate = 0.1m;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

    public event EventHandler<bool>? BacktestCompleted;

    public BacktestConfigurationDialogViewModel(
        ILogger<BacktestConfigurationDialogViewModel> logger,
        IBacktestService backtestService,
        IAlgorithmRegistryService algorithmRegistryService,
        IUserSessionService userSessionService,
        AlgorithmPositionModel position)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _backtestService = backtestService ?? throw new ArgumentNullException(nameof(backtestService));
        _algorithmRegistryService = algorithmRegistryService ?? throw new ArgumentNullException(nameof(algorithmRegistryService));
        _position = position ?? throw new ArgumentNullException(nameof(position));

        _currentUserId = userSessionService.CurrentUserId;
        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        BacktestName = $"{_position.PositionName} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

        _ = LoadAlgorithmNameAsync();
    }

    private async Task LoadAlgorithmNameAsync()
    {
        try
        {
            var algorithms = await _algorithmRegistryService.GetAllAvailableAlgorithmsAsync(_currentUserId);
            var algorithm = algorithms.FirstOrDefault(_ => _.Id == _position.AlgorithmId);

            if (algorithm != null)
            {
                AlgorithmName = string.IsNullOrEmpty(algorithm.Abbreviation)
                    ? algorithm.Name
                    : $"{algorithm.Name} ({algorithm.Abbreviation})";
            }
            else
            {
                AlgorithmName = "Unknown Algorithm";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading algorithm name");
            AlgorithmName = "Error loading algorithm";
        }
    }

    [RelayCommand]
    private async Task RunBacktest()
    {
        if (!ValidateInput())
            return;

        try
        {
            _logger.LogInformation("Running backtest for position: {PositionId}", _position.Id);

            var backtestRun = new BacktestRunModel
            {
                Id = Guid.NewGuid(),
                Name = BacktestName,
                AlgorithmId = _position.AlgorithmId,
                UserId = _currentUserId,
                Symbol = _position.Symbol,
                Exchange = Exchange.Kraken,
                Timeframe = ParseTimeframe(SelectedTimeframe),
                BacktestStartDate = StartDate!.Value,
                BacktestEndDate = EndDate!.Value,
                Status = BacktestStatus.Pending,
                InitialBalance = InitialBalance,
                CommissionRate = CommissionRate / 100m,
                AlgorithmParameters = "{}"
            };

            await _backtestService.CreateBacktestRunAsync(backtestRun);
            _logger.LogInformation("Backtest created successfully");

            BacktestCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backtest");
            ValidationMessage = $"Error creating backtest: {ex.Message}";
            HasValidationError = true;
        }
    }

    private bool ValidateInput()
    {
        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(BacktestName))
        {
            ValidationMessage = "Backtest name is required";
            HasValidationError = true;
            return false;
        }

        if (!StartDate.HasValue)
        {
            ValidationMessage = "Start date is required";
            HasValidationError = true;
            return false;
        }

        if (!EndDate.HasValue)
        {
            ValidationMessage = "End date is required";
            HasValidationError = true;
            return false;
        }

        if (EndDate <= StartDate)
        {
            ValidationMessage = "End date must be after start date";
            HasValidationError = true;
            return false;
        }

        if (InitialBalance <= 0)
        {
            ValidationMessage = "Initial balance must be greater than 0";
            HasValidationError = true;
            return false;
        }

        if (CommissionRate < 0 || CommissionRate > 100)
        {
            ValidationMessage = "Commission rate must be between 0 and 100";
            HasValidationError = true;
            return false;
        }

        return true;
    }

    private static Timeframe ParseTimeframe(string timeframe) => timeframe switch
    {
        "1m" => Timeframe.OneMinute,
        "5m" => Timeframe.FiveMinutes,
        "15m" => Timeframe.FifteenMinutes,
        "1h" => Timeframe.OneHour,
        "4h" => Timeframe.FourHours,
        "1d" => Timeframe.OneDay,
        _ => Timeframe.OneHour
    };

    private static string ConvertTimeframeToDisplay(Timeframe timeframe) => timeframe switch
    {
        Timeframe.OneMinute => "1m",
        Timeframe.FiveMinutes => "5m",
        Timeframe.FifteenMinutes => "15m",
        Timeframe.OneHour => "1h",
        Timeframe.FourHours => "4h",
        Timeframe.OneDay => "1d",
        _ => "1h"
    };
}