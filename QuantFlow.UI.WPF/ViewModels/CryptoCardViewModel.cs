namespace QuantFlow.UI.WPF.ViewModels;

public partial class CryptoCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _quoteCurrency = string.Empty;

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

    [ObservableProperty]
    private string _exchange = string.Empty;

    [ObservableProperty]
    private bool _isSupported = true;

    [ObservableProperty]
    private string _unsupportedMessage = string.Empty;

    public CryptoCardViewModel(SymbolModel symbolInfo, double price, double changePercent, double[] chartData, string exchange, bool isSupported = true)
    {
        Symbol = symbolInfo.BaseAsset;
        Name = GetCryptoName(symbolInfo.BaseAsset);
        Exchange = exchange;
        IsSupported = isSupported;
        _changePercent = changePercent;
        _quoteCurrency = $"/{symbolInfo.QuoteAsset}";

        if (!isSupported)
        {
            PriceFormatted = "N/A";
            ChangePercentFormatted = "N/A";
            ChangeIndicator = "";
            ChangeColor = "#666666";
            UnsupportedMessage = "Not supported by exchange";
            ChartView = CreateEmptyChart();
        }
        else
        {
            // Format price
            PriceFormatted = price < 1
                ? $"${price:F4}"
                : price < 100
                    ? $"${price:F2}"
                    : $"${price:N2}";

            ChangePercentFormatted = $"{Math.Abs(changePercent):F2}%";
            ChangeIndicator = changePercent >= 0 ? "▲" : "▼";
            ChangeColor = changePercent >= 0 ? "#4cceac" : "#db4f4a";
            ChartView = CreateChart(chartData, ChangeColor);
        }
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

    private CartesianChart CreateEmptyChart()
    {
        var color = (Color)ColorConverter.ConvertFromString("#666666");

        var seriesCollection = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<double>(new double[7]),
                Stroke = new SolidColorBrush(color),
                Fill = Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometry = null,
                LineSmoothness = 0
            }
        };

        return new CartesianChart
        {
            Series = seriesCollection,
            Hoverable = false,
            DisableAnimations = true,
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

    private string GetCryptoName(string symbol)
    {
        return symbol switch
        {
            "BTC" => "Bitcoin",
            "ETH" => "Ethereum",
            "ADA" => "Cardano",
            "SOL" => "Solana",
            "DOT" => "Polkadot",
            "FET" => "Fetch.ai",
            "MATIC" => "Polygon",
            "AVAX" => "Avalanche",
            "LINK" => "Chainlink",
            "UNI" => "Uniswap",
            _ => symbol
        };
    }
}