using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using LocalSettingsSync.Models;

namespace LocalSettingsSync.Services;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
}

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = Path.Combine(appData, "LocalSettingsSync", "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var node = JsonNode.Parse(json);

            // Migrate old format: had SourceFolder/TargetFolder/FilterPatterns at root
            if (node?["Profiles"] == null)
            {
                var oldSource = node?["SourceFolder"]?.GetValue<string>() ?? string.Empty;
                var oldTarget = node?["TargetFolder"]?.GetValue<string>() ?? string.Empty;
                var oldPatterns = node?["FilterPatterns"]?.AsArray()
                    .Select(p => p?.GetValue<string>() ?? string.Empty)
                    .ToList() ?? new List<string>();

                return new AppSettings
                {
                    ActiveProfileName = "Default",
                    Profiles = new List<Profile>
                    {
                        new Profile
                        {
                            Name = "Default",
                            SourceFolder = oldSource,
                            TargetFolder = oldTarget,
                            FilterPatterns = oldPatterns
                        }
                    }
                };
            }

            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var dir = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
