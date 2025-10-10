namespace QuantFlow.UI.WPF.Converters;

/// <summary>
/// Converts DialogMode enum to Visibility based on expected mode
/// </summary>
public class DialogModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DialogMode currentMode && parameter is string expectedMode)
        {
            if (Enum.TryParse<DialogMode>(expectedMode, out var expected))
            {
                return currentMode == expected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}