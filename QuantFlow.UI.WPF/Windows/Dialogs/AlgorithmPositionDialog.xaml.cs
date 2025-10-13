namespace QuantFlow.UI.WPF.Windows.Dialogs;

/// <summary>
/// Interaction logic for AlgorithmPositionDialog.xaml
/// </summary>
public partial class AlgorithmPositionDialog : Window
{
    public AlgorithmPositionDialog()
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