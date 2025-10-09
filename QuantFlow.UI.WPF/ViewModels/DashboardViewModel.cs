namespace QuantFlow.UI.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private List<CryptoCardViewModel> _cryptoCards = new();

    [ObservableProperty]
    private List<PortfolioCardViewModel> _portfolios = new();

    [ObservableProperty]
    private List<RecentTradeViewModel> _recentTrades = new();

    public DashboardViewModel(ILogger<DashboardViewModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("DashboardViewModel initialized");

        InitializeCryptoCards();
        InitializePortfolios();
        InitializeRecentTrades();
    }

    private void InitializeCryptoCards()
    {
        CryptoCards = new List<CryptoCardViewModel>
        {
            new CryptoCardViewModel("BTC", "Bitcoin", 43521.32, 2.45,
                new[] { 42000.0, 42500, 41800, 43000, 42700, 43200, 43521 }),

            new CryptoCardViewModel("ETH", "Ethereum", 2284.56, 3.12,
                new[] { 2200.0, 2250, 2180, 2300, 2270, 2290, 2284 }),

            new CryptoCardViewModel("ADA", "Cardano", 0.5821, -1.23,
                new[] { 0.60, 0.59, 0.58, 0.57, 0.58, 0.59, 0.58 }),

            new CryptoCardViewModel("FET", "Fetch.ai", 1.2453, 5.67,
                new[] { 1.10, 1.15, 1.12, 1.18, 1.20, 1.23, 1.24 }),

            new CryptoCardViewModel("SOL", "Solana", 98.32, 4.21,
                new[] { 92.0, 94, 93, 96, 97, 98, 98 }),

            new CryptoCardViewModel("DOT", "Polkadot", 6.84, -0.87,
                new[] { 7.10, 7.05, 6.95, 6.90, 6.85, 6.88, 6.84 })
        };
    }

    private void InitializePortfolios()
    {
        Portfolios = new List<PortfolioCardViewModel>
        {
            new PortfolioCardViewModel("Main Trading", true, "Kraken", 127384.52, 100000.00, 8),
            new PortfolioCardViewModel("Strategy Testing", false, "Kraken", 15847.32, 10000.00, 3),
            new PortfolioCardViewModel("Conservative", true, "KuCoin", 52847.12, 50000.00, 4)
        };
    }

    private void InitializeRecentTrades()
    {
        // Show only the 10 most recent trades
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