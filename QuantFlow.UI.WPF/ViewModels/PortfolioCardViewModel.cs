namespace QuantFlow.UI.WPF.ViewModels;

public partial class PortfolioCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _portfolioName = string.Empty;

    [ObservableProperty]
    private bool _isLive;

    [ObservableProperty]
    private string _modeBadgeText = string.Empty;

    [ObservableProperty]
    private string _modeBadgeColor = string.Empty;

    [ObservableProperty]
    private string _exchange = string.Empty;

    [ObservableProperty]
    private string _totalValue = string.Empty;

    [ObservableProperty]
    private string _allocatedFunds = string.Empty;

    [ObservableProperty]
    private string _profitLoss = string.Empty;

    [ObservableProperty]
    private string _profitLossPercent = string.Empty;

    [ObservableProperty]
    private string _profitLossColor = string.Empty;

    [ObservableProperty]
    private int _activeAlgorithms;

    public PortfolioCardViewModel(PortfolioModel portfolio)
    {
        PortfolioName = portfolio.Name;

        // Mode badge
        IsLive = portfolio.Mode == PortfolioMode.ExchangeConnected;
        ModeBadgeText = portfolio.Mode == PortfolioMode.ExchangeConnected ? "LIVE" : "TEST";
        ModeBadgeColor = portfolio.Mode == PortfolioMode.ExchangeConnected ? "#4cceac" : "#f59e0b";

        // Exchange
        Exchange = portfolio.Exchange?.ToString() ?? "N/A";

        // Total Value = Current Balance
        TotalValue = $"${portfolio.CurrentBalance:N2}";

        // Allocated Funds = Initial Balance
        AllocatedFunds = $"${portfolio.InitialBalance:N2}";

        // Profit/Loss calculation
        var pl = portfolio.CurrentBalance - portfolio.InitialBalance;
        var plPercent = portfolio.InitialBalance > 0
            ? (pl / portfolio.InitialBalance) * 100
            : 0;

        ProfitLoss = pl >= 0 ? $"+${pl:N2}" : $"-${Math.Abs(pl):N2}";
        ProfitLossPercent = pl >= 0 ? $"+{plPercent:F2}%" : $"{plPercent:F2}%";
        ProfitLossColor = pl >= 0 ? "#4cceac" : "#db4f4a";

        // Active Algorithms - TODO: track this when we implement active trading
        ActiveAlgorithms = 0;
    }
}