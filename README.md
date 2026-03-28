# LocalSettingsSync

A lightweight WPF desktop utility for .NET developers to back up and restore local configuration files across machines or team members.

## What It Does

When working in a team, files like `appsettings.local.json` are typically git-ignored but still need to be shared or preserved. LocalSettingsSync lets you:

- **Profiles** — manage multiple named profiles, each with its own source folder, target folder, and filter patterns. Profiles can be created, renamed, and deleted.
- **Backup** — scan a solution folder for files matching your patterns and copy them to a backup location, preserving the original folder structure.
- **Restore** — copy backed-up files back to the source, with per-file conflict prompts. Choose to overwrite or skip per file, or apply the decision to all remaining conflicts at once.
- **Filter flexibly** — match files by exact name or regular expression, with comment support for documenting your patterns.

## Screenshots

> _Add screenshots here once the UI is finalized._

## Requirements

- Windows 10 or later
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Getting Started

### Build

```bash
git clone https://github.com/pir264/LocalSettingsSync.git
cd LocalSettingsSync
dotnet build LocalSettingsSync.sln
```

### Run

```bash
dotnet run --project LocalSettingsSync.csproj
```

Or open `LocalSettingsSync.sln` in Visual Studio 2022 and press **F5**.

## Usage

1. **Profile** — select an existing profile from the dropdown, or click **New** to create one. Each profile stores its own source folder, target folder, and filter patterns. Use **Rename** or **Delete** to manage profiles.
2. **Source Folder** — select the root of your .NET solution.
3. **Target Folder** — select a backup destination (local folder, network share, cloud-synced folder, etc.).
4. **Filter Patterns** — enter filenames or regex patterns, one per line. Lines starting with `#` are comments.

   ```
   # Exact filename match
   appsettings.local.json

   # Regex — any file ending in .local.json
   .*\.local\.json$

   # Regex — any secrets file
   secrets\..*
   ```

5. Click **Preview Matching Files** to see what will be included.
6. Click **Backup to Target** to copy the files.
7. Click **Restore to Source** to copy them back. When a file already exists at the destination you will be prompted with five options:
   - **Overwrite** — overwrite this file only.
   - **Overwrite All** — overwrite this and all remaining conflicting files without further prompts.
   - **Skip** — skip this file only.
   - **Skip All** — skip this and all remaining conflicting files without further prompts.
   - **Cancel** — stop the restore immediately.

All settings are saved automatically to `%AppData%\LocalSettingsSync\settings.json`.

## Project Structure

```
LocalSettingsSync/
├── Models/           # AppSettings, Profile, FileMatch
├── Services/         # SettingsService, FileScanner, FileCopier
├── ViewModels/       # MainViewModel, RelayCommand
├── Views/            # MainWindow.xaml, OverwritePromptDialog.xaml, ProfileNameDialog.xaml
├── App.xaml(.cs)     # Application entry point & dependency wiring
└── LocalSettingsSync.csproj
```

## Contributing

Pull requests are welcome. For significant changes, please open an issue first to discuss what you would like to change.

## License

[MIT](LICENSE)
