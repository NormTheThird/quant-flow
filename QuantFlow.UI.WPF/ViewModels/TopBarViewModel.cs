namespace QuantFlow.UI.WPF.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    private readonly ILogger<TopBarViewModel> _logger;
    private readonly IThemeService _themeService;
    private readonly ICredentialStorageService _credentialStorage;
    private readonly ITokenStorageService _tokenStorage;
    private readonly Guid _userId;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private List<string> _availableThemes = new();

    [ObservableProperty]
    private string _selectedTheme = string.Empty;

    [ObservableProperty]
    private bool _isUserMenuOpen;

    public event EventHandler? LogoutRequested;
    public event EventHandler? SettingsRequested;

    public TopBarViewModel(
        ILogger<TopBarViewModel> logger,
        IThemeService themeService,
        ICredentialStorageService credentialStorage,
        ITokenStorageService tokenStorage,
        Guid userId,
        string username)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
        _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        _userId = userId;
        _username = username;

        LoadAvailableThemes();
        LoadThemeAsync();
    }

    private void LoadAvailableThemes()
    {
        try
        {
            AvailableThemes = _themeService.GetAvailableThemeNames();
            _logger.LogInformation("Loaded {Count} available themes", AvailableThemes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load available themes");
            AvailableThemes = new List<string> { "DevExpress Light", "Clean Light", "Midnight Pro" };
        }
    }

    private async void LoadThemeAsync()
    {
        try
        {
            var theme = await _themeService.GetCurrentThemeAsync(_userId);
            SelectedTheme = theme;
            _themeService.ApplyTheme(theme);
            _logger.LogInformation("Loaded and applied theme: {Theme}", theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load theme for user {UserId}", _userId);
            SelectedTheme = "DevExpress Light";
            _themeService.ApplyTheme(SelectedTheme);
        }
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        _logger.LogInformation("Theme changed to: {Theme}", value);

        Task.Run(async () =>
        {
            try
            {
                await _themeService.SetThemeAsync(_userId, value);
                _logger.LogInformation("Theme applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set theme to {Theme}", value);
            }
        });
    }

    [RelayCommand]
    private void ToggleUserMenu()
    {
        IsUserMenuOpen = !IsUserMenuOpen;
    }

    [RelayCommand]
    private void Settings()
    {
        try
        {
            IsUserMenuOpen = false;
            _logger.LogInformation("Settings clicked for user {Username}", Username);
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening settings for user {Username}", Username);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        try
        {
            IsUserMenuOpen = false;
            _credentialStorage.ClearCredentials();
            _tokenStorage.ClearTokens();
            _logger.LogInformation("User {Username} logged out", Username);
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {Username}", Username);
        }
    }
}