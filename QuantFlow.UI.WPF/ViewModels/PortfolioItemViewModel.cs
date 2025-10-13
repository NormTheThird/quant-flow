namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying a single portfolio item in the list
/// </summary>
public partial class PortfolioItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _hasDescription;

    [ObservableProperty]
    private string _modeText = string.Empty;

    [ObservableProperty]
    private string _modeBadgeColor = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _statusBadgeColor = string.Empty;

    [ObservableProperty]
    private string _exchangeName = string.Empty;

    [ObservableProperty]
    private string _initialBalanceFormatted = string.Empty;

    [ObservableProperty]
    private string _currentBalanceFormatted = string.Empty;

    [ObservableProperty]
    private string _createdAtFormatted = string.Empty;

    public PortfolioItemViewModel(PortfolioModel portfolio)
    {
        Id = portfolio.Id;
        Name = portfolio.Name;
        Description = portfolio.Description;
        HasDescription = !string.IsNullOrWhiteSpace(portfolio.Description);

        // Mode
        ModeText = portfolio.Mode == PortfolioMode.TestMode ? "TEST MODE" : "EXCHANGE";
        ModeBadgeColor = portfolio.Mode == PortfolioMode.TestMode ? "#f59e0b" : "#3b82f6";

        // Status
        StatusText = portfolio.Status.ToString().ToUpper();
        StatusBadgeColor = portfolio.Status switch
        {
            Status.Active => "#4cceac",
            Status.Inactive => "#6b7280",
            Status.Paused => "#f59e0b",
            Status.Archived => "#94a3b8",
            _ => "#6b7280"
        };

        // Exchange
        ExchangeName = portfolio.Exchange.ToString();

        // Balances
        InitialBalanceFormatted = $"${portfolio.InitialBalance:N2}";
        CurrentBalanceFormatted = $"${portfolio.CurrentBalance:N2}";

        // Created date
        CreatedAtFormatted = portfolio.CreatedAt.ToString("MMM dd, yyyy");
    }
}