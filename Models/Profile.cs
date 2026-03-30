using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocalSettingsSync.Models;

public class Profile : INotifyPropertyChanged
{
    private string _name = "Default";

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged();
        }
    }

    public string SourceFolder { get; set; } = string.Empty;
    public string TargetFolder { get; set; } = string.Empty;
    public List<string> FilterPatterns { get; set; } = ["!bin"];

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
