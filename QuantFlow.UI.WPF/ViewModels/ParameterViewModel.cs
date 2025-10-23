namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying and editing algorithm parameters
/// </summary>
public partial class ParameterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private int _type;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayValue))]
    private object _value = string.Empty;

    /// <summary>
    /// String representation of value for text binding
    /// </summary>
    public string DisplayValue
    {
        get => Value?.ToString() ?? string.Empty;
        set
        {
            // Store the raw string value to allow empty during editing
            if (Type == 1) // Integer
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Value = null;
                }
                else if (int.TryParse(value, out var intVal))
                {
                    Value = intVal;
                }
            }
            else if (Type == 2) // Decimal
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Value = null;
                }
                else if (decimal.TryParse(value, out var decVal))
                {
                    Value = decVal;
                }
            }
            else if (Type == 3) // Boolean
            {
                if (bool.TryParse(value, out var boolVal))
                    Value = boolVal;
            }
            else
            {
                Value = value;
            }
        }
    }
}