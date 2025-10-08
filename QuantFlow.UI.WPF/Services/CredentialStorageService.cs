namespace QuantFlow.UI.WPF.Services;

public class CredentialStorageService : ICredentialStorageService
{
    private const string TARGET_NAME = "QuantFlow";

    public void SaveCredentials(string username, string password)
    {
        using var credential = new Credential
        {
            Target = TARGET_NAME,
            Username = username,
            Password = password,
            PersistanceType = PersistanceType.LocalComputer
        };
        credential.Save();
    }

    public (string? username, string? password) GetCredentials()
    {
        using var credential = new Credential { Target = TARGET_NAME };
        if (credential.Load())
        {
            return (credential.Username, credential.Password);
        }
        return (null, null);
    }

    public void ClearCredentials()
    {
        using var credential = new Credential { Target = TARGET_NAME };
        credential.Delete();
    }

    public bool HasStoredCredentials()
    {
        using var credential = new Credential { Target = TARGET_NAME };
        return credential.Load();
    }
}