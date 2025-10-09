namespace QuantFlow.UI.WPF.Interfaces;

/// <summary>
/// Service for managing application theme settings
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme name for the specified user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The theme name</returns>
    Task<string> GetCurrentThemeAsync(Guid userId);

    /// <summary>
    /// Sets the theme for the specified user and applies it to the application
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="themeName">The theme name to apply</param>
    Task SetThemeAsync(Guid userId, string themeName);

    /// <summary>
    /// Applies the specified theme to the application immediately
    /// </summary>
    /// <param name="themeName">The theme name to apply</param>
    void ApplyTheme(string themeName);

    /// <summary>
    /// Gets a list of all available theme names
    /// </summary>
    /// <returns>List of theme names</returns>
    List<string> GetAvailableThemeNames();

    /// <summary>
    /// Event raised when the theme changes
    /// </summary>
    event EventHandler<string>? ThemeChanged;
}