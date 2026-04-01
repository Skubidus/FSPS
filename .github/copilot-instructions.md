# Copilot Instructions for FSPS

## Project Overview

FSPS (Farming Simulator Profile Switcher) is a WinUI 3 desktop application targeting .NET 9.0 on Windows. It is structured as two projects:

- **FSPSLibrary** – Class library containing models and business logic. Referenced by the UI project.
- **FSPSWinUI** – WinUI 3 application with XAML views and ViewModels. Packaged as MSIX.

## Build Commands

```shell
# Build the full solution (default platform is Any CPU; WinUI requires a specific platform)
dotnet build -p:Platform=x64

# Build for other platforms
dotnet build -p:Platform=x86
dotnet build -p:Platform=ARM64

# Run the app (unpackaged mode)
dotnet run --project FSPSWinUI

# Publish
dotnet publish -p:Platform=x64
```

There are no test projects. The app can be launched from Visual Studio using either the **FSPSWinUI (Package)** (MSIX) or **FSPSWinUI (Unpackaged)** launch profile.

## Architecture

### MVVM with CommunityToolkit.Mvvm

- ViewModels live in `FSPSWinUI/ViewModels/` and inherit `ObservableObject`.
- Use `[ObservableProperty]` for reactive properties and `[RelayCommand]` for commands — the toolkit generates the boilerplate via source generators.
- Views live in `FSPSWinUI/Views/` (currently empty). `MainWindow.xaml` acts as the shell.
- ViewModels are manually instantiated in code-behind (no DI container).
- DataContext is set in code-behind, not in XAML.

### Models

- Plain sealed C# classes live in `FSPSLibrary/Models/`. No DTOs or repository pattern currently used.

### Configuration

- App settings are loaded from `FSPSWinUI/appsettings.json` at runtime with graceful fallback (e.g., `App.Title`).

## Key Conventions

### Naming

- Private fields: `_camelCase` with leading underscore (e.g., `_selectedProfile`, `_viewModel`).
- Properties and methods: `PascalCase`.
- File-scoped namespaces are used everywhere (`namespace FSPSWinUI;`).

### Global Usings

Each project has a `GlobalUsings.cs`. CommunityToolkit namespaces are **intentionally excluded** from global usings — add them explicitly at the file level to avoid ambiguity.

### WinUI 3 Patterns

- The app uses **Mica backdrop** (`MicaController`) for the Fluent Design background.
- Window size is set via the **AppWindow API** (`AppWindow.Resize`), not `Window.Width/Height`.
- The title bar is customized via XAML (`ExtendsContentIntoTitleBar = true`), with a custom `AppTitleBar` element in `MainWindow.xaml`.
- DPI awareness is configured as `PerMonitorV2` in `app.manifest`.

### C# Language

- C# 14, nullable reference types enabled (`<Nullable>enable</Nullable>`).
- `var` only when the type is apparent from the right-hand side (enforced by `.editorconfig`).
- CRLF line endings, UTF-8 encoding (enforced by `.editorconfig`).
