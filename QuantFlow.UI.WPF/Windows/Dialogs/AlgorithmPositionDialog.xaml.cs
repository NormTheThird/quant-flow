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

    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AlgorithmPositionDialogViewModel viewModel &&
            !string.IsNullOrWhiteSpace(viewModel.ValidationMessage))
        {
            try
            {
                Clipboard.SetText(viewModel.ValidationMessage);

                // Optional: Show brief feedback
                if (sender is Button button)
                {
                    var originalToolTip = button.ToolTip;
                    button.ToolTip = "Copied!";

                    // Reset tooltip after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        button.ToolTip = originalToolTip;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch
            {
                // Clipboard access failed - ignore silently
            }
        }
    }
}