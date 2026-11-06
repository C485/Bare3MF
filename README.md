# Bare3MF

> **Are you tired of 3MF files destroying your slicer configuration?**
>
> Say no more. Bare3MF removes everything that isn't needed and keeps your color configuration.

Bare3MF is a Windows desktop app that cleans bundled slicer profiles out of `.3mf` files. Share or archive a 3MF without carrying someone else's printer, filament, or print settings — while keeping the model, plate layout, and multi-color extruder assignments.

![Bare3MF screenshot](Assets/screenshot.png)

## Downloads

Download the latest build from the [Releases](https://github.com/C485/Bare3MF/releases/latest) page:

| Build | Requires .NET | Description |
| --- | --- | --- |
| **Standalone** | No | Self-contained single-file executable. Recommended for most users. |
| **Framework-dependent** | .NET 10 Desktop Runtime | Smaller single-file executable that uses the installed .NET runtime. |

Both builds are produced automatically for every `v*` release tag.

## Why Bare3MF?

Opening a shared 3MF can silently replace your carefully tuned slicer profiles. Bare3MF strips those bundled profiles so the file is safe to open, while preserving the model and its color separation.

- Removes bundled slicer, printer, and filament profile configs.
- Keeps model geometry and plate layout.
- Keeps per-object and per-part extruder assignments used by multi-color models.
- Works with single files, multiple files, and whole folders.
- Can overwrite files in place or save cleaned copies to another folder.
- Processes files entirely locally — your 3MF files never leave your PC.
- Checks GitHub Releases for updates and links directly to the latest release.

## What is removed

Bare3MF removes profile and config files inside the 3MF archive's `Metadata/` folder, including:

- `Metadata/project_settings.config` — printer, filament, and print profiles.
- Other `Metadata/*.config` profile files.

## What is kept

- `3D/3dmodel.model` — model geometry.
- `Metadata/model_settings.config` — objects, parts, plate layout, and extruder/color assignments.
- `Metadata/_model.config` — model metadata.
- `_rels/.rels`, `[Content_Types].xml`, thumbnails, and all other non-profile files.

> **Color note:** Bare3MF preserves the multi-color mapping (which part uses extruder 1, 2, etc.). The exact filament hex palette may be stored in the removed `project_settings.config`; if needed, reselect filament colors in your slicer without losing the color separation.

## Usage

1. Download the latest release or build `Bare3MF.exe` from source.
2. Launch the app.
3. Drag and drop one or more `.3mf` files, or a folder, onto the drop area.
   - You can also use **Open 3MF file(s)** or **Open folder**.
4. Choose:
   - **Overwrite existing** — replace the original file.
   - **Save to folder…** — write a cleaned copy elsewhere.
5. Optionally enable **Apply this choice to all remaining files**.
6. Follow the progress and results in the log.

Overwrite mode writes to a temporary file first and only replaces the original after the cleaned archive is complete.

## Requirements

- Windows 10 or Windows 11 (x64).
- **Standalone build:** no .NET runtime installation required.
- **Framework-dependent build:** requires the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).

## Build from source

Requirements: .NET 10 SDK.

```powershell
dotnet build
```

Publish the standalone single-file build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\standalone
```

Publish the framework-dependent single-file build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\publish\framework-dependent
```

Outputs:

```text
publish\standalone\Bare3MF.exe
publish\framework-dependent\Bare3MF.exe
```

## Automated releases

The workflow at `.github/workflows/release.yml` runs when a tag such as `v1.0.0` is pushed. It builds and publishes:

- `Bare3MF-v1.0.0-win-x64-standalone.zip`
- `Bare3MF-v1.0.0-win-x64-framework-dependent.zip`

The tag version is embedded in the executable so the in-app update check can compare it with the latest GitHub release.

## Updates

Bare3MF checks the latest GitHub release when it starts. If a newer version is available, the link in the top-right corner changes to **Update available: vX.Y.Z**; click it to open the release page. Click **Check for updates** at any time to check again.

## Project structure

```text
Bare3MF/
├── .github/
│   └── workflows/
│       └── release.yml
├── Assets/
│   ├── ico.ico
│   └── screenshot.png
├── Models/
│   └── UpdateCheckResult.cs
├── Services/
│   ├── AppVersion.cs
│   └── UpdateService.cs
├── Bare3MF.csproj
├── ConfigSanitizer.cs
├── MainForm.cs
├── Program.cs
└── README.md
```

## How it works

Bare3MF opens the `.3mf` file as a ZIP archive, identifies bundled slicer profile configs under `Metadata/`, and rebuilds the archive without them. Everything else is copied unchanged. The sanitizer preserves `Metadata/_model.config` and `Metadata/model_settings.config` so multi-color extruder mappings survive the cleanup.

## Credits

- Created by **Kamil Skoracki**
- Version **1.0.0**
- Icon: [3mf icons created by designhub - Flaticon](https://www.flaticon.com/free-icons/3mf)
