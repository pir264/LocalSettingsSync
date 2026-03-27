# LocalSettingsSync

A lightweight WPF desktop utility for .NET developers to back up and restore local configuration files across machines or team members.

## What It Does

When working in a team, files like `appsettings.local.json` are typically git-ignored but still need to be shared or preserved. LocalSettingsSync lets you:

- **Backup** — scan a solution folder for files matching your patterns and copy them to a backup location, preserving the original folder structure.
- **Restore** — copy backed-up files back to the source, with per-file prompts when conflicts are detected.
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

1. **Source Folder** — select the root of your .NET solution.
2. **Target Folder** — select a backup destination (local folder, network share, cloud-synced folder, etc.).
3. **Filter Patterns** — enter filenames or regex patterns, one per line. Lines starting with `#` are comments.

   ```
   # Exact filename match
   appsettings.local.json

   # Regex — any file ending in .local.json
   .*\.local\.json$

   # Regex — any secrets file
   secrets\..*
   ```

4. Click **Preview Matching Files** to see what will be included.
5. Click **Backup to Target** to copy the files.
6. Click **Restore to Source** to copy them back. If a file already exists at the destination, you will be prompted to **Overwrite**, **Skip**, or **Cancel**.

Settings (source folder, target folder, filter patterns) are saved automatically to `%AppData%\LocalSettingsSync\settings.json`.

## Project Structure

```
LocalSettingsSync/
├── Models/           # AppSettings, FileMatch
├── Services/         # SettingsService, FileScanner, FileCopier
├── ViewModels/       # MainViewModel, RelayCommand
├── Views/            # MainWindow.xaml, OverwritePromptDialog.xaml
├── App.xaml(.cs)     # Application entry point & dependency wiring
└── LocalSettingsSync.csproj
```

## Contributing

Pull requests are welcome. For significant changes, please open an issue first to discuss what you would like to change.

## License

[MIT](LICENSE)
