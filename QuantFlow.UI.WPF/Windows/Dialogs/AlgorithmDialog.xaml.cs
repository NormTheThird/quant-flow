namespace QuantFlow.UI.WPF.Windows.Dialogs;

public partial class AlgorithmDialog : Window
{
    public AlgorithmDialog()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}