# OpenSourceHub Installer

A professional Windows setup wizard built with [Inno Setup](https://jrsoftware.org/isinfo.php) 6+.

## Build the installer

1. **Publish the app** (framework-dependent, x64):
   ```bash
   dotnet publish src/OpenSourceHub.UI/OpenSourceHub.UI.csproj \
     -c Release -r win-x64 --self-contained false -o release/output
   ```
2. **Install Inno Setup 6+** from https://jrsoftware.org/isdl.php
3. **Compile the script** (produces `release/installer/OpenSourceHub-<version>-Setup.exe`):
   ```powershell
   & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\OpenSourceHub.iss
   ```

## What the installer does

- Modern wizard UI, multi-language (EN/TR/RU/DE/NL)
- MIT license acceptance page
- Custom install path (defaults to Program Files)
- Start-menu shortcut + optional desktop shortcut
- App icon embedded; uninstall entry in Apps & Features
- Warns if the **.NET 10 Desktop Runtime (x64)** is missing (does not block)
- Optional "launch after install"

## Notes

- The app is **framework-dependent** — end users need the
  [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).
  For a self-contained installer, publish with `--self-contained true` and the
  installer will bundle the runtime (larger output).
