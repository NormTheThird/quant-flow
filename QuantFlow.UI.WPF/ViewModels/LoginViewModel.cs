namespace QuantFlow.UI.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IAuthenticationApiService _authService;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ICredentialStorageService _credentialStorage;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public AuthenticateResponse? LastAuthResult { get; private set; }

    public event EventHandler? LoginSuccessful;

    public LoginViewModel(ILogger<LoginViewModel> logger, IAuthenticationApiService authService, ITokenStorageService tokenStorage,
                          ICredentialStorageService credentialStorage)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
    }

    [RelayCommand]
    private async Task LoginAsync(object parameter)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter a username";
            return;
        }

        var password = (parameter as System.Windows.Controls.PasswordBox)?.Password;
        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter a password";
            return;
        }

        var result = await _authService.AuthenticateAsync(Username, password);

        if (result == null)
        {
            ErrorMessage = "Invalid username or password";
            return;
        }

        // Store the result
        LastAuthResult = result;

        // Store tokens
        _tokenStorage.StoreToken(result.Token, result.RefreshToken);

        // Save credentials
        _credentialStorage.SaveCredentials(Username, password);

        _logger.LogInformation("Login successful for user: {Username}", Username);

        // Raise event to notify login window
        LoginSuccessful?.Invoke(this, EventArgs.Empty);
    }
}