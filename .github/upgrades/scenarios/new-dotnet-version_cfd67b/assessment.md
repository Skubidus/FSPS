# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [FSPSLibrary\FSPSLibrary.csproj](#fspslibraryfspslibrarycsproj)
  - [FSPSWinUI\FSPSWinUI.csproj](#fspswinuifspswinuicsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 2 | All require upgrade |
| Total NuGet Packages | 5 | All compatible |
| Total Code Files | 14 |  |
| Total Code Files with Incidents | 5 |  |
| Total Lines of Code | 1167 |  |
| Total Number of Issues | 8 |  |
| Estimated LOC to modify | 6+ | at least 0,5% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [FSPSLibrary\FSPSLibrary.csproj](#fspslibraryfspslibrarycsproj) | net9.0-windows10.0.19041.0 | 🟢 Low | 0 | 0 |  | WinUI, Sdk Style = True |
| [FSPSWinUI\FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | net9.0-windows10.0.19041.0 | 🟢 Low | 0 | 6 | 6+ | WinForms, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 5 | 100,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 0 | 0,0% |
| ***Total NuGet Packages*** | ***5*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 6 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1600 |  |
| ***Total APIs Analyzed*** | ***1606*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| CommunityToolkit.Mvvm | 8.4.2 |  | [FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | ✅Compatible |
| Microsoft.Extensions.Configuration.Json | 10.0.5 |  | [FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | ✅Compatible |
| Microsoft.Extensions.DependencyInjection | 10.0.5 |  | [FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | ✅Compatible |
| Microsoft.Windows.SDK.BuildTools | 10.0.28000.1721 |  | [FSPSLibrary.csproj](#fspslibraryfspslibrarycsproj)<br/>[FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | ✅Compatible |
| Microsoft.WindowsAppSDK | 1.8.260317003 |  | [FSPSLibrary.csproj](#fspslibraryfspslibrarycsproj)<br/>[FSPSWinUI.csproj](#fspswinuifspswinuicsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Uri | 3 | 50,0% | Behavioral Change |
| M:System.Uri.#ctor(System.String) | 3 | 50,0% | Behavioral Change |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;FSPSLibrary.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
    P2["<b>📦&nbsp;FSPSWinUI.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
    P2 --> P1
    click P1 "#fspslibraryfspslibrarycsproj"
    click P2 "#fspswinuifspswinuicsproj"

```

## Project Details

<a id="fspslibraryfspslibrarycsproj"></a>
### FSPSLibrary\FSPSLibrary.csproj

#### Project Info

- **Current Target Framework:** net9.0-windows10.0.19041.0
- **Proposed Target Framework:** net10.0-windows10.0.22000.0
- **SDK-style**: True
- **Project Kind:** WinUI
- **Dependencies**: 0
- **Dependants**: 1
- **Number of Files**: 3
- **Number of Files with Incidents**: 1
- **Lines of Code**: 41
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P2["<b>📦&nbsp;FSPSWinUI.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
        click P2 "#fspswinuifspswinuicsproj"
    end
    subgraph current["FSPSLibrary.csproj"]
        MAIN["<b>📦&nbsp;FSPSLibrary.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
        click MAIN "#fspslibraryfspslibrarycsproj"
    end
    P2 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 5 |  |
| ***Total APIs Analyzed*** | ***5*** |  |

<a id="fspswinuifspswinuicsproj"></a>
### FSPSWinUI\FSPSWinUI.csproj

#### Project Info

- **Current Target Framework:** net9.0-windows10.0.19041.0
- **Proposed Target Framework:** net10.0-windows10.0.22000.0
- **SDK-style**: True
- **Project Kind:** WinForms
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 27
- **Number of Files with Incidents**: 4
- **Lines of Code**: 1126
- **Estimated LOC to modify**: 6+ (at least 0,5% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["FSPSWinUI.csproj"]
        MAIN["<b>📦&nbsp;FSPSWinUI.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
        click MAIN "#fspswinuifspswinuicsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;FSPSLibrary.csproj</b><br/><small>net9.0-windows10.0.19041.0</small>"]
        click P1 "#fspslibraryfspslibrarycsproj"
    end
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 6 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1595 |  |
| ***Total APIs Analyzed*** | ***1601*** |  |

