# Lenovo Control

Lenovo Control is a lightweight Windows system-tray utility for switching between Windows power profiles.

> This is an independent open-source project and is not affiliated with, endorsed by, or supported by Lenovo.

## Version

**0.1.0**

## Features

- Runs entirely in the Windows system tray
- Battery, Balanced and Performance profile selection
- Shows the currently active profile
- Uses native Windows power-management APIs
- Does not use powercfg.exe for profile switching
- No Windows service
- No telemetry
- No administrator privileges required for normal use
- Lenovo-specific profiles are detected instead of using machine-specific GUIDs

## Supported profiles

Lenovo Control currently looks for:

- Lenovo Battery
- Windows Balanced
- Lenovo Performance

If a Lenovo-specific profile is not installed, its menu entry is disabled.

Lenovo Control does not create, delete or modify power-plan settings.

## Requirements

- Windows 11
- x64
- .NET 8 Desktop Runtime
- Compatible power profiles already configured

## Current limitations

Version 0.1.0 does not include:

- CPU temperature monitoring
- Lenovo thermal-mode switching
- synchronization between Windows power plans and Lenovo thermal modes
- automatic startup with Windows
- custom tray icon

## Build

Build:

    dotnet build .\LenovoControl.sln -c Release

Publish a single-file x64 executable:

    dotnet publish .\src\LenovoControl\LenovoControl.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false

## License

MIT License.
