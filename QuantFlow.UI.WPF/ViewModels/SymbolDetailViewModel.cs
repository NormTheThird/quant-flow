namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying symbol timeframe details
/// </summary>
public partial class SymbolDetailViewModel : ObservableObject
{
    private readonly ILogger<SymbolDetailViewModel> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly Exchange _exchange;

    public event EventHandler? NavigateBackRequested;

    [ObservableProperty]
    private string _exchangeName = string.Empty;

    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MarketDataSummary> _timeframeSummaries = [];

    public event EventHandler<MarketDataSummary>? TimeframeSelected;

    public SymbolDetailViewModel(ILogger<SymbolDetailViewModel> logger, IMarketDataService marketDataService, Exchange exchange, string symbol)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _exchange = exchange;
        _symbol = symbol;

        ExchangeName = exchange.ToString();
        Symbol = symbol;

        _ = LoadTimeframeSummariesAsync();
    }

    private async Task LoadTimeframeSummariesAsync()
    {
        try
        {
            _logger.LogInformation("Loading timeframe summaries for {Symbol} on {Exchange}", Symbol, _exchange);

            var allSummaries = await _marketDataService.GetDataAvailabilitySummaryAsync();

            var symbolSummaries = allSummaries
                .Where(_ => _.Symbol.Equals(Symbol, StringComparison.OrdinalIgnoreCase) &&
                           _.Exchange.Equals(_exchange.ToString(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(_ => GetTimeframeOrder(_.Timeframe))
                .ToList();

            TimeframeSummaries = new ObservableCollection<MarketDataSummary>(symbolSummaries);

            _logger.LogInformation("Loaded {Count} timeframe summaries for {Symbol}", TimeframeSummaries.Count, Symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading timeframe summaries for {Symbol}", Symbol);
            MessageBox.Show($"Error loading timeframes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private int GetTimeframeOrder(string timeframe) => timeframe.ToLower() switch
    {
        "1m" => 1,
        "5m" => 2,
        "15m" => 3,
        "30m" => 4,
        "1h" => 5,
        "4h" => 6,
        "1d" => 7,
        "1w" => 8,
        "1mo" => 9,
        _ => 99
    };

    [RelayCommand]
    private void SelectTimeframe(MarketDataSummary summary)
    {
        _logger.LogInformation("Timeframe selected: {Timeframe}", summary.Timeframe);
        TimeframeSelected?.Invoke(this, summary);
    }

    [RelayCommand]
    private void NavigateBackToExchange()
    {
        _logger.LogInformation("Navigate back to exchange symbols");
        NavigateBackRequested?.Invoke(this, EventArgs.Empty);
    }
}