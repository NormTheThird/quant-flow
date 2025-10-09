namespace QuantFlow.UI.WPF.Controls;

public partial class LoadingSpinner : UserControl
{
    public static readonly DependencyProperty SpinnerSizeProperty =
        DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(LoadingSpinner), new PropertyMetadata(48.0));

    public double SpinnerSize
    {
        get => (double)GetValue(SpinnerSizeProperty);
        set => SetValue(SpinnerSizeProperty, value);
    }

    public LoadingSpinner()
    {
        InitializeComponent();
    }
}