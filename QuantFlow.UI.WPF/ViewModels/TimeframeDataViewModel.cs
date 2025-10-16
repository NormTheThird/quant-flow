namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying timeframe OHLCV data
/// </summary>
public partial class TimeframeDataViewModel : ObservableObject
{
    private readonly ILogger<TimeframeDataViewModel> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly MarketDataSummary _summary;
    private readonly IKrakenMarketDataCollectionService? _krakenMarketDataCollectionService;

    private List<MarketDataModel> _allMarketData = [];

    [ObservableProperty]
    private string _exchangeName = string.Empty;

    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private string _timeframeDisplay = string.Empty;

    [ObservableProperty]
    private int _totalRecordCount;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private ObservableCollection<MarketDataModel> _filteredMarketData = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime? _filterStartDate;

    [ObservableProperty]
    private DateTime? _filterEndDate;

    [ObservableProperty]
    private ObservableCollection<int> _pageSizeOptions = [20, 50, 100, 500];

    [ObservableProperty]
    private int _selectedPageSize = 20;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _filteredRecordCount;

    [ObservableProperty]
    private int _currentPageStartIndex;

    [ObservableProperty]
    private int _currentPageEndIndex;

    [ObservableProperty]
    private bool _hasGaps;

    [ObservableProperty]
    private ObservableCollection<DataGap> _dataGaps = [];

    public event EventHandler? NavigateToExchangeRequested;
    public event EventHandler? NavigateToSymbolRequested;

    public TimeframeDataViewModel(ILogger<TimeframeDataViewModel> logger, IMarketDataService marketDataService, MarketDataSummary summary, 
                                  IKrakenMarketDataCollectionService? krakenMarketDataCollectionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _summary = summary ?? throw new ArgumentNullException(nameof(summary));
        _krakenMarketDataCollectionService = krakenMarketDataCollectionService ?? throw new ArgumentNullException(nameof(krakenMarketDataCollectionService));

        ExchangeName = summary.Exchange;
        Symbol = summary.Symbol;
        TimeframeDisplay = summary.Timeframe;
        TotalRecordCount = summary.RecordCount;
        StartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        EndDate = DateTime.UtcNow;

        _ = LoadMarketDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFiltersAndPagination();
    }

    partial void OnFilterStartDateChanged(DateTime? value)
    {
        ApplyFiltersAndPagination();
    }

    partial void OnFilterEndDateChanged(DateTime? value)
    {
        ApplyFiltersAndPagination();
    }

    partial void OnSelectedPageSizeChanged(int value)
    {
        CurrentPage = 1;
        ApplyFiltersAndPagination();
    }

    private async Task LoadMarketDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading market data for {Symbol} {Timeframe} on {Exchange}",
                Symbol, TimeframeDisplay, ExchangeName);

            var exchange = Enum.Parse<Exchange>(ExchangeName);
            var timeframe = ParseTimeframe(TimeframeDisplay);

            var data = await _marketDataService.GetMarketDataAsync(
                exchange,
                Symbol,
                timeframe,
                StartDate,
                EndDate);

            _allMarketData = data.OrderByDescending(_ => _.Timestamp).ToList();
            TotalRecordCount = _allMarketData.Count;

            _logger.LogInformation("Loaded {Count} market data records", _allMarketData.Count);

            // Detect gaps
            var gaps = await _marketDataService.GetDataGapsAsync(
                exchange,
                Symbol,
                timeframe,
                StartDate,
                EndDate);

            DataGaps = new ObservableCollection<DataGap>(gaps.OrderBy(_ => _.StartTime));
            HasGaps = DataGaps.Count > 0;

            _logger.LogInformation("Detected {Count} data gaps", DataGaps.Count);

            ApplyFiltersAndPagination();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading market data for {Symbol} {Timeframe}", Symbol, TimeframeDisplay);
            MessageBox.Show($"Error loading market data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFiltersAndPagination()
    {
        var filtered = _allMarketData.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(_ => _.Timestamp.ToString("yyyy-MM-dd HH:mm:ss").Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply date range filter
        if (FilterStartDate.HasValue)
        {
            filtered = filtered.Where(_ => _.Timestamp >= FilterStartDate.Value);
        }

        if (FilterEndDate.HasValue)
        {
            filtered = filtered.Where(_ => _.Timestamp <= FilterEndDate.Value.AddDays(1).AddSeconds(-1));
        }

        var filteredList = filtered.ToList();
        FilteredRecordCount = filteredList.Count;

        // Apply pagination
        var skip = (CurrentPage - 1) * SelectedPageSize;
        var paged = filteredList.Skip(skip).Take(SelectedPageSize).ToList();

        FilteredMarketData = new ObservableCollection<MarketDataModel>(paged);

        // Update pagination info
        CurrentPageStartIndex = FilteredRecordCount > 0 ? skip + 1 : 0;
        CurrentPageEndIndex = Math.Min(skip + SelectedPageSize, FilteredRecordCount);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            ApplyFiltersAndPagination();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        var totalPages = (int)Math.Ceiling((double)FilteredRecordCount / SelectedPageSize);
        if (CurrentPage < totalPages)
        {
            CurrentPage++;
            ApplyFiltersAndPagination();
        }
    }

    [RelayCommand]
    private async Task ExportCsv()
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"{Symbol}_{TimeframeDisplay}_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filtered = GetFilteredData();
                await ExportToCsvAsync(saveFileDialog.FileName, filtered);
                MessageBox.Show($"Exported {filtered.Count} records to CSV", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to CSV");
            MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ExportExcel()
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"{Symbol}_{TimeframeDisplay}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filtered = GetFilteredData();
                await ExportToExcelAsync(saveFileDialog.FileName, filtered);
                MessageBox.Show($"Exported {filtered.Count} records to Excel", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel");
            MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private List<MarketDataModel> GetFilteredData()
    {
        var filtered = _allMarketData.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(_ => _.Timestamp.ToString("yyyy-MM-dd HH:mm:ss").Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (FilterStartDate.HasValue)
        {
            filtered = filtered.Where(_ => _.Timestamp >= FilterStartDate.Value);
        }

        if (FilterEndDate.HasValue)
        {
            filtered = filtered.Where(_ => _.Timestamp <= FilterEndDate.Value.AddDays(1).AddSeconds(-1));
        }

        return filtered.ToList();
    }

    private async Task ExportToCsvAsync(string filePath, List<MarketDataModel> data)
    {
        await Task.Run(() =>
        {
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Timestamp,Open,High,Low,Close,Volume");

            foreach (var item in data)
            {
                writer.WriteLine($"{item.Timestamp:yyyy-MM-dd HH:mm:ss},{item.Open},{item.High},{item.Low},{item.Close},{item.Volume}");
            }
        });
    }

    private async Task ExportToExcelAsync(string filePath, List<MarketDataModel> data)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"{Symbol}_{TimeframeDisplay}");

            // Headers
            worksheet.Cell(1, 1).Value = "Timestamp";
            worksheet.Cell(1, 2).Value = "Open";
            worksheet.Cell(1, 3).Value = "High";
            worksheet.Cell(1, 4).Value = "Low";
            worksheet.Cell(1, 5).Value = "Close";
            worksheet.Cell(1, 6).Value = "Volume";

            // Data
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                worksheet.Cell(i + 2, 1).Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(i + 2, 2).Value = (double)item.Open;
                worksheet.Cell(i + 2, 3).Value = (double)item.High;
                worksheet.Cell(i + 2, 4).Value = (double)item.Low;
                worksheet.Cell(i + 2, 5).Value = (double)item.Close;
                worksheet.Cell(i + 2, 6).Value = (double)item.Volume;
            }

            // Format
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        });
    }

    private Timeframe ParseTimeframe(string timeframe)
    {
        if (Enum.TryParse<Timeframe>(timeframe, true, out var parsed))
        {
            return parsed;
        }

        return timeframe.ToLower() switch
        {
            "1m" => Timeframe.OneMinute,
            "5m" => Timeframe.FiveMinutes,
            "15m" => Timeframe.FifteenMinutes,
            "30m" => Timeframe.ThirtyMinutes,
            "1h" => Timeframe.OneHour,
            "4h" => Timeframe.FourHours,
            "1d" => Timeframe.OneDay,
            "1w" => Timeframe.OneWeek,
            "1mo" => Timeframe.OneMonth,
            _ => Timeframe.Unknown
        };
    }

    [RelayCommand]
    private void NavigateToExchange()
    {
        _logger.LogInformation("Navigate back to exchange symbols");
        NavigateToExchangeRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void NavigateToSymbol()
    {
        _logger.LogInformation("Navigate back to symbol detail");
        NavigateToSymbolRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task FillGap(DataGap gap)
    {
        try
        {
            _logger.LogInformation("Filling gap from {Start} to {End} for {Symbol} {Timeframe}",
                gap.StartTime, gap.EndTime, Symbol, TimeframeDisplay);

            var exchange = Enum.Parse<Exchange>(ExchangeName);
            var timeframe = ParseTimeframe(TimeframeDisplay);

            // Convert DataGap to MissingDataRange
            var missingRange = new MissingDataRange
            {
                StartTime = gap.StartTime,
                EndTime = gap.EndTime,
                Duration = gap.Duration,
                ExpectedDataPoints = gap.MissingDataPoints,
                Timeframe = timeframe.ToString(),
                Description = $"Gap from {gap.StartTime:yyyy-MM-dd} to {gap.EndTime:yyyy-MM-dd}"
            };

            // Call appropriate exchange service
            if (exchange == Exchange.Kraken && _krakenMarketDataCollectionService != null)
            {
                var result = await _krakenMarketDataCollectionService.PopulateMissingDataAsync(
                    Symbol,
                    timeframe,
                    new[] { missingRange });

                if (result.SuccessfulRanges > 0)
                {
                    MessageBox.Show($"Successfully filled gap with {result.NewDataPointsAdded} data points!",
                        "Gap Filled", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload data and re-detect gaps
                    await LoadMarketDataAsync();
                }
                else
                {
                    var errorMessage = result.Errors.Any()
                        ? string.Join("\n", result.Errors)
                        : "No data was retrieved from the exchange.";
                    MessageBox.Show($"Failed to fill gap:\n{errorMessage}",
                        "Gap Fill Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show($"No collection service available for {exchange}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filling gap for {Symbol} {Timeframe}", Symbol, TimeframeDisplay);
            MessageBox.Show($"Error filling gap: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}