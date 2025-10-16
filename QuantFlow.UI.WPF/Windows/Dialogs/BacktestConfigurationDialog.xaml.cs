namespace QuantFlow.UI.WPF.Windows.Dialogs;

/// <summary>
/// Interaction logic for BacktestConfigurationDialog
/// </summary>
public partial class BacktestConfigurationDialog : Window
{
    public BacktestConfigurationDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}