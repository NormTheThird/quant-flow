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

    public PortfolioCardViewModel(string name, bool isLive, string exchange, double totalValue, double allocatedFunds, int activeAlgorithms)
    {
        PortfolioName = name;
        IsLive = isLive;
        ModeBadgeText = isLive ? "LIVE" : "TEST";
        ModeBadgeColor = isLive ? "#4cceac" : "#f59e0b";
        Exchange = exchange;

        TotalValue = $"${totalValue:N2}";
        AllocatedFunds = $"${allocatedFunds:N2}";

        var pl = totalValue - allocatedFunds;
        var plPercent = (pl / allocatedFunds) * 100;

        ProfitLoss = pl >= 0 ? $"+${pl:N2}" : $"-${Math.Abs(pl):N2}";
        ProfitLossPercent = pl >= 0 ? $"+{plPercent:F2}%" : $"{plPercent:F2}%";
        ProfitLossColor = pl >= 0 ? "#4cceac" : "#db4f4a";

        ActiveAlgorithms = activeAlgorithms;
    }
}