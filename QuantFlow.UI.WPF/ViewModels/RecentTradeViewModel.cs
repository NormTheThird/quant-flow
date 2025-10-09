namespace QuantFlow.UI.WPF.ViewModels;

public partial class RecentTradeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _tradeType = string.Empty; // "BUY" or "SELL"

    [ObservableProperty]
    private string _tradeTypeColor = string.Empty;

    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private string _amount = string.Empty;

    [ObservableProperty]
    private string _price = string.Empty;

    [ObservableProperty]
    private string _total = string.Empty;

    [ObservableProperty]
    private string _timestamp = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty; // "Completed", "Pending", "Failed"

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private bool _isTest;

    [ObservableProperty]
    private string _modeBadge = string.Empty;

    [ObservableProperty]
    private string _modeBadgeColor = string.Empty;

    [ObservableProperty]
    private string _portfolioName = string.Empty;

    [ObservableProperty]
    private string _exchange = string.Empty;

    public RecentTradeViewModel(string tradeType, string symbol, double amount, double price,
        DateTime timestamp, string status, bool isTest, string portfolioName, string exchange)
    {
        TradeType = tradeType;
        TradeTypeColor = tradeType == "BUY" ? "#4cceac" : "#db4f4a";

        Symbol = symbol;
        Amount = $"{amount:F4}";
        Price = $"${price:N2}";
        Total = $"${(amount * price):N2}";
        Timestamp = GetRelativeTime(timestamp);

        Status = status;
        StatusColor = status switch
        {
            "Completed" => "#4cceac",
            "Pending" => "#f59e0b",
            "Failed" => "#db4f4a",
            _ => "#8B8E98"
        };

        IsTest = isTest;
        ModeBadge = isTest ? "TEST" : "LIVE";
        ModeBadgeColor = isTest ? "#f59e0b" : "#4cceac";

        PortfolioName = portfolioName;
        Exchange = exchange;
    }

    private string GetRelativeTime(DateTime timestamp)
    {
        var diff = DateTime.Now - timestamp;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}d ago";

        return timestamp.ToString("MMM dd");
    }
}