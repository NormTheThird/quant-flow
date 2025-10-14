namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for the Algorithms view
/// </summary>
public partial class AlgorithmsViewModel : ObservableObject
{
    private readonly ILogger<AlgorithmsViewModel> _logger;
    private readonly IAlgorithmService _algorithmService;
    private readonly Guid _currentUserId;

    [ObservableProperty]
    private ObservableCollection<AlgorithmModel> _algorithms = [];

    [ObservableProperty]
    private bool _hasNoAlgorithms;

    public AlgorithmsViewModel(
        ILogger<AlgorithmsViewModel> logger,
        IAlgorithmService algorithmService,
        IUserSessionService userSessionService)
    {
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _currentUserId = userSessionService.CurrentUserId;
        if (_currentUserId == Guid.Empty)
            throw new InvalidOperationException("User is not logged in");

        _ = LoadAlgorithmsAsync();
    }

    private async Task LoadAlgorithmsAsync()
    {
        try
        {
            _logger.LogInformation("Loading algorithms for user: {UserId}", _currentUserId);

            var algorithms = await _algorithmService.GetAlgorithmsByUserIdAsync(_currentUserId);

            Algorithms = new ObservableCollection<AlgorithmModel>(algorithms.OrderByDescending(_ => _.UpdatedAt));

            HasNoAlgorithms = Algorithms.Count == 0;

            _logger.LogInformation("Loaded {Count} algorithms", Algorithms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading algorithms");
            MessageBox.Show($"Error loading algorithms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CreateAlgorithm()
    {
        _logger.LogInformation("Create algorithm clicked");

        try
        {
            var dialog = new AlgorithmDialog();
            var userSessionService = ((App)Application.Current).Services.GetRequiredService<IUserSessionService>();
            var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<AlgorithmDialogViewModel>>();
            var viewModel = new AlgorithmDialogViewModel(dialogLogger, _algorithmService, userSessionService);

            viewModel.SaveCompleted += async (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    await LoadAlgorithmsAsync();
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening create algorithm dialog");
            MessageBox.Show($"Error opening dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditAlgorithm(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Edit algorithm clicked: {AlgorithmId}", algorithm.Id);

        try
        {
            var dialog = new AlgorithmDialog();
            var userSessionService = ((App)Application.Current).Services.GetRequiredService<IUserSessionService>();
            var dialogLogger = ((App)Application.Current).Services.GetRequiredService<ILogger<AlgorithmDialogViewModel>>();
            var viewModel = new AlgorithmDialogViewModel(dialogLogger, _algorithmService, userSessionService, algorithm);

            viewModel.SaveCompleted += async (s, success) =>
            {
                if (success)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    await LoadAlgorithmsAsync();
                }
            };

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening edit algorithm dialog: {AlgorithmId}", algorithm.Id);
            MessageBox.Show($"Error opening dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteAlgorithm(AlgorithmModel algorithm)
    {
        _logger.LogInformation("Delete algorithm clicked: {AlgorithmId}", algorithm.Id);

        var result = MessageBox.Show(
            $"Are you sure you want to delete algorithm '{algorithm.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _algorithmService.DeleteAlgorithmAsync(algorithm.Id);

                if (success)
                {
                    _logger.LogInformation("Algorithm deleted successfully: {AlgorithmId}", algorithm.Id);
                    await LoadAlgorithmsAsync();
                    MessageBox.Show("Algorithm deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting algorithm: {AlgorithmId}", algorithm.Id);
                MessageBox.Show($"Error deleting algorithm: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}