# Changelog

All notable changes to OpenSourceHub will be documented in this file.

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
