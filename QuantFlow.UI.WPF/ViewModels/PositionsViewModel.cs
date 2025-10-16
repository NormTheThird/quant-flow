namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for the Positions Library view showing all standalone positions
/// </summary>
public partial class PositionsViewModel : ObservableObject
{
    private readonly ILogger<PositionsViewModel> _logger;
    private readonly IAlgorithmService _algorithmService;
    private readonly IAlgorithmPositionService _algorithmPositionService;
    private readonly IPortfolioService _portfolioService;
    private readonly Guid _currentUserId;

    [ObservableProperty]
    private ObservableCollection<PositionDisplayModel> _positions = [];

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasNoPositions;

    public PositionsViewModel(ILogger<PositionsViewModel> logger, IAlgorithmService algorithmService, IAlgorithmPositionService algorithmPositionService,
                                      IPortfolioService portfolioService, IUserSessionService userSessionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));

        _currentUserId = userSessionService.CurrentUserId;
        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        _ = LoadPositionsAsync();
    }

    private async Task LoadPositionsAsync()
    {
        try
        {
            _logger.LogInformation("Loading all positions for user: {UserId}", _currentUserId);

            var positions = await _algorithmPositionService.GetPositionsByUserIdAsync(_currentUserId);
            var positionDisplays = new List<PositionDisplayModel>();

            foreach (var position in positions)
            {
                var algorithm = await _algorithmService.GetAlgorithmByIdAsync(position.AlgorithmId);

                string portfolioName = "Unassigned";
                if (position.PortfolioId.HasValue)
                {
                    var portfolio = await _portfolioService.GetPortfolioByIdAsync(position.PortfolioId.Value);
                    portfolioName = portfolio?.Name ?? "Unknown Portfolio";
                }

                positionDisplays.Add(new PositionDisplayModel
                {
                    Position = position,
                    AlgorithmName = algorithm?.Name ?? "Unknown Algorithm",
                    PortfolioName = portfolioName
                });
            }

            Positions = new ObservableCollection<PositionDisplayModel>(positionDisplays.OrderByDescending(_ => _.Position.UpdatedAt));
            HasNoPositions = Positions.Count == 0;

            _logger.LogInformation("Loaded {Count} positions", Positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading positions");
            MessageBox.Show($"Error loading positions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CreatePosition()
    {
        _logger.LogInformation("Create position clicked");
        var dialog = new AlgorithmPositionDialog();
        var app = (App)Application.Current;

        // Create a scope to resolve scoped services
        var scope = app.Services.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var dialogLogger = scopedProvider.GetRequiredService<ILogger<AlgorithmPositionDialogViewModel>>();
        var algorithmService = scopedProvider.GetRequiredService<IAlgorithmService>();
        var symbolService = scopedProvider.GetRequiredService<ISymbolService>();
        var userSessionService = scopedProvider.GetRequiredService<IUserSessionService>();

        var viewModel = new AlgorithmPositionDialogViewModel(
            dialogLogger,
            _algorithmPositionService,
            algorithmService,
            symbolService,
            userSessionService,
            portfolioId: null,
            existingPosition: null
        );

        viewModel.SaveCompleted += async (s, success) =>
        {
            if (success)
            {
                dialog.DialogResult = true;
                dialog.Close();
                await LoadPositionsAsync();
            }
            scope.Dispose(); // Clean up the scope
        };

        dialog.DataContext = viewModel;
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void EditPosition(PositionDisplayModel positionDisplay)
    {
        _logger.LogInformation("Edit position clicked: {PositionId}", positionDisplay.Position.Id);

        var dialog = new AlgorithmPositionDialog();

        using var scope = ((App)Application.Current).Services.CreateScope();
        var dialogLogger = scope.ServiceProvider.GetRequiredService<ILogger<AlgorithmPositionDialogViewModel>>();
        var algorithmService = scope.ServiceProvider.GetRequiredService<IAlgorithmService>();
        var symbolService = scope.ServiceProvider.GetRequiredService<ISymbolService>();
        var userSessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();

        var viewModel = new AlgorithmPositionDialogViewModel(
            dialogLogger,
            _algorithmPositionService,
            algorithmService,
            symbolService,
            userSessionService,
            portfolioId: null,
            existingPosition: positionDisplay.Position
        );

        viewModel.SaveCompleted += async (s, success) =>
        {
            if (success)
            {
                dialog.DialogResult = true;
                dialog.Close();
                await LoadPositionsAsync();
            }
            scope.Dispose(); // Clean up the scope
        };

        dialog.DataContext = viewModel;
        dialog.ShowDialog();
    }

    [RelayCommand]
    private async Task DeletePosition(PositionDisplayModel positionDisplay)
    {
        _logger.LogInformation("Delete position clicked: {PositionId}", positionDisplay.Position.Id);

        var result = MessageBox.Show(
            $"Are you sure you want to delete position '{positionDisplay.Position.PositionName}'?",
            "Delete Position",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var deleted = await _algorithmPositionService.DeletePositionAsync(positionDisplay.Position.Id);

            if (deleted)
            {
                MessageBox.Show("Position deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPositionsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting position: {PositionId}", positionDisplay.Position.Id);
            MessageBox.Show($"Error deleting position: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RunBacktest(PositionDisplayModel positionDisplay)
    {
        _logger.LogInformation("Run backtest clicked for position: {PositionId}", positionDisplay.Position.Id);

        try
        {
            var dialog = new BacktestConfigurationDialog();

            using var scope = ((App)Application.Current).Services.CreateScope();
            var backtestService = scope.ServiceProvider.GetRequiredService<IBacktestService>();
            var userSessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
            var dialogLogger = scope.ServiceProvider.GetRequiredService<ILogger<BacktestConfigurationDialogViewModel>>();

            var viewModel = new BacktestConfigurationDialogViewModel(dialogLogger, backtestService, userSessionService, positionDisplay.Position);
            viewModel.BacktestCompleted += (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    MessageBox.Show("Backtest created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening backtest configuration dialog");
            MessageBox.Show($"Error opening dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}