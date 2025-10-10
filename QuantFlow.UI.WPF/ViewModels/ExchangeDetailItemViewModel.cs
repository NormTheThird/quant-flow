namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying a single exchange detail item
/// </summary>
public partial class ExchangeDetailItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _keyName = string.Empty;

    [ObservableProperty]
    private string _keyValue = string.Empty;

    [ObservableProperty]
    private bool _isEncrypted;

    [ObservableProperty]
    private string _encryptionBadge = string.Empty;

    [ObservableProperty]
    private string _encryptionBadgeColor = string.Empty;

    public ExchangeDetailItemViewModel(UserExchangeDetailsModel model)
    {
        Id = model.Id;
        KeyName = model.KeyName;
        KeyValue = model.KeyValue;
        IsEncrypted = model.IsEncrypted;

        EncryptionBadge = model.IsEncrypted ? "ENCRYPTED" : "PLAIN TEXT";
        EncryptionBadgeColor = model.IsEncrypted ? "#4cceac" : "#6b7280";
    }
}