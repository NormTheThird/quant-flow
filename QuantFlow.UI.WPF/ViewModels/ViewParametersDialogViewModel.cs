namespace QuantFlow.UI.WPF.ViewModels;

/// <summary>
/// View model for displaying algorithm parameters
/// </summary>
public partial class ViewParametersDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private List<ParameterDisplayItem> _parameters = [];

    public ViewParametersDialogViewModel(string parametersJson)
    {
        LoadParameters(parametersJson);
    }

    private void LoadParameters(string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson) || parametersJson == "{}")
        {
            Parameters = new List<ParameterDisplayItem>
            {
                new ParameterDisplayItem { Name = "No parameters", Value = "Default values used" }
            };
            return;
        }

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(parametersJson);

            if (dict != null)
            {
                Parameters = dict.Select(_ => new ParameterDisplayItem
                {
                    Name = FormatParameterName(_.Key),
                    Value = FormatParameterValue(_.Value)
                }).ToList();
            }
        }
        catch (Exception)
        {
            Parameters = new List<ParameterDisplayItem>
            {
                new ParameterDisplayItem { Name = "Error", Value = "Could not parse parameters" }
            };
        }
    }

    private static string FormatParameterName(string name)
    {
        // Convert "FastPeriod" to "Fast Period"
        return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    private static string FormatParameterValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => "Yes",
            JsonValueKind.False => "No",
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.String => value.GetString() ?? "",
            _ => value.ToString()
        };
    }
}

public class ParameterDisplayItem
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}