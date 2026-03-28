using System.Windows;
using LocalSettingsSync.Services;

namespace LocalSettingsSync.Views;

public partial class OverwritePromptDialog : Window
{
    public ConflictResolution Result { get; private set; } = ConflictResolution.Cancel;

    public OverwritePromptDialog(string filePath)
    {
        InitializeComponent();
        FilePathText.Text = filePath;
    }

    private void OnOverwrite(object sender, RoutedEventArgs e)    { Result = ConflictResolution.Overwrite;    Close(); }
    private void OnOverwriteAll(object sender, RoutedEventArgs e) { Result = ConflictResolution.OverwriteAll; Close(); }
    private void OnSkip(object sender, RoutedEventArgs e)         { Result = ConflictResolution.Skip;         Close(); }
    private void OnSkipAll(object sender, RoutedEventArgs e)      { Result = ConflictResolution.SkipAll;      Close(); }
    private void OnCancel(object sender, RoutedEventArgs e)       { Result = ConflictResolution.Cancel;       Close(); }
}
