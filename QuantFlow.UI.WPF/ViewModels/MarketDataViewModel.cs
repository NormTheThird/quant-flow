namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for the Market Data view
/// </summary>
public partial class MarketDataViewModel : ObservableObject
{
    private readonly ILogger<MarketDataViewModel> _logger;
    private readonly IMarketDataService _marketDataService;

    [ObservableProperty]
    private ObservableCollection<MarketDataSummary> _marketDataSummaries = [];

    [ObservableProperty]
    private bool _isLoading;

    public MarketDataViewModel(ILogger<MarketDataViewModel> logger, IMarketDataService marketDataService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));

        _ = LoadMarketDataAsync();
    }

    private async Task LoadMarketDataAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading market data summaries");

            var summaries = await _marketDataService.GetDataAvailabilitySummaryAsync();

            MarketDataSummaries = new ObservableCollection<MarketDataSummary>(
                summaries.OrderBy(_ => _.Symbol)
                    .ThenBy(_ => _.Exchange)
                    .ThenBy(_ => _.Timeframe));

            _logger.LogInformation("Loaded {Count} market data summaries", MarketDataSummaries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading market data");
            MessageBox.Show($"Error loading market data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}