namespace LocalSettingsSync.Models;

public class AppSettings
{
    public string SourceFolder { get; set; } = string.Empty;
    public string TargetFolder { get; set; } = string.Empty;
    public List<string> FilterPatterns { get; set; } = new();
}
