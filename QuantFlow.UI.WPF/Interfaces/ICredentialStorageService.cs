namespace QuantFlow.UI.WPF.Interfaces;

public interface ICredentialStorageService
{
    void SaveCredentials(string username, string password);
    (string? username, string? password) GetCredentials();
    void ClearCredentials();
    bool HasStoredCredentials();
}