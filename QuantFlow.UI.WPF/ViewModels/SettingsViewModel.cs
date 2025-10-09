namespace QuantFlow.UI.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly ISymbolService _symbolService;
    private readonly IUserPreferencesRepository _userPreferencesRepository;
    private readonly IUserSessionService _userSessionService;

    [ObservableProperty]
    private List<SymbolModel> _availableSymbols = new();

    [ObservableProperty]
    private List<string> _selectedSymbols = new();

    [ObservableProperty]
    private SymbolModel? _symbolToAdd;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasChanges = false;

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        ISymbolService symbolService,
        IUserPreferencesRepository userPreferencesRepository,
        IUserSessionService userSessionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _symbolService = symbolService ?? throw new ArgumentNullException(nameof(symbolService));
        _userPreferencesRepository = userPreferencesRepository ?? throw new ArgumentNullException(nameof(userPreferencesRepository));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));

        _logger.LogInformation("SettingsViewModel initialized");

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            await LoadAvailableSymbolsAsync();
            await LoadUserSymbolsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing settings");
            StatusMessage = "Error loading settings";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAvailableSymbolsAsync()
    {
        try
        {
            var symbols = await _symbolService.GetActiveAsync();
            AvailableSymbols = symbols.ToList();
            _logger.LogInformation("Loaded {Count} available symbols", AvailableSymbols.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available symbols");
            AvailableSymbols = new List<SymbolModel>();
        }
    }

    private async Task LoadUserSymbolsAsync()
    {
        try
        {
            var userId = _userSessionService.CurrentUserId;
            var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId);

            if (preferences == null)
            {
                SelectedSymbols = new List<string>();
                return;
            }

            var marketOverviewCards = preferences.MarketOverviewCards as Dictionary<string, object>;
            if (marketOverviewCards == null || !marketOverviewCards.ContainsKey("Kraken"))
            {
                SelectedSymbols = new List<string>();
                return;
            }

            var krakenSymbols = marketOverviewCards["Kraken"] as List<object>;
            SelectedSymbols = krakenSymbols?.Select(s => s.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            _logger.LogInformation("Loaded {Count} user symbols", SelectedSymbols.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user symbols");
            SelectedSymbols = new List<string>();
        }
    }

    [RelayCommand]
    private void AddSymbol()
    {
        if (SymbolToAdd == null)
        {
            StatusMessage = "Please select a symbol to add";
            return;
        }

        if (SelectedSymbols.Count >= 10)
        {
            StatusMessage = "Maximum 10 symbols allowed";
            return;
        }

        if (SelectedSymbols.Contains(SymbolToAdd.Symbol))
        {
            StatusMessage = $"{SymbolToAdd.Symbol} is already in your list";
            return;
        }

        SelectedSymbols = new List<string>(SelectedSymbols) { SymbolToAdd.Symbol };
        HasChanges = true;
        StatusMessage = $"Added {SymbolToAdd.Symbol}";
        SymbolToAdd = null;

        _logger.LogInformation("Added symbol, count now: {Count}", SelectedSymbols.Count);
    }

    [RelayCommand]
    private void RemoveSymbol(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return;

        SelectedSymbols = SelectedSymbols.Where(s => s != symbol).ToList();
        HasChanges = true;
        StatusMessage = $"Removed {symbol}";

        _logger.LogInformation("Removed symbol {Symbol}, count now: {Count}", symbol, SelectedSymbols.Count);
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        IsLoading = true;
        StatusMessage = "Saving...";

        try
        {
            var userId = _userSessionService.CurrentUserId;
            var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId);

            if (preferences == null)
            {
                StatusMessage = "Error: User preferences not found";
                return;
            }

            var marketOverviewCards = preferences.MarketOverviewCards as Dictionary<string, object> ?? new Dictionary<string, object>();
            marketOverviewCards["Kraken"] = SelectedSymbols;
            preferences.MarketOverviewCards = marketOverviewCards;

            await _userPreferencesRepository.UpdateAsync(preferences);

            HasChanges = false;
            StatusMessage = "Changes saved successfully!";
            _logger.LogInformation("Saved {Count} symbols for user", SelectedSymbols.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            StatusMessage = "Error saving changes";
        }
        finally
        {
            IsLoading = false;
        }
    }
}