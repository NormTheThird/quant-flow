using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace QuantFlow.UI.WPF.ViewModels;

public partial class CryptoCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _quoteCurrency = "/USDT";

    [ObservableProperty]
    private string _priceFormatted = string.Empty;

    [ObservableProperty]
    private double _changePercent;

    [ObservableProperty]
    private string _changePercentFormatted = string.Empty;

    [ObservableProperty]
    private string _changeIndicator = string.Empty;

    [ObservableProperty]
    private string _changeColor = string.Empty;

    [ObservableProperty]
    private object _chartView = null!;

    public CryptoCardViewModel(string symbol, string name, double price, double changePercent, double[] chartData)
    {
        Symbol = symbol;
        Name = name;
        _changePercent = changePercent;

        // Format price
        PriceFormatted = price < 1
            ? $"${price:F4}"
            : price < 100
                ? $"${price:F2}"
                : $"${price:N2}";

        // Format change percent
        ChangePercentFormatted = $"{Math.Abs(changePercent):F2}%";
        ChangeIndicator = changePercent >= 0 ? "▲" : "▼";
        ChangeColor = changePercent >= 0 ? "#4cceac" : "#db4f4a";

        // Create chart
        ChartView = CreateChart(chartData, ChangeColor);
    }

    private CartesianChart CreateChart(double[] values, string colorHex)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);

        var seriesCollection = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<double>(values),
                Stroke = new SolidColorBrush(color),
                Fill = Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometry = null,
                LineSmoothness = 0.5
            }
        };

        return new CartesianChart
        {
            Series = seriesCollection,
            Hoverable = false,
            DisableAnimations = false,
            AxisX = new LiveCharts.Wpf.AxesCollection
            {
                new LiveCharts.Wpf.Axis { ShowLabels = false, IsEnabled = false }
            },
            AxisY = new LiveCharts.Wpf.AxesCollection
            {
                new LiveCharts.Wpf.Axis { ShowLabels = false, IsEnabled = false }
            }
        };
    }
}