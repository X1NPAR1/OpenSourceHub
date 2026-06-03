# Changelog

All notable changes to OpenSourceHub will be documented in this file.

## [1.2.4] - 2026-06-03

### Fixed
- **XamlParseException "FrameworkElement.Style"**: `SettingsCardStyle` had `Effect="{StaticResource ShadowSM}"` — WPF cannot share a single `DropShadowEffect` Freezable instance across multiple elements when all apply it via the same style setter at the same time. Removed the shadow from `SettingsCardStyle`.
- **Localization keys not resolving in Settings page**: Three wrong keys corrected — `Settings.Reports` → `Settings.ReportsPath`, `Settings.AutoUpdate` → `Settings.EnableAutoUpdate`, `Settings.Telemetry` → `Settings.EnableTelemetry`.
- **Hardcoded "Version 1.1.5" in Settings About**: Replaced with dynamic binding to `SettingsViewModel.AppVersion` (read from `Assembly.GetExecutingAssembly().GetName().Version`).

### Added
- **German (Deutsch) language support**: Full translation of all 300+ localization keys.
- **Dutch (Nederlands) language support**: Full translation of all 300+ localization keys.
- Language picker in Settings now shows 5 options: English, Türkçe, Русский, Deutsch, Nederlands.
- `AppLanguage` enum extended with `German` and `Dutch` values.

---

## [1.2.3] - 2026-06-03

### Fixed
- **XamlParseException on startup**: `Icon="/Assets/AppIcon.ico"` caused `TypeConverterMarkupExtension` failure because WPF's BAML ICO decoder rejects PNG-compressed ICO (Vista+ format). Fixed by:
  - Regenerating `AppIcon.ico` with proper DIB/BMP format (6 sizes: 256/128/64/48/32/16px, 370KB).
  - Switching `Window.Icon` in `MainWindow.xaml` from `.ico` to `/Assets/AppIcon.png` — WPF loads PNG icons natively without issues.
  - `ApplicationIcon` in `.csproj` still points to `AppIcon.ico` so the exe file icon in Windows Explorer works correctly.
- Removed conflicting `<None Update>` item from `.csproj` that could shadow the `<Resource Include="Assets\**">` entry.

---

## [1.2.2] - 2026-06-03

### Fixed
- **Token entry crash**: Removed duplicate `SignedIn` event handler in `SignInPage` that called `NavigationService.GoBack()` after login, causing the Frame to navigate back to SignIn while `DashboardPage.OnInitialized` was still running — resulting in a silent app crash.
- **Thread safety**: Added `Dispatcher.Invoke` guard in `MainViewModel.OnAuthStateChanged` so GitHub auth state updates from background threads are always marshaled to the UI thread.
- **Silent crashes**: Added `DispatcherUnhandledException` and `AppDomain.UnhandledException` handlers in `App.xaml.cs` — unhandled exceptions now show an error dialog instead of silently terminating the process.
- **Page load crashes**: Replaced `async void OnInitialized` with `Page.Loaded` event in all 5 auto-loading pages — exceptions from async page loads are now properly caught and logged instead of crashing the dispatcher.

### Performance
- Removed `DropShadowEffect` from all card styles (`CardStyle`, `CardElevatedStyle`, `GlassCardStyle`) and Dashboard stat cards — these GPU effects were recalculated on every render frame and were the primary source of lag.
- Removed `ShadowLG` from the main window outer border.
- Simplified page transition animation from `FadeIn + SlideInFromBottom` (two simultaneous animations) to a single `FadeIn(180ms)`.

### Added
- **Application icon**: Multi-resolution `AppIcon.ico` (256/64/48/32/16px) embedded in exe and shown in taskbar/title bar — resolves missing icon in Explorer and taskbar.

---

## [1.2.1] - 2026-06-03

### Fixed
- **Critical startup crash**: Removed duplicate stub `MainWindow.xaml` from project root that conflicted with the actual `Views/MainWindow.xaml`. C# namespace resolution was selecting the stub (which did not implement `IAppBootstrapper`), causing an `InvalidCastException` on every launch.
- Added explicit using alias `MainWindow = OpenSourceHub.UI.Views.MainWindow` in `App.xaml.cs` to guarantee correct type resolution.

### Changed
- Version bump to 1.2.1
- Updated `ProductHeaderValue` in `GitHubAuthService` to reflect new version

---

## [1.2.0] - 2026-06-03

### Added
- Complete repository health scoring system (6 dimensions)
- AI-powered insights with OpenAI and Ollama support
- Security Center with categorized risk alerts
- Repository comparison (up to 4 repositories side-by-side)
- Trend Explorer with language and period filtering
- Organization analytics with member and repository views
- Professional report generation (PDF, Markdown, HTML)
- Favorites system with note annotation
- Application logging with export support
- Complete multilingual support (English, Turkish, Russian)
- Instant language switching without restart
- Modern Fluent Design dark theme
- Custom title bar and window controls
- Notification system (Success, Warning, Error, Info)
- Smart caching with configurable duration
- GitHub OAuth via Personal Access Token (DPAPI encrypted)
- Async operations throughout — no UI blocking

### Changed
- Initial release

### Fixed
- N/A (initial release)

## [1.1.5] - 2026-06-03

### Added
- Complete repository health scoring system (6 dimensions)
- AI-powered insights with OpenAI and Ollama support
- Security Center with categorized risk alerts
- Repository comparison (up to 4 repositories side-by-side)
- Trend Explorer with language and period filtering
- Organization analytics with member and repository views
- Professional report generation (PDF, Markdown, HTML)
- Favorites system with note annotation
- Application logging with export support
- Complete multilingual support (English, Turkish, Russian)
- Instant language switching without restart
- Modern Fluent Design dark theme
- Custom title bar and window controls
- Notification system (Success, Warning, Error, Info)
- Smart caching with configurable duration
- GitHub OAuth via Personal Access Token (DPAPI encrypted)
- Async operations throughout — no UI blocking

---

## [1.0.0] - 2026-01-01

### Added
- Initial project scaffolding
