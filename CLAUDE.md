# LocalSettingsSync — AI Context

## Project Overview

**LocalSettingsSync** is a WPF desktop utility for .NET developers to back up and restore local configuration files (e.g., `appsettings.local.json`) from a solution folder to a backup location.

## Tech Stack

- **Language**: C# 12
- **Framework**: .NET 10.0 Windows (`net10.0-windows`)
- **UI**: WPF (Windows Presentation Foundation) with XAML
- **Pattern**: MVVM (Model-View-ViewModel)
- **Persistence**: JSON via `System.Text.Json` to `%AppData%\LocalSettingsSync\settings.json`

## Architecture

```
Models/       — Pure data classes (AppSettings, Profile, FileMatch)
Services/     — Business logic (SettingsService, FileScanner, FileCopier)
ViewModels/   — UI state and commands (MainViewModel, RelayCommand)
Views/        — XAML markup and code-behind (MainWindow, OverwritePromptDialog, ProfileNameDialog)
App.xaml.cs   — Application entry point; manual dependency injection
```

### Key Design Decisions

- **Interfaces for all services** (`ISettingsService`, `IFileScanner`, `IFileCopier`) — enables testability.
- **Constructor injection** in `App.xaml.cs` — services are wired manually, no DI container.
- **RelayCommand** wraps `Action`/`Func<bool>` — standard WPF command pattern.
- **Pattern matching**: each filter line is compiled as a case-insensitive regex; falls back to literal filename match if the regex is invalid. Lines starting with `#` are comments.
- **Conflict resolution** during restore uses a callback (`Func<FileMatch, string, ConflictResolution>`) so the ViewModel stays decoupled from the dialog. `OverwriteAll`/`SkipAll` are handled inside `FileCopier` via a `blanket` variable — once chosen, the callback is skipped for remaining conflicts.
- **Profiles**: `AppSettings` is the root JSON container holding a `List<Profile>` and `ActiveProfileName`. On first load of the old single-profile format, `SettingsService` auto-migrates to a "Default" profile.

## Build & Run

```bash
# Build
dotnet build LocalSettingsSync.sln

# Run
dotnet run --project LocalSettingsSync.csproj
```

Requires: .NET 10 SDK, Windows OS.

## Core Workflow

1. User selects or creates a **Profile** (name, source folder, target folder, filter patterns).
2. User selects **Source** folder (solution root) and **Target** folder (backup destination).
3. User enters filter patterns (filenames or regex) in the text area.
4. **Preview** — `FileScanner` recursively finds matches; results shown in `ListView`.
5. **Backup** — `FileCopier.CopyToTarget()` copies matched files, preserving relative paths.
6. **Restore** — `FileCopier.CopyToSource()` copies back; prompts via `OverwritePromptDialog` when a file already exists (Overwrite / Overwrite All / Skip / Skip All / Cancel).

## File Locations

| Purpose | Path |
|---|---|
| Project file | `LocalSettingsSync.csproj` |
| App entry point | `App.xaml.cs` |
| Root settings model | `Models/AppSettings.cs` |
| Profile model | `Models/Profile.cs` |
| File result model | `Models/FileMatch.cs` |
| Settings persistence | `Services/SettingsService.cs` |
| File scanning | `Services/FileScanner.cs` |
| File copying | `Services/FileCopier.cs` |
| Main UI logic | `ViewModels/MainViewModel.cs` |
| Main window | `Views/MainWindow.xaml` |
| Conflict dialog | `Views/OverwritePromptDialog.xaml` |
| Profile name dialog | `Views/ProfileNameDialog.xaml` |
| Runtime settings | `%AppData%\LocalSettingsSync\settings.json` |

## Conventions

- Nullable reference types are enabled — use `?` annotations correctly.
- `ObservableCollection<T>` for all list properties bound to the UI.
- All property setters in ViewModels call `OnPropertyChanged` and `SaveSettings()` automatically.
- Do not add a DI container — keep manual wiring in `App.xaml.cs`.
- Do not modify `bin/` or `obj/` — these are build artifacts.
