namespace QuantFlow.UI.WPF.Windows.Dialogs;

/// <summary>
/// Interaction logic for ViewParametersDialog.xaml
/// </summary>
public partial class ViewParametersDialog : Window
{
    public ViewParametersDialog()
    {
        InitializeComponent();

        // Center the window manually
        Loaded += (s, e) =>
        {
            var workArea = SystemParameters.WorkArea;
            Left = (workArea.Width - Width) / 2 + workArea.Left;
            Top = (workArea.Height - Height) / 2 + workArea.Top;
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}