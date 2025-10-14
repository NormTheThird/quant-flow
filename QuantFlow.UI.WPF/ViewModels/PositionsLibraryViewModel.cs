namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for the Positions Library view showing all standalone positions
/// </summary>
public partial class PositionsLibraryViewModel : ObservableObject
{
    private readonly ILogger<PositionsLibraryViewModel> _logger;
    private readonly IAlgorithmService _algorithmService;
    private readonly IAlgorithmPositionService _algorithmPositionService;
    private readonly IUserSessionService _userSessionService;

    [ObservableProperty]
    private List<PositionDisplayModel> _positions = [];

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasNoPositions;

    public PositionsLibraryViewModel(ILogger<PositionsLibraryViewModel> logger, IAlgorithmService algorithmService, IAlgorithmPositionService algorithmPositionService,
                                     IUserSessionService userSessionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));

        _ = LoadPositionsAsync();
    }

    private async Task LoadPositionsAsync()
    {
        IsLoading = true;

        try
        {
            var userId = _userSessionService.CurrentUserId;
            _logger.LogInformation("Loading unassigned positions for user: {UserId}", userId);

            var positions = await _algorithmPositionService.GetUnassignedPositionsByUserIdAsync(userId);
            var algorithms = await _algorithmService.GetAlgorithmsByUserIdAsync(userId);

            var algorithmDict = algorithms.ToDictionary(a => a.Id, a => a.Name);

            Positions = positions
                .OrderByDescending(_ => _.CreatedAt)
                .Select(p => new PositionDisplayModel
                {
                    Position = p,
                    AlgorithmName = algorithmDict.TryGetValue(p.AlgorithmId, out var name) ? name : "Unknown"
                })
                .ToList();

            HasNoPositions = Positions.Count == 0;

            _logger.LogInformation("Loaded {Count} unassigned positions", Positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading positions");
            Positions = [];
            HasNoPositions = true;
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
    private void BacktestPosition(PositionDisplayModel positionDisplay)
    {
        _logger.LogInformation("Backtest position clicked: {PositionId}", positionDisplay.Position.Id);
        // TODO: Navigate to backtest view
    }
}