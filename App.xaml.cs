using System.Windows;
using LocalSettingsSync.Services;
using LocalSettingsSync.ViewModels;
using LocalSettingsSync.Views;

namespace LocalSettingsSync;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var settingsService = new SettingsService();
        var fileScanner = new FileScanner();
        var fileCopier = new FileCopier();
        var viewModel = new MainViewModel(settingsService, fileScanner, fileCopier);

        var mainWindow = new MainWindow { DataContext = viewModel };
        mainWindow.Show();
    }
}
