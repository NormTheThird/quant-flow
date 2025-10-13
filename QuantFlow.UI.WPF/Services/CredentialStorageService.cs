namespace QuantFlow.UI.WPF.Services;

public class CredentialStorageService : ICredentialStorageService
{
    private const string TARGET_NAME = "QuantFlow";

    public void SaveCredentials(string username, string password)
    {
        var credential = new Credential
        {
            Target = TARGET_NAME,
            Username = username,
            Password = password
        };
        credential.Save();
    }

    public (string? username, string? password) GetCredentials()
    {
        var credential = new Credential { Target = TARGET_NAME };
        return credential.Load() ? (credential.Username, credential.Password) : (null, null);
    }

    public void ClearCredentials()
    {
        var credential = new Credential { Target = TARGET_NAME };
        credential.Delete();
    }

    public bool HasStoredCredentials()
    {
        var credential = new Credential { Target = TARGET_NAME };
        return credential.Load();
    }
}