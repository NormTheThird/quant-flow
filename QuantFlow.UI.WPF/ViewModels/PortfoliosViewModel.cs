namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for the Portfolios view
/// </summary>
public partial class PortfoliosViewModel : ObservableObject
{
    private readonly ILogger<PortfoliosViewModel> _logger;
    private readonly IPortfolioService _portfolioService;
    private readonly Guid _currentUserId;

    [ObservableProperty]
    private ObservableCollection<PortfolioItemViewModel> _portfolios = [];

    [ObservableProperty]
    private bool _hasNoPortfolios;

    public PortfoliosViewModel(ILogger<PortfoliosViewModel> logger, IPortfolioService portfolioService, IUserSessionService userSessionService)
    {
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _currentUserId = userSessionService.CurrentUserId;
        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        _ = LoadPortfoliosAsync();
    }

    private async Task LoadPortfoliosAsync()
    {
        try
        {
            _logger.LogInformation("Loading portfolios for user: {UserId}", _currentUserId);

            var portfolios = await _portfolioService.GetPortfoliosByUserIdAsync(_currentUserId);

            Portfolios = new ObservableCollection<PortfolioItemViewModel>(portfolios.Select(p => new PortfolioItemViewModel(p))
                                                                                    .OrderBy(_ => _.ModeText));

            HasNoPortfolios = Portfolios.Count == 0;

            _logger.LogInformation("Loaded {Count} portfolios", Portfolios.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolios");
            MessageBox.Show($"Error loading portfolios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CreatePortfolio()
    {
        _logger.LogInformation("Create portfolio clicked");

        var dialog = new PortfolioDialog();
        var userSessionService = ((App)Application.Current).Services.GetRequiredService<IUserSessionService>();
        var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<PortfolioDialogViewModel>>();
        var viewModel = new PortfolioDialogViewModel(dialogLogger, _portfolioService, userSessionService);

        viewModel.SaveCompleted += async (s, success) =>
        {
            if (success)
            {
                dialog.DialogResult = true;
                dialog.Close();
                await LoadPortfoliosAsync();
            }
        };

        dialog.DataContext = viewModel;
        dialog.ShowDialog();
    }

    [RelayCommand]
    private async Task EditPortfolio(PortfolioItemViewModel portfolioItem)
    {
        _logger.LogInformation("Edit portfolio clicked: {PortfolioId}", portfolioItem.Id);

        try
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(portfolioItem.Id);

            if (portfolio == null)
            {
                MessageBox.Show("Portfolio not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new PortfolioDialog();
            var userSessionService = ((App)Application.Current).Services.GetRequiredService<IUserSessionService>();
            var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<PortfolioDialogViewModel>>();
            var viewModel = new PortfolioDialogViewModel(dialogLogger, _portfolioService, userSessionService, portfolio);

            viewModel.SaveCompleted += async (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    await LoadPortfoliosAsync();
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolio for edit: {PortfolioId}", portfolioItem.Id);
            MessageBox.Show($"Error loading portfolio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeletePortfolio(PortfolioItemViewModel portfolio)
    {
        _logger.LogInformation("Delete portfolio clicked: {PortfolioId}", portfolio.Id);

        var result = MessageBox.Show(
            $"Are you sure you want to delete portfolio '{portfolio.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _portfolioService.DeletePortfolioAsync(portfolio.Id);

                if (success)
                {
                    _logger.LogInformation("Portfolio deleted successfully: {PortfolioId}", portfolio.Id);
                    await LoadPortfoliosAsync();
                    MessageBox.Show("Portfolio deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete portfolio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio: {PortfolioId}", portfolio.Id);
                MessageBox.Show($"Error deleting portfolio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ViewPortfolioDetails(PortfolioItemViewModel portfolioItem)
    {
        _logger.LogInformation("View portfolio details clicked: {PortfolioId}", portfolioItem.Id);

        try
        {
            // Get the MainWindow to navigate
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
            {
                // Create a scope for the detail view
                var scope = ((App)Application.Current).Services.CreateScope();
                var detailViewModel = scope.ServiceProvider.GetRequiredService<PortfolioDetailViewModel>();
                var detailView = new PortfolioDetailView
                {
                    DataContext = detailViewModel
                };

                // Load the portfolio
                _ = detailViewModel.LoadPortfolioAsync(portfolioItem.Id);

                // Navigate to the detail view
                mainViewModel.CurrentView = detailView;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to portfolio details: {PortfolioId}", portfolioItem.Id);
            MessageBox.Show($"Error opening portfolio details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}