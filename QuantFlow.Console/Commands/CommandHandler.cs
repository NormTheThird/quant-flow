
namespace QuantFlow.Console.Commands;

/// <summary>
/// Interface for handling console commands
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Executes a command based on the provided arguments
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code (0 for success, non-zero for error)</returns>
    Task<int> ExecuteAsync(string[] args);
    Task ExecuteCustomAsync();
}

/// <summary>
/// Main command handler that routes commands to specific handlers
/// </summary>
public class CommandHandler : ICommandHandler
{
    private readonly IMarketDataService _marketDataService;
    private readonly IExchangeConfigurationService _exchangeConfigService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        IMarketDataService marketDataService,
        IExchangeConfigurationService exchangeConfigService,
        ILogger<CommandHandler> logger)
    {
        _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _exchangeConfigService = exchangeConfigService ?? throw new ArgumentNullException(nameof(exchangeConfigService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a command based on the provided arguments
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code (0 for success, non-zero for error)</returns>
    public async Task<int> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return 0;
            }

            var command = args[0].ToLowerInvariant();

            return command switch
            {
                "market-data" => await HandleMarketDataCommand(args.Skip(1).ToArray()),
                "help" or "--help" or "-h" => ShowHelp(),
                _ => ShowUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in command execution");
            System.Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task ExecuteCustomAsync()
    {
        await TestInfluxDB();
    }

    /// <summary>
    /// Handles market data related commands
    /// </summary>
    private async Task<int> HandleMarketDataCommand(string[] args)
    {
        var options = ParseMarketDataOptions(args);
        if (options == null) return 1;

        try
        {
            switch (options.Action)
            {
                case "get":
                    return await GetMarketData(options);
                case "latest":
                    return await GetLatestMarketData(options);
                case "validate":
                    return await ValidateMarketData(options);
                case "gaps":
                    return await DetectDataGaps(options);
                case "availability":
                    return await CheckDataAvailability(options);
                default:
                    System.Console.WriteLine($"Unknown market-data action: {options.Action}");
                    ShowMarketDataHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing market-data command");
            System.Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Gets market data for specified parameters
    /// </summary>
    private async Task<int> GetMarketData(MarketDataOptions options)
    {
        System.Console.WriteLine($"Retrieving market data for {options.Symbol} ({options.Timeframe})...");

        var marketData = await _marketDataService.GetMarketDataAsync(options.Exchange, options.Symbol!,
            options.Timeframe!, options.StartDate!.Value, options.EndDate!.Value, options.Limit);

        var dataList = marketData.ToList();

        if (dataList.Count == 0)
        {
            System.Console.WriteLine("No data found for the specified criteria.");
            return 0;
        }

        System.Console.WriteLine($"\nRetrieved {dataList.Count} data points:");
        System.Console.WriteLine("Timestamp\t\t\tOpen\t\tHigh\t\tLow\t\tClose\t\tVolume");
        System.Console.WriteLine(new string('-', 120));

        var displayCount = Math.Min(options.Limit ?? 10, dataList.Count);
        for (int i = 0; i < displayCount; i++)
        {
            var data = dataList[i];
            System.Console.WriteLine($"{data.Timestamp:yyyy-MM-dd HH:mm:ss}\t{data.Open:F4}\t\t{data.High:F4}\t\t{data.Low:F4}\t\t{data.Close:F4}\t\t{data.Volume:F2}");
        }

        if (dataList.Count > displayCount)
        {
            System.Console.WriteLine($"... and {dataList.Count - displayCount} more records (use --limit to show more)");
        }

        return 0;
    }

    /// <summary>
    /// Gets the latest market data for a symbol
    /// </summary>
    private async Task<int> GetLatestMarketData(MarketDataOptions options)
    {
        System.Console.WriteLine($"Retrieving latest market data for {options.Symbol}...");

        var latestData = await _marketDataService.GetLatestMarketDataAsync(options.Exchange, options.Symbol!, options.Timeframe);

        if (latestData == null)
        {
            System.Console.WriteLine("No data found for the specified symbol.");
            return 0;
        }

        System.Console.WriteLine($"\nLatest data for {latestData.Symbol}:");
        System.Console.WriteLine($"Timeframe: {latestData.Timeframe}");
        System.Console.WriteLine($"Exchange: {latestData.Exchange}");
        System.Console.WriteLine($"Timestamp: {latestData.Timestamp:yyyy-MM-dd HH:mm:ss}");
        System.Console.WriteLine($"Open:      {latestData.Open:F4}");
        System.Console.WriteLine($"High:      {latestData.High:F4}");
        System.Console.WriteLine($"Low:       {latestData.Low:F4}");
        System.Console.WriteLine($"Close:     {latestData.Close:F4}");
        System.Console.WriteLine($"Volume:    {latestData.Volume:F2}");

        return 0;
    }

    /// <summary>
    /// Validates market data quality
    /// </summary>
    private async Task<int> ValidateMarketData(MarketDataOptions options)
    {
        System.Console.WriteLine($"Validating market data for {options.Symbol} ({options.Timeframe})...");

        var marketData = await _marketDataService.GetMarketDataAsync(options.Exchange, options.Symbol!, options.Timeframe!,
            options.StartDate!.Value, options.EndDate!.Value);

        var report = await _marketDataService.ValidateDataQualityAsync(marketData, options.Timeframe!);

        System.Console.WriteLine($"\nData Quality Report for {report.Symbol}:");
        System.Console.WriteLine($"Exchange:             {options.Exchange}");
        System.Console.WriteLine($"Timeframe:              {report.Timeframe}");
        System.Console.WriteLine($"Period:                 {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
        System.Console.WriteLine($"Total Data Points:      {report.TotalDataPoints:N0}");
        System.Console.WriteLine($"Expected Data Points:   {report.ExpectedDataPoints:N0}");
        System.Console.WriteLine($"Data Completeness:      {report.DataCompleteness:P2}");
        System.Console.WriteLine($"Is Valid:               {(report.IsValid ? "Yes" : "No")}");
        System.Console.WriteLine($"Invalid Price Relations: {report.InvalidPriceRelationships}");
        System.Console.WriteLine($"Zero Volume Candles:    {report.ZeroVolumeCandles}");
        System.Console.WriteLine($"Duplicate Timestamps:   {report.DuplicateTimestamps}");
        System.Console.WriteLine($"Data Gaps:              {report.Gaps.Count}");

        if (report.ValidationErrors.Count > 0)
        {
            System.Console.WriteLine($"\nValidation Errors:");
            foreach (var error in report.ValidationErrors.Take(10))
            {
                System.Console.WriteLine($"  - {error}");
            }
            if (report.ValidationErrors.Count > 10)
            {
                System.Console.WriteLine($"  ... and {report.ValidationErrors.Count - 10} more errors");
            }
        }

        return report.IsValid ? 0 : 1;
    }

    /// <summary>
    /// Detects gaps in market data
    /// </summary>
    private async Task<int> DetectDataGaps(MarketDataOptions options)
    {
        System.Console.WriteLine($"Detecting data gaps for {options.Symbol} ({options.Timeframe})...");

        var gaps = await _marketDataService.GetDataGapsAsync(options.Exchange, options.Symbol!, options.Timeframe!, options.StartDate!.Value, options.EndDate!.Value);

        var gapList = gaps.ToList();

        if (gapList.Count == 0)
        {
            System.Console.WriteLine("No data gaps detected.");
            return 0;
        }

        System.Console.WriteLine($"\nDetected {gapList.Count} data gaps:");
        System.Console.WriteLine("Start Time\t\t\tEnd Time\t\t\tDuration\t\tMissing Points");
        System.Console.WriteLine(new string('-', 100));

        foreach (var gap in gapList.Take(20))
        {
            System.Console.WriteLine($"{gap.StartTime:yyyy-MM-dd HH:mm:ss}\t{gap.EndTime:yyyy-MM-dd HH:mm:ss}\t{gap.Duration}\t\t{gap.MissingDataPoints}");
        }

        if (gapList.Count > 20)
        {
            System.Console.WriteLine($"... and {gapList.Count - 20} more gaps");
        }

        return 0;
    }

    /// <summary>
    /// Checks data availability for a symbol
    /// </summary>
    private async Task<int> CheckDataAvailability(MarketDataOptions options)
    {
        System.Console.WriteLine($"Checking data availability for {options.Symbol}...");

        var availability = await _marketDataService.GetDataAvailabilityAsync(options.Exchange, options.Symbol!);

        System.Console.WriteLine($"\nData Availability for {availability.Symbol}:");
        System.Console.WriteLine($"Exchange:       {availability.Exchange}"); // Note: should be availability.DataSource when model is updated
        System.Console.WriteLine($"Earliest Data:    {availability.EarliestDataPoint?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None"}");
        System.Console.WriteLine($"Latest Data:      {availability.LatestDataPoint?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None"}");
        System.Console.WriteLine($"Total Data Span:  {availability.TotalDataSpan?.ToString(@"dd\.hh\:mm\:ss") ?? "None"}");
        System.Console.WriteLine($"Total Data Points: {availability.TotalDataPoints:N0}");

        if (availability.TimeframeAvailability.Count > 0)
        {
            System.Console.WriteLine($"\nTimeframe Availability:");
            System.Console.WriteLine("Timeframe\tFirst Available\t\t\tLast Available\t\t\tData Points\tCompleteness\tGaps");
            System.Console.WriteLine(new string('-', 120));

            foreach (var tf in availability.TimeframeAvailability.OrderBy(x => x.Key))
            {
                var info = tf.Value;
                System.Console.WriteLine($"{tf.Key}\t\t{info.FirstAvailable?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None"}\t{info.LastAvailable?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None"}\t{info.DataPointCount:N0}\t\t{info.CompletenessPercentage:P1}\t\t{info.GapCount}");
            }
        }

        return 0;
    }

    /// <summary>
    /// Parses market data command options
    /// </summary>
    private MarketDataOptions? ParseMarketDataOptions(string[] args)
    {
        var options = new MarketDataOptions();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "get":
                case "latest":
                case "validate":
                case "gaps":
                case "availability":
                    options.Action = args[i].ToLowerInvariant();
                    break;
                case "--symbol":
                case "-s":
                    if (i + 1 < args.Length)
                        options.Symbol = args[++i];
                    break;
                case "--timeframe":
                case "-t":
                    if (i + 1 < args.Length)
                    {
                        var timeframeString = args[++i];
                        if (TryParseTimeframe(timeframeString, out var timeframe))
                            options.Timeframe = timeframe;
                        else
                            System.Console.WriteLine($"Warning: Invalid timeframe '{timeframeString}'. Using default.");
                    }
                    break;
                case "--start":
                    if (i + 1 < args.Length && DateTime.TryParse(args[++i], out var start))
                        options.StartDate = start;
                    break;
                case "--end":
                    if (i + 1 < args.Length && DateTime.TryParse(args[++i], out var end))
                        options.EndDate = end;
                    break;
                case "--exchange":
                case "--datasource":
                case "--data-source":
                case "-e":
                case "-d":
                    if (i + 1 < args.Length)
                    {
                        var exchangeString = args[++i];
                        if (TryParseExchange(exchangeString, out var exchange))
                            options.Exchange = exchange;
                        else
                            System.Console.WriteLine($"Warning: Invalid exchange '{exchangeString}'. Using default.");
                    }
                    break;
                case "--limit":
                case "-l":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var limit))
                        options.Limit = limit;
                    break;
                case "--help":
                case "-h":
                    ShowMarketDataHelp();
                    return null;
                default:
                    if (string.IsNullOrEmpty(options.Action))
                        options.Action = args[i].ToLowerInvariant();
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Tries to parse a timeframe string to Timeframe enum
    /// </summary>
    private static bool TryParseTimeframe(string timeframeString, out Timeframe timeframe)
    {
        timeframe = timeframeString.ToLowerInvariant() switch
        {
            "1m" => Timeframe.OneMinute,
            "5m" => Timeframe.FiveMinutes,
            "15m" => Timeframe.FifteenMinutes,
            "30m" => Timeframe.ThirtyMinutes,
            "1h" => Timeframe.OneHour,
            "4h" => Timeframe.FourHours,
            "1d" => Timeframe.OneDay,
            "1w" => Timeframe.OneWeek,
            _ => Timeframe.Unknown
        };

        return timeframe != Timeframe.Unknown;
    }

    /// <summary>
    /// Tries to parse an exchange string to Exchange enum
    /// </summary>
    private static bool TryParseExchange(string exchangeString, out Exchange exchange)
    {
        return Enum.TryParse<Exchange>(exchangeString, true, out exchange) &&
               exchange != Exchange.Unknown;
    }

    /// <summary>
    /// Shows general help information
    /// </summary>
    private int ShowHelp()
    {
        System.Console.WriteLine("QuantFlow Trading Platform Console");
        System.Console.WriteLine("==================================");
        System.Console.WriteLine();
        System.Console.WriteLine("Usage: QuantFlow.Console [command] [options]");
        System.Console.WriteLine();
        System.Console.WriteLine("Available commands:");
        System.Console.WriteLine("  market-data    Market data operations (get, validate, analyze)");
        System.Console.WriteLine("  help           Show this help message");
        System.Console.WriteLine();
        System.Console.WriteLine("For command-specific help, use: [command] --help");
        System.Console.WriteLine();
        System.Console.WriteLine("Examples:");
        System.Console.WriteLine("  QuantFlow.Console market-data get --symbol BTCUSDT --timeframe 1h --start 2024-01-01");
        System.Console.WriteLine("  QuantFlow.Console market-data latest --symbol ETHUSDT --datasource kraken");
        System.Console.WriteLine("  QuantFlow.Console market-data validate --symbol BTCUSDT --timeframe 1d --start 2024-01-01");

        return 0;
    }

    /// <summary>
    /// Shows market data command help
    /// </summary>
    private static void ShowMarketDataHelp()
    {
        System.Console.WriteLine("Market Data Commands");
        System.Console.WriteLine("===================");
        System.Console.WriteLine();
        System.Console.WriteLine("Usage: market-data [action] [options]");
        System.Console.WriteLine();
        System.Console.WriteLine("Actions:");
        System.Console.WriteLine("  get           Get market data for a time range");
        System.Console.WriteLine("  latest        Get the latest market data point");
        System.Console.WriteLine("  validate      Validate data quality for a time range");
        System.Console.WriteLine("  gaps          Detect gaps in market data");
        System.Console.WriteLine("  availability  Check data availability for a symbol");
        System.Console.WriteLine();
        System.Console.WriteLine("Options:");
        System.Console.WriteLine("  --symbol, -s        Trading symbol (required, e.g., BTCUSDT)");
        System.Console.WriteLine("  --timeframe, -t     Timeframe (required for get/validate/gaps, e.g., 1m, 5m, 1h, 1d)");
        System.Console.WriteLine("  --start             Start date (required for get/validate/gaps, e.g., 2024-01-01)");
        System.Console.WriteLine("  --end               End date (optional, defaults to now)");
        System.Console.WriteLine("  --datasource, -d    Data source filter (optional, e.g., kraken, kucoin)");
        System.Console.WriteLine("  --limit, -l         Limit number of records displayed (default: 10)");
        System.Console.WriteLine("  --help, -h          Show this help message");
        System.Console.WriteLine();
        System.Console.WriteLine("Examples:");
        System.Console.WriteLine("  market-data get --symbol BTCUSDT --timeframe 1h --start 2024-01-01 --limit 20");
        System.Console.WriteLine("  market-data latest --symbol ETHUSDT --datasource kraken");
        System.Console.WriteLine("  market-data validate --symbol BTCUSDT --timeframe 1d --start 2024-01-01");
        System.Console.WriteLine("  market-data gaps --symbol ADAUSDT --timeframe 1h --start 2024-06-01 --end 2024-06-30");
        System.Console.WriteLine("  market-data availability --symbol DOTUSDT --datasource kucoin");
    }

    /// <summary>
    /// Shows unknown command message
    /// </summary>
    private int ShowUnknownCommand(string command)
    {
        System.Console.WriteLine($"Unknown command: {command}");
        System.Console.WriteLine("Use 'help' to see available commands.");
        return 1;
    }

    // Exchange configuration methods remain the same since they don't use market data parameters
    // [Include all the existing exchange configuration methods here - they don't need updates]

    private async Task TestInfluxDB()
    {
        await Task.Delay(1);
    }

    private static IEnumerable<MarketDataModel> GenerateMockData(Exchange exchange, string symbol, Timeframe timeframe, DateTime start, DateTime end)
    {
        var random = new Random();
        var current = start;
        var price = 45000m; // Starting price
        var data = new List<MarketDataModel>();

        // Determine interval based on timeframe enum
        var interval = TimeSpan.FromMinutes((int)timeframe);

        while (current <= end && data.Count < 1000) // Limit to 1000 points for demo
        {
            // Generate realistic price movement
            var change = (decimal)(random.NextDouble() - 0.5) * 100; // +/- $50 max change
            var open = price;
            var close = Math.Max(1m, price + change);
            var high = Math.Max(open, close) + (decimal)(random.NextDouble() * 50);
            var low = Math.Min(open, close) - (decimal)(random.NextDouble() * 50);
            var volume = (decimal)(random.NextDouble() * 100 + 10);

            data.Add(new MarketDataModel
            {
                Symbol = symbol,
                Timeframe = timeframe,
                Exchange = Exchange.Kraken,
                Open = open,
                High = high,
                Low = Math.Max(1m, low),
                Close = close,
                Volume = volume,
                VWAP = (high + low + close) / 3,
                TradeCount = random.Next(100, 1000),
                Bid = close - 0.01m,
                Ask = close + 0.01m,
                QuoteVolume = volume * close,
                Timestamp = current
            });

            price = close;
            current = current.Add(interval);
        }

        return data;
    }



}