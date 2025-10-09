namespace QuantFlow.UI.WPF.Windows;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        _viewModel.LoginSuccessful += OnLoginSuccessful;
    }

    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        var result = _viewModel.LastAuthResult;
        if (result != null)
        {
            var authService = _serviceProvider.GetRequiredService<IAuthenticationApiService>();
            _ = authService.ValidateUserPreferencesAsync(result.User.Id, result.Token);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.InitializeTopBar(result.User.Id, result.User.Username);
            mainWindow.Show();
            this.Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}