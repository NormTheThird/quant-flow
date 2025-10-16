namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying exchange symbols
/// </summary>
public partial class ExchangeSymbolsViewModel : ObservableObject
{
    private readonly ILogger<ExchangeSymbolsViewModel> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly Exchange _exchange;

    [ObservableProperty]
    private string _exchangeName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _symbols = [];

    public event EventHandler<string>? SymbolSelected;

    public ExchangeSymbolsViewModel(ILogger<ExchangeSymbolsViewModel> logger, IMarketDataService marketDataService, Exchange exchange)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _exchange = exchange;

        ExchangeName = exchange.ToString();

        _ = LoadSymbolsAsync();
    }

    private async Task LoadSymbolsAsync()
    {
        try
        {
            _logger.LogInformation("Loading symbols for {Exchange}", _exchange);

            var summaries = await _marketDataService.GetDataAvailabilitySummaryAsync();

            _logger.LogInformation("Retrieved {Count} total summaries", summaries.Count());

            var filteredSummaries = summaries
                .Where(_ => _.Exchange.Equals(_exchange.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Filtered to {Count} summaries for {Exchange}", filteredSummaries.Count, _exchange);

            var uniqueSymbols = filteredSummaries
                .Select(_ => _.Symbol)
                .Distinct()
                .OrderBy(_ => _)
                .ToList();

            _logger.LogInformation("Found {Count} unique symbols", uniqueSymbols.Count);

            Symbols = new ObservableCollection<string>(uniqueSymbols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading symbols for {Exchange}", _exchange);
            MessageBox.Show($"Error loading symbols: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SelectSymbol(string symbol)
    {
        _logger.LogInformation("Symbol selected: {Symbol}", symbol);
        SymbolSelected?.Invoke(this, symbol);
    }
}