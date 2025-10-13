namespace QuantFlow.UI.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILogger<DashboardViewModel> _logger;
    private readonly IUserPreferencesRepository _userPreferencesRepository;
    private readonly IKrakenApiService _krakenApiService;
    private readonly IUserSessionService _userSessionService;
    private readonly ISymbolService _symbolService;
    private readonly IPortfolioService _portfolioService;

    [ObservableProperty]
    private List<CryptoCardViewModel> _cryptoCards = new();

    [ObservableProperty]
    private List<PortfolioCardViewModel> _portfolios = new();

    [ObservableProperty]
    private List<RecentTradeViewModel> _recentTrades = new();

    [ObservableProperty]
    private bool _isLoadingMarketOverview = false;

    [ObservableProperty]
    private bool _isLoadingPortfolios = false;

    [ObservableProperty]
    private bool _hasNoSymbols = false;

    [ObservableProperty]
    private bool _hasNoPortfolios;

    public DashboardViewModel(ILogger<DashboardViewModel> logger, IUserPreferencesRepository userPreferencesRepository, IKrakenApiService krakenApiService,
                                      IUserSessionService userSessionService, ISymbolService symbolService, IPortfolioService portfolioService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userPreferencesRepository = userPreferencesRepository ?? throw new ArgumentNullException(nameof(userPreferencesRepository));
        _krakenApiService = krakenApiService ?? throw new ArgumentNullException(nameof(krakenApiService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _symbolService = symbolService ?? throw new ArgumentNullException(nameof(symbolService));
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));

        _logger.LogInformation("DashboardViewModel initialized");

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        InitializeRecentTrades();

        // Load market overview cards asynchronously without blocking
        _ = LoadMarketOverviewCardsAsync();
        await LoadPortfoliosAsync();
    }

    private async Task LoadMarketOverviewCardsAsync()
    {
        IsLoadingMarketOverview = true;

        try
        {
            var userId = _userSessionService.CurrentUserId;
            _logger.LogInformation("Loading market overview cards for user: {UserId}", userId);

            var cryptoCards = await FetchCryptoCardsAsync(userId);

            CryptoCards = cryptoCards;
            HasNoSymbols = cryptoCards.Count == 0;
            _logger.LogInformation("Loaded {Count} crypto cards", cryptoCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error loading market overview cards");
            SetEmptyState();
        }
        finally
        {
            IsLoadingMarketOverview = false;
        }
    }

    private async Task<List<CryptoCardViewModel>> FetchCryptoCardsAsync(Guid userId)
    {
        // Get user preferences
        var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId);
        if (preferences == null)
        {
            _logger.LogWarning("No preferences found for user: {UserId}", userId);
            return new List<CryptoCardViewModel>();
        }

        // Parse MarketOverviewCards from preferences
        var marketOverviewCards = preferences.MarketOverviewCards as Dictionary<string, object>;
        if (marketOverviewCards == null || !marketOverviewCards.ContainsKey("Kraken"))
        {
            _logger.LogInformation("No Kraken symbols configured for user: {UserId}", userId);
            return new List<CryptoCardViewModel>();
        }

        // Get Kraken symbols
        var krakenSymbols = marketOverviewCards["Kraken"] as List<object>;
        if (krakenSymbols == null || !krakenSymbols.Any())
        {
            _logger.LogInformation("User has empty Kraken symbols list");
            return new List<CryptoCardViewModel>();
        }

        var cryptoCards = new List<CryptoCardViewModel>();

        // Fetch data for each symbol
        foreach (var symbolObj in krakenSymbols)
        {
            var symbol = symbolObj?.ToString();
            if (string.IsNullOrEmpty(symbol)) continue;

            var card = await FetchCardForSymbolAsync(symbol);
            if (card != null)
                cryptoCards.Add(card);
        }

        return cryptoCards;
    }
    
    private async Task<CryptoCardViewModel?> FetchCardForSymbolAsync(string symbol)
    {
        try
        {
            // Get symbol info from database first
            var symbolInfo = await _symbolService.GetBySymbolAsync(symbol);
            if (symbolInfo == null)
            {
                _logger.LogWarning("Symbol {Symbol} not found in database, skipping", symbol);
                return null;
            }

            var dailyData = await _krakenApiService.GetDailyKlinesAsync(symbol, 7);
            if (dailyData == null || !dailyData.Any())
            {
                _logger.LogWarning("No daily data found for symbol: {Symbol}, showing as unsupported", symbol);
                return new CryptoCardViewModel(symbolInfo, 0, 0, new double[7], "Kraken", isSupported: false);
            }

            var currentPrice = dailyData.Last().ClosingPrice;
            var previousClose = dailyData.First().ClosingPrice;
            var changePercent = ((currentPrice - previousClose) / previousClose) * 100;
            var chartData = dailyData.Select(k => (double)k.ClosingPrice).ToArray();

            return new CryptoCardViewModel(
                symbolInfo,
                (double)currentPrice,
                (double)changePercent,
                chartData,
                "Kraken",
                isSupported: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch data for symbol: {Symbol}", symbol);

            // Try to get symbol info for unsupported card
            var symbolInfo = await _symbolService.GetBySymbolAsync(symbol);
            if (symbolInfo != null)
                return new CryptoCardViewModel(symbolInfo, 0, 0, new double[7], "Kraken", isSupported: false);

            return null;
        }
    }
   
    private async Task LoadPortfoliosAsync()
    {
        IsLoadingPortfolios = true;

        try
        {
            var userId = _userSessionService.CurrentUserId;
            _logger.LogInformation("Loading portfolios for user: {UserId}", userId);

            var portfolios = await _portfolioService.GetPortfoliosByUserIdAsync(userId);

            Portfolios = portfolios.Select(p => new PortfolioCardViewModel(p)).OrderBy(_ => _.ModeBadgeText).ToList();

            HasNoPortfolios = Portfolios.Count == 0;
            _logger.LogInformation("Loaded {Count} portfolios for dashboard", Portfolios.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading portfolios for dashboard");
            Portfolios = new List<PortfolioCardViewModel>();
            HasNoPortfolios = true;
        }
        finally
        {
            IsLoadingPortfolios = false;
        }
    }

    private void SetEmptyState()
    {
        CryptoCards = new List<CryptoCardViewModel>();
        HasNoSymbols = true;
    }

    private void InitializeRecentTrades()
    {
        RecentTrades = new List<RecentTradeViewModel>
        {
            new RecentTradeViewModel("BUY", "BTC/USDT", 0.0245, 43521.32,
                DateTime.Now.AddMinutes(-5), "Completed", false, "Main Trading", "Kraken"),

            new RecentTradeViewModel("SELL", "ETH/USDT", 0.8520, 2284.56,
                DateTime.Now.AddMinutes(-15), "Completed", true, "Strategy Testing", "Kraken"),

            new RecentTradeViewModel("BUY", "ADA/USDT", 1500.00, 0.5821,
                DateTime.Now.AddHours(-1), "Pending", false, "Main Trading", "Kraken"),

            new RecentTradeViewModel("BUY", "SOL/USDT", 5.2100, 98.32,
                DateTime.Now.AddHours(-2), "Completed", true, "Strategy Testing", "Kraken"),

            new RecentTradeViewModel("SELL", "FET/USDT", 425.50, 1.2453,
                DateTime.Now.AddHours(-3), "Failed", false, "Main Trading", "Kraken"),

            new RecentTradeViewModel("BUY", "DOT/USDT", 75.00, 6.84,
                DateTime.Now.AddHours(-5), "Completed", true, "Strategy Testing", "Kraken"),

            new RecentTradeViewModel("SELL", "BTC/USDT", 0.0150, 43200.00,
                DateTime.Now.AddHours(-6), "Completed", false, "Conservative", "KuCoin"),

            new RecentTradeViewModel("BUY", "ETH/USDT", 1.2500, 2250.00,
                DateTime.Now.AddHours(-8), "Completed", false, "Main Trading", "Kraken"),

            new RecentTradeViewModel("SELL", "ADA/USDT", 2000.00, 0.59,
                DateTime.Now.AddHours(-10), "Completed", true, "Strategy Testing", "Kraken"),

            new RecentTradeViewModel("BUY", "SOL/USDT", 3.5000, 95.50,
                DateTime.Now.AddHours(-12), "Completed", false, "Main Trading", "Kraken")
        };
    }
}