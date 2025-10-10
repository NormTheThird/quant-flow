namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for managing exchange API settings
/// </summary>
public partial class ExchangeSettingsViewModel : ObservableObject
{
    private readonly IUserExchangeDetailsService _exchangeDetailsService;
    private readonly IUserSessionService _userSessionService;
    private readonly ILogger<ExchangeSettingsViewModel> _logger;

    [ObservableProperty]
    private string _selectedExchange = "Kraken";

    [ObservableProperty]
    private string _keyName = string.Empty;

    [ObservableProperty]
    private string _keyValue = string.Empty;

    [ObservableProperty]
    private bool _shouldEncrypt = false;

    [ObservableProperty]
    private ObservableCollection<ExchangeDetailItemViewModel> _exchangeDetails = [];

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public ExchangeSettingsViewModel(ILogger<ExchangeSettingsViewModel> logger, IUserExchangeDetailsService exchangeDetailsService,
                                      IUserSessionService userSessionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exchangeDetailsService = exchangeDetailsService ?? throw new ArgumentNullException(nameof(exchangeDetailsService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));

        _ = LoadExchangeDetailsAsync();
    }

    [RelayCommand]
    private async Task AddExchangeDetail()
    {
        if (string.IsNullOrWhiteSpace(KeyName) || string.IsNullOrWhiteSpace(KeyValue))
        {
            StatusMessage = "Key Name and Key Value are required";
            return;
        }

        try
        {
            var model = new UserExchangeDetailsModel
            {
                UserId = _userSessionService.CurrentUserId,
                Exchange = SelectedExchange,
                KeyName = KeyName,
                KeyValue = KeyValue,
                IsEncrypted = ShouldEncrypt,
                IsActive = true,
                CreatedBy = "CurrentUser",
                UpdatedBy = "CurrentUser"
            };

            await _exchangeDetailsService.CreateAsync(model);

            // Clear form
            KeyName = string.Empty;
            KeyValue = string.Empty;
            ShouldEncrypt = false;

            StatusMessage = "Exchange detail added successfully";
            await LoadExchangeDetailsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exchange detail");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteExchangeDetail(ExchangeDetailItemViewModel item)
    {
        try
        {
            await _exchangeDetailsService.DeleteAsync(item.Id);
            StatusMessage = "Exchange detail deleted successfully";
            await LoadExchangeDetailsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exchange detail");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadExchangeDetailsAsync()
    {
        try
        {
            var userId = _userSessionService.CurrentUserId;
            var details = await _exchangeDetailsService.GetByUserAndExchangeAsync(userId, SelectedExchange);

            ExchangeDetails = new ObservableCollection<ExchangeDetailItemViewModel>(
                details.Select(d => new ExchangeDetailItemViewModel(d)));

            _logger.LogInformation("Loaded {Count} exchange details for {Exchange}", ExchangeDetails.Count, SelectedExchange);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exchange details");
            ExchangeDetails = [];
        }
    }

    partial void OnSelectedExchangeChanged(string value)
    {
        _ = LoadExchangeDetailsAsync();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        SelectedExchange = value == 0 ? "Kraken" : "KuCoin";
    }
}