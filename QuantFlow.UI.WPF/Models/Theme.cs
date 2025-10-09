namespace QuantFlow.UI.WPF.Models;

/// <summary>
/// Represents a UI theme with base and derived colors
/// </summary>
public class Theme
{
    public string Name { get; set; } = string.Empty;

    // Base colors (user-defined)
    public string Primary { get; set; } = string.Empty;
    public string Secondary { get; set; } = string.Empty;
    public string Accent { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string Surface { get; set; } = string.Empty;
    public string TextPrimary { get; set; } = string.Empty;
    public string TextSecondary { get; set; } = string.Empty;

    // Derived colors (automatically calculated)
    public string Border => DeriveColor(Secondary, 0.3);
    public string Shadow => "#000000";
    public string Hover => DeriveColor(Secondary, 0.8);
    public string SidebarHover => DeriveColor(Primary, 1.2);

    /// <summary>
    /// Derives a color by adjusting brightness
    /// </summary>
    private string DeriveColor(string hexColor, double factor)
    {
        try
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);

            byte Adjust(byte value) => (byte)Math.Min(255, Math.Max(0, value * factor));

            var adjusted = System.Windows.Media.Color.FromRgb(
                Adjust(color.R),
                Adjust(color.G),
                Adjust(color.B)
            );

            return $"#{adjusted.R:X2}{adjusted.G:X2}{adjusted.B:X2}";
        }
        catch
        {
            return hexColor;
        }
    }
}

/// <summary>
/// Provides predefined themes
/// </summary>
public static class ThemeProvider
{
    public static List<Theme> GetAvailableThemes() => new()
    {
        new Theme
        {
            Name = "DevExpress Light",
            Primary = "#FFFFFF",
            Secondary = "#F7F7F7",
            Accent = "#0078D4",
            Background = "#FAFAFA",
            Surface = "#FFFFFF",
            TextPrimary = "#1A1A1A",
            TextSecondary = "#666666"
        },
        new Theme
        {
            Name = "Clean Light",
            Primary = "#F5F5F5",
            Secondary = "#EBEBEB",
            Accent = "#2196F3",
            Background = "#FFFFFF",
            Surface = "#F9F9F9",
            TextPrimary = "#212121",
            TextSecondary = "#757575"
        },
        new Theme
        {
            Name = "Midnight Pro",
            Primary = "#1A1D29",
            Secondary = "#252834",
            Accent = "#00D9C0",
            Background = "#0F1117",
            Surface = "#1E2130",
            TextPrimary = "#E8E9ED",
            TextSecondary = "#8B8E98"
        }
    };
}