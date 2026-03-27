namespace LocalSettingsSync.Models;

public class FileMatch
{
    public string AbsolutePath { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
