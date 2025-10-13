namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// ViewModel for portfolio detail view showing positions
/// </summary>
public partial class PortfolioDetailViewModel : ObservableObject
{
    private readonly ILogger<PortfolioDetailViewModel> _logger;
    private readonly IPortfolioService _portfolioService;
    private readonly IAlgorithmPositionService _algorithmPositionService;

    [ObservableProperty]
    private PortfolioModel? _portfolio;

    [ObservableProperty]
    private ObservableCollection<AlgorithmPositionModel> _positions;

    [ObservableProperty]
    private bool _isLoading;

    public PortfolioDetailViewModel(
        ILogger<PortfolioDetailViewModel> logger,
        IPortfolioService portfolioService,
        IAlgorithmPositionService algorithmPositionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
        _algorithmPositionService = algorithmPositionService ?? throw new ArgumentNullException(nameof(algorithmPositionService));

        _positions = [];
    }

    public async Task LoadPortfolioAsync(Guid portfolioId)
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading portfolio: {PortfolioId}", portfolioId);

            Portfolio = await _portfolioService.GetPortfolioByIdAsync(portfolioId);

            if (Portfolio == null)
            {
                _logger.LogWarning("Portfolio not found: {PortfolioId}", portfolioId);
                return;
            }

            await LoadPositionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolio: {PortfolioId}", portfolioId);
            MessageBox.Show($"Error loading portfolio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPositionsAsync()
    {
        if (Portfolio == null) return;

        try
        {
            _logger.LogInformation("Loading positions for portfolio: {PortfolioId}", Portfolio.Id);

            var positions = await _algorithmPositionService.GetPositionsByPortfolioIdAsync(Portfolio.Id);

            Positions.Clear();
            foreach (var position in positions)
            {
                Positions.Add(position);
            }

            _logger.LogInformation("Loaded {Count} positions", Positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading positions");
            MessageBox.Show($"Error loading positions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddPosition()
    {
        if (Portfolio == null) return;

        _logger.LogInformation("Add position clicked for portfolio: {PortfolioId}", Portfolio.Id);

        try
        {
            var dialog = new AlgorithmPositionDialog();
            var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<AlgorithmPositionDialogViewModel>>();
            var viewModel = new AlgorithmPositionDialogViewModel(
                dialogLogger,
                _algorithmPositionService,
                Portfolio.Id);

            viewModel.SaveCompleted += async (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    await LoadPositionsAsync();
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening add position dialog");
            MessageBox.Show($"Error opening dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditPosition(AlgorithmPositionModel? position)
    {
        if (position == null || Portfolio == null) return;

        _logger.LogInformation("Edit position clicked: {PositionId}", position.Id);

        try
        {
            var dialog = new AlgorithmPositionDialog();
            var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<AlgorithmPositionDialogViewModel>>();
            var viewModel = new AlgorithmPositionDialogViewModel(
                dialogLogger,
                _algorithmPositionService,
                Portfolio.Id,
                position);

            viewModel.SaveCompleted += async (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    await LoadPositionsAsync();
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening edit position dialog");
            MessageBox.Show($"Error opening dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeletePosition(AlgorithmPositionModel? position)
    {
        if (position == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete position '{position.PositionName}'?",
            "Delete Position",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            _logger.LogInformation("Deleting position: {PositionId}", position.Id);

            var deleted = await _algorithmPositionService.DeletePositionAsync(position.Id);

            if (deleted)
            {
                Positions.Remove(position);
                MessageBox.Show("Position deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting position: {PositionId}", position.Id);
            MessageBox.Show($"Error deleting position: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadPositionsAsync();
    }
}