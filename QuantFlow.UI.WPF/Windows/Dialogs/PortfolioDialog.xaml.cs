namespace QuantFlow.UI.WPF.Windows.Dialogs;

public partial class PortfolioDialog : Window
{
    public PortfolioDialog()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}