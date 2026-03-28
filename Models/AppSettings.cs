namespace LocalSettingsSync.Models;

public class AppSettings
{
    public List<Profile> Profiles { get; set; } = new() { new Profile() };
    public string ActiveProfileName { get; set; } = "Default";
}
