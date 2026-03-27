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
Models/       — Pure data classes (AppSettings, FileMatch)
Services/     — Business logic (SettingsService, FileScanner, FileCopier)
ViewModels/   — UI state and commands (MainViewModel, RelayCommand)
Views/        — XAML markup and code-behind (MainWindow, OverwritePromptDialog)
App.xaml.cs   — Application entry point; manual dependency injection
```

### Key Design Decisions

- **Interfaces for all services** (`ISettingsService`, `IFileScanner`, `IFileCopier`) — enables testability.
- **Constructor injection** in `App.xaml.cs` — services are wired manually, no DI container.
- **RelayCommand** wraps `Action`/`Func<bool>` — standard WPF command pattern.
- **Pattern matching**: each filter line is compiled as a case-insensitive regex; falls back to literal filename match if the regex is invalid. Lines starting with `#` are comments.
- **Conflict resolution** during restore uses a callback (`Func<string, ConflictResolution>`) so the ViewModel stays decoupled from the dialog.

## Build & Run

```bash
# Build
dotnet build LocalSettingsSync.sln

# Run
dotnet run --project LocalSettingsSync.csproj
```

Requires: .NET 10 SDK, Windows OS.

## Core Workflow

1. User selects **Source** folder (solution root) and **Target** folder (backup destination).
2. User enters filter patterns (filenames or regex) in the text area.
3. **Preview** — `FileScanner` recursively finds matches; results shown in `ListView`.
4. **Backup** — `FileCopier.CopyToTarget()` copies matched files, preserving relative paths.
5. **Restore** — `FileCopier.CopyToSource()` copies back; prompts via `OverwritePromptDialog` when a file already exists (Overwrite / Skip / Cancel).

## File Locations

| Purpose | Path |
|---|---|
| Project file | `LocalSettingsSync.csproj` |
| App entry point | `App.xaml.cs` |
| Settings model | `Models/AppSettings.cs` |
| File result model | `Models/FileMatch.cs` |
| Settings persistence | `Services/SettingsService.cs` |
| File scanning | `Services/FileScanner.cs` |
| File copying | `Services/FileCopier.cs` |
| Main UI logic | `ViewModels/MainViewModel.cs` |
| Main window | `Views/MainWindow.xaml` |
| Conflict dialog | `Views/OverwritePromptDialog.xaml` |
| Runtime settings | `%AppData%\LocalSettingsSync\settings.json` |

## Conventions

- Nullable reference types are enabled — use `?` annotations correctly.
- `ObservableCollection<T>` for all list properties bound to the UI.
- All property setters in ViewModels call `OnPropertyChanged` and `SaveSettings()` automatically.
- Do not add a DI container — keep manual wiring in `App.xaml.cs`.
- Do not modify `bin/` or `obj/` — these are build artifacts.
