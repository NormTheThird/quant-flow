namespace QuantFlow.UI.WPF.Windows;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private MainWindowViewModel? _mainViewModel;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    public void InitializeTopBar(Guid userId, string username)
    {
        // Initialize MainWindowViewModel
        var logger = _serviceProvider.GetRequiredService<ILogger<MainWindowViewModel>>();
        _mainViewModel = new MainWindowViewModel(_serviceProvider, logger);
        _mainViewModel.LogoutRequested += OnLogoutRequested;
        DataContext = _mainViewModel;

        // Initialize TopBarViewModel
        var topBarLogger = _serviceProvider.GetRequiredService<ILogger<TopBarViewModel>>();
        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        var credentialStorage = _serviceProvider.GetRequiredService<ICredentialStorageService>();
        var tokenStorage = _serviceProvider.GetRequiredService<ITokenStorageService>();

        var topBarViewModel = new TopBarViewModel(
            topBarLogger,
            themeService,
            credentialStorage,
            tokenStorage,
            userId,
            username);

        topBarViewModel.LogoutRequested += (s, e) => _mainViewModel?.Logout();
        topBarViewModel.SettingsRequested += (s, e) => _mainViewModel?.NavigateToSettings();

        TopBar.DataContext = topBarViewModel;
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        this.Close();
    }
}