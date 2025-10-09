namespace QuantFlow.UI.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private object? _currentView;

    public event EventHandler? LogoutRequested;

    public MainWindowViewModel(IServiceProvider serviceProvider, ILogger<MainWindowViewModel> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set default view to Dashboard
        NavigateToDashboard();
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        _logger.LogInformation("Navigating to Dashboard");

        // Create a scope for scoped services
        var scope = _serviceProvider.CreateScope();
        var dashboardViewModel = scope.ServiceProvider.GetRequiredService<DashboardViewModel>();
        var dashboardView = scope.ServiceProvider.GetRequiredService<DashboardView>();
        dashboardView.DataContext = dashboardViewModel;
        CurrentView = dashboardView;
    }

    [RelayCommand]
    public void NavigateToSettings()
    {
        _logger.LogInformation("Navigating to Settings");

        // Create a scope for scoped services
        var scope = _serviceProvider.CreateScope();
        var settingsViewModel = scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
        var settingsView = scope.ServiceProvider.GetRequiredService<SettingsView>();
        settingsView.DataContext = settingsViewModel;
        CurrentView = settingsView;
    }

    [RelayCommand]
    public void NavigateToPortfolios()
    {
        _logger.LogInformation("Navigating to Portfolios");
        // TODO: Create PortfoliosViewModel and PortfoliosView
    }

    [RelayCommand]
    public void NavigateToAlgorithms()
    {
        _logger.LogInformation("Navigating to Algorithms");
        // TODO: Create AlgorithmsViewModel and AlgorithmsView
    }

    [RelayCommand]
    public void NavigateToBacktests()
    {
        _logger.LogInformation("Navigating to Backtests");
        // TODO: Create BacktestsViewModel and BacktestsView
    }

    [RelayCommand]
    public void Logout()
    {
        _logger.LogInformation("Logout requested from MainWindowViewModel");
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}