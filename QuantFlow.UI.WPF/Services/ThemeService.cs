namespace QuantFlow.UI.WPF.Services;

public class ThemeService : IThemeService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ThemeService> _logger;

    public event EventHandler<string>? ThemeChanged;

    public ThemeService(ILogger<ThemeService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<string> GetCurrentThemeAsync(Guid userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var userPreferencesRepository = scope.ServiceProvider.GetRequiredService<IUserPreferencesRepository>();
        var preferences = await userPreferencesRepository.GetByUserIdAsync(userId);
        return preferences?.Theme ?? "DevExpress Light";
    }

    public async Task SetThemeAsync(Guid userId, string themeName)
    {
        using var scope = _serviceProvider.CreateScope();
        var userPreferencesRepository = scope.ServiceProvider.GetRequiredService<IUserPreferencesRepository>();
        await userPreferencesRepository.UpdateSectionAsync(userId, "theme", themeName);

        ApplyTheme(themeName);

        _logger.LogInformation("Theme changed to {ThemeName} for user {UserId}", themeName, userId);
    }

    public void ApplyTheme(string themeName)
    {
        // Map old theme names to new theme names
        themeName = themeName switch
        {
            "dark" => "Midnight Pro",
            "light" => "DevExpress Light",
            "Light" => "DevExpress Light",
            "Coffee Bean" => "DevExpress Light",
            "Forest Dawn" => "Midnight Pro",
            _ => themeName
        };

        var themes = ThemeProvider.GetAvailableThemes();
        var theme = themes.FirstOrDefault(t => t.Name == themeName) ?? themes.First();

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Base colors
            Application.Current.Resources["PrimaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Primary));
            Application.Current.Resources["SecondaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Secondary));
            Application.Current.Resources["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Accent));
            Application.Current.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Background));
            Application.Current.Resources["SurfaceColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Surface));
            Application.Current.Resources["TextPrimaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.TextPrimary));
            Application.Current.Resources["TextSecondaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.TextSecondary));

            // Derived colors
            Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Border));
            Application.Current.Resources["ShadowColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Shadow));
            Application.Current.Resources["HoverColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Hover));
            Application.Current.Resources["SidebarHoverColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.SidebarHover));
        });

        _logger.LogInformation("Theme resources updated successfully to {ThemeName}", themeName);

        ThemeChanged?.Invoke(this, themeName);
    }

    public List<string> GetAvailableThemeNames()
    {
        return ThemeProvider.GetAvailableThemes().Select(t => t.Name).ToList();
    }
}