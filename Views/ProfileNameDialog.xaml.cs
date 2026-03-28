using System.Windows;
using System.Windows.Input;

namespace LocalSettingsSync.Views;

public partial class ProfileNameDialog : Window
{
    private readonly IReadOnlyCollection<string> _existingNames;

    public string? Result { get; private set; }

    public ProfileNameDialog(string? initialName = null, IReadOnlyCollection<string>? existingNames = null)
    {
        InitializeComponent();
        _existingNames = existingNames ?? Array.Empty<string>();
        if (initialName != null)
            NameBox.Text = initialName;
        Loaded += (_, _) => { NameBox.Focus(); NameBox.SelectAll(); };
    }

    private void OnOk(object sender, RoutedEventArgs e) => TryAccept();

    private void OnCancel(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            TryAccept();
    }

    private void TryAccept()
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ShowError("Name cannot be empty.");
            return;
        }
        if (_existingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            ShowError("A profile with this name already exists.");
            return;
        }
        Result = name;
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
