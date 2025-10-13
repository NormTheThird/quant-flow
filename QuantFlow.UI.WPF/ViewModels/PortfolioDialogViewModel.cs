namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for Create/Edit Portfolio dialog
/// </summary>
public partial class PortfolioDialogViewModel : ObservableObject
{
    private readonly ILogger<PortfolioDialogViewModel> _logger;
    private readonly IPortfolioService _portfolioService;
    private readonly Guid _currentUserId;
    private readonly Guid? _portfolioId;

    [ObservableProperty]
    private DialogMode _dialogMode = DialogMode.Unknown;

    [ObservableProperty]
    private string _dialogTitle = "Create Portfolio";

    [ObservableProperty]
    private string _saveButtonText = "Create";

    [ObservableProperty]
    private string _portfolioName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _initialBalance = "10000";

    [ObservableProperty]
    private bool _isTestMode = true;

    [ObservableProperty]
    private bool _isExchangeConnected = false;

    [ObservableProperty]
    private List<string> _availableExchanges = ["Kraken", "KuCoin"];

    [ObservableProperty]
    private string? _selectedExchange = "Kraken";

    [ObservableProperty]
    private List<string> _availableCurrencies = ["USDT", "USDC"];

    [ObservableProperty]
    private string _selectedBaseCurrency = "USDT";

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError = false;

    public event EventHandler<bool>? SaveCompleted;

    public PortfolioDialogViewModel(ILogger<PortfolioDialogViewModel> logger, IPortfolioService portfolioService, IUserSessionService userSessionService,
                                    PortfolioModel? existingPortfolio = null)
    {
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _currentUserId = userSessionService.CurrentUserId;

        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        if (existingPortfolio != null)
        {
            _portfolioId = existingPortfolio.Id;
            DialogMode = DialogMode.Edit;
            DialogTitle = "Edit Portfolio";
            SaveButtonText = "Save";
            LoadExistingPortfolio(existingPortfolio);
        }
        else
        {
            DialogMode = DialogMode.Create;
            DialogTitle = "Create Portfolio";
            SaveButtonText = "Create";
        }
    }

    public string InitialBalanceFormatted => decimal.TryParse(InitialBalance, out var balance) ? $"${balance:N2}" : InitialBalance;

    private void LoadExistingPortfolio(PortfolioModel portfolio)
    {
        PortfolioName = portfolio.Name;
        Description = portfolio.Description;
        InitialBalance = portfolio.InitialBalance.ToString("F2");

        IsTestMode = portfolio.Mode == PortfolioMode.TestMode;
        IsExchangeConnected = portfolio.Mode == PortfolioMode.ExchangeConnected;

        SelectedExchange = portfolio.Exchange.ToString();
        SelectedBaseCurrency = portfolio.BaseCurrency.ToString();
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateInput())
            return;

        try
        {
            var portfolio = new PortfolioModel
            {
                Id = _portfolioId ?? Guid.NewGuid(),
                Name = PortfolioName,
                Description = Description,
                InitialBalance = decimal.Parse(InitialBalance),
                CurrentBalance = (DialogMode == DialogMode.Edit) ? 0 : decimal.Parse(InitialBalance),
                Status = Status.Inactive,
                Mode = IsTestMode ? PortfolioMode.TestMode : PortfolioMode.ExchangeConnected,
                Exchange = Enum.Parse<Exchange>(SelectedExchange ?? "Kraken"),
                BaseCurrency = Enum.Parse<BaseCurrency>(SelectedBaseCurrency),
                UserExchangeDetailsId = null, // TODO: Get from selected exchange details when in ExchangeConnected mode
                UserId = _currentUserId,
                CreatedBy = "CurrentUser", // TODO: Get from user session
                UpdatedBy = "CurrentUser"
            };

            if (DialogMode == DialogMode.Edit)
            {
                await _portfolioService.UpdatePortfolioAsync(portfolio);
                _logger.LogInformation("Portfolio updated: {PortfolioId}", portfolio.Id);
            }
            else
            {
                await _portfolioService.CreatePortfolioAsync(portfolio);
                _logger.LogInformation("Portfolio created: {PortfolioId}", portfolio.Id);
            }

            SaveCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving portfolio");

            var innerException = ex;
            while (innerException.InnerException != null)
            {
                innerException = innerException.InnerException;
            }

            ValidationMessage = $"Error: {innerException.Message}";
            HasValidationError = true;
        }
    }

    private bool ValidateInput()
    {
        ValidationMessage = string.Empty;
        HasValidationError = false;

        if (string.IsNullOrWhiteSpace(PortfolioName))
        {
            ValidationMessage = "Portfolio name is required";
            HasValidationError = true;
            return false;
        }

        if (!decimal.TryParse(InitialBalance, out var balance) || balance <= 0)
        {
            ValidationMessage = "Initial balance must be a positive number";
            HasValidationError = true;
            return false;
        }

        if (string.IsNullOrEmpty(SelectedExchange))
        {
            ValidationMessage = "Please select an exchange";
            HasValidationError = true;
            return false;
        }

        if (string.IsNullOrEmpty(SelectedBaseCurrency))
        {
            ValidationMessage = "Please select a base currency";
            HasValidationError = true;
            return false;
        }

        return true;
    }

    partial void OnIsTestModeChanged(bool value)
    {
        if (value)
        {
            IsExchangeConnected = false;
        }
    }

    partial void OnIsExchangeConnectedChanged(bool value)
    {
        if (value)
        {
            IsTestMode = false;
        }
    }
}