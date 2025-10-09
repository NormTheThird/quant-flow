namespace QuantFlow.UI.WPF;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = ConfigurationService.ConfigureServices();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Apply default theme immediately
        var themeService = _host.Services.GetRequiredService<IThemeService>();
        themeService.ApplyTheme("Midnight Pro");

        var credentialStorage = _host.Services.GetRequiredService<ICredentialStorageService>();
        var authService = _host.Services.GetRequiredService<IAuthenticationApiService>();
        var tokenStorage = _host.Services.GetRequiredService<ITokenStorageService>();

        // Check for stored credentials
        if (credentialStorage.HasStoredCredentials())
        {
            var (username, password) = credentialStorage.GetCredentials();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var result = await authService.AuthenticateAsync(username, password);

                if (result != null)
                {
                    tokenStorage.StoreToken(result.Token, result.RefreshToken);

                    // Ensure user preferences exist
                    _ = authService.ValidateUserPreferencesAsync(result.User.Id, result.Token);

                    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                    mainWindow.InitializeTopBar(result.User.Id, result.User.Username);
                    mainWindow.Show();
                    base.OnStartup(e);
                    return;
                }
            }
        }

        // Show login if no stored credentials or auto-login failed
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}