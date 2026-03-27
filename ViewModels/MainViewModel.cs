using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using LocalSettingsSync.Models;
using LocalSettingsSync.Services;
using LocalSettingsSync.Views;
using Microsoft.Win32;

namespace LocalSettingsSync.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly IFileScanner _fileScanner;
    private readonly IFileCopier _fileCopier;

    private string _sourceFolder = string.Empty;
    private string _targetFolder = string.Empty;
    private string _filterPatternsText = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "Ready.";

    public MainViewModel(ISettingsService settingsService, IFileScanner fileScanner, IFileCopier fileCopier)
    {
        _settingsService = settingsService;
        _fileScanner = fileScanner;
        _fileCopier = fileCopier;

        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseTargetCommand = new RelayCommand(BrowseTarget);
        ScanCommand = new RelayCommand(Scan, CanScan);
        BackupCommand = new RelayCommand(Backup, CanBackup);
        RestoreCommand = new RelayCommand(Restore, CanRestore);

        var settings = _settingsService.Load();
        _sourceFolder = settings.SourceFolder;
        _targetFolder = settings.TargetFolder;
        _filterPatternsText = string.Join(Environment.NewLine, settings.FilterPatterns);
    }

    public string SourceFolder
    {
        get => _sourceFolder;
        set
        {
            if (_sourceFolder == value) return;
            _sourceFolder = value;
            OnPropertyChanged();
            SaveSettings();
            RaiseCommandsCanExecuteChanged();
        }
    }

    public string TargetFolder
    {
        get => _targetFolder;
        set
        {
            if (_targetFolder == value) return;
            _targetFolder = value;
            OnPropertyChanged();
            SaveSettings();
            RaiseCommandsCanExecuteChanged();
        }
    }

    public string FilterPatternsText
    {
        get => _filterPatternsText;
        set
        {
            if (_filterPatternsText == value) return;
            _filterPatternsText = value;
            OnPropertyChanged();
            SaveSettings();
            RaiseCommandsCanExecuteChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            RaiseCommandsCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public ObservableCollection<FileMatch> PreviewFiles { get; } = new();

    public RelayCommand BrowseSourceCommand { get; }
    public RelayCommand BrowseTargetCommand { get; }
    public RelayCommand ScanCommand { get; }
    public RelayCommand BackupCommand { get; }
    public RelayCommand RestoreCommand { get; }

    private void BrowseSource()
    {
        var folder = BrowseFolder("Select source folder (your .NET solution root)");
        if (folder != null)
        {
            SourceFolder = folder;
            PreviewFiles.Clear();
            StatusMessage = "Source folder updated. Click 'Preview' to scan.";
        }
    }

    private void BrowseTarget()
    {
        var folder = BrowseFolder("Select target (backup) folder");
        if (folder != null)
        {
            TargetFolder = folder;
            PreviewFiles.Clear();
            StatusMessage = "Target folder updated.";
        }
    }

    private void Scan()
    {
        PreviewFiles.Clear();
        var patterns = ParsePatterns();
        if (patterns.Count == 0)
        {
            StatusMessage = "No filter patterns entered.";
            return;
        }

        var matches = _fileScanner.Scan(SourceFolder, patterns);
        foreach (var m in matches)
            PreviewFiles.Add(m);

        StatusMessage = matches.Count == 0
            ? "No matching files found."
            : $"Found {matches.Count} matching file(s).";

        RaiseCommandsCanExecuteChanged();
    }

    private bool CanScan() => !IsBusy && !string.IsNullOrWhiteSpace(SourceFolder) && !string.IsNullOrWhiteSpace(FilterPatternsText);

    private void Backup()
    {
        if (PreviewFiles.Count == 0)
        {
            StatusMessage = "Nothing to backup. Run Preview first.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Backing up...";
        try
        {
            var count = _fileCopier.CopyToTarget(PreviewFiles.ToList(), TargetFolder);
            StatusMessage = $"Backup complete. {count} file(s) copied to target.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Backup failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanBackup() => !IsBusy && PreviewFiles.Count > 0 && !string.IsNullOrWhiteSpace(TargetFolder);

    private void Restore()
    {
        var patterns = ParsePatterns();
        if (patterns.Count == 0)
        {
            StatusMessage = "No filter patterns entered.";
            return;
        }

        // Scan the target folder for files that match the patterns
        var filesToRestore = _fileScanner.Scan(TargetFolder, patterns);
        if (filesToRestore.Count == 0)
        {
            StatusMessage = "No matching files found in target folder.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Restoring...";
        try
        {
            var (count, cancelled) = _fileCopier.CopyToSource(
                filesToRestore,
                SourceFolder,
                (file, destination) =>
                {
                    var dialog = new OverwritePromptDialog(destination)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    dialog.ShowDialog();
                    return dialog.Result;
                });

            StatusMessage = cancelled
                ? $"Restore cancelled. {count} file(s) restored before cancellation."
                : $"Restore complete. {count} file(s) restored to source.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Restore failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRestore() => !IsBusy && !string.IsNullOrWhiteSpace(SourceFolder) && !string.IsNullOrWhiteSpace(TargetFolder) && !string.IsNullOrWhiteSpace(FilterPatternsText);

    private List<string> ParsePatterns()
    {
        return FilterPatternsText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith('#'))
            .ToList();
    }

    private void SaveSettings()
    {
        _settingsService.Save(new AppSettings
        {
            SourceFolder = SourceFolder,
            TargetFolder = TargetFolder,
            FilterPatterns = ParsePatterns()
        });
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        ScanCommand.RaiseCanExecuteChanged();
        BackupCommand.RaiseCanExecuteChanged();
        RestoreCommand.RaiseCanExecuteChanged();
    }

    private static string? BrowseFolder(string description)
    {
        var dialog = new OpenFolderDialog { Title = description };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
