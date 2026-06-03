# OpenSourceHub v1.2.2 Release Notes

**Release Date:** 2026-06-03  
**Publisher:** XinPari Software  
**License:** MIT

---

## What's New in v1.2.2

### Critical Bug Fixes

**Token Entry Crash — Fixed**

After entering a GitHub token and clicking Sign In, the application closed silently. Root cause:
- `SignInPage` subscribed to the `SignedIn` event and called `NavigationService.GoBack()`.
- `MainWindow` had already subscribed to the same event and navigated to Dashboard.
- The `GoBack()` call navigated the Frame back to SignIn while `DashboardPage.OnInitialized` was still awaiting GitHub API calls — producing an unhandled exception on the dispatcher that silently killed the process.

**Thread Safety — Fixed**
- `MainViewModel.OnAuthStateChanged` now uses `Dispatcher.Invoke` to ensure GitHub auth callbacks from background threads are safely marshaled to the UI thread.

**Silent Crash Protection — Added**
- `App.xaml.cs` now registers `DispatcherUnhandledException` and `AppDomain.UnhandledException` handlers. Any future unhandled exception shows a visible error dialog instead of silently terminating the process.

**Page Load Safety — Fixed**
- Replaced `async void OnInitialized` with `Loaded` event handler in all auto-loading pages. Exceptions from async page initialization are now caught and logged rather than crashing the dispatcher.

### Performance Improvements

- Removed `DropShadowEffect` from all card styles and Dashboard stat cards — these were the primary source of UI lag (GPU shadow recomposition on every frame for every visible card).
- Removed `ShadowLG` from the main window border.
- Simplified page transition to a single `FadeIn(180ms)` instead of two simultaneous animations.

### App Icon — Added

Multi-resolution icon (256/64/48/32/16px) is now embedded in the executable and visible in Windows Explorer, taskbar, and the application title bar.

---

## Previous: v1.2.1

**Application Startup Crash — Fixed**

The application failed to launch on all machines due to a duplicate `MainWindow` class conflict:

- The project contained a stub `MainWindow.xaml` at the project root (namespace `OpenSourceHub.UI`) alongside the actual `Views/MainWindow.xaml` (namespace `OpenSourceHub.UI.Views`).
- C# resolves unqualified type names to the **current namespace first**, so `App.xaml.cs` was registering the empty stub in the DI container instead of the real window.
- The stub did not implement `IAppBootstrapper`, causing an `InvalidCastException` on every startup.

**Fix applied:**
- Removed the stub `MainWindow.xaml` and its code-behind from the project root.
- Added explicit using alias `MainWindow = OpenSourceHub.UI.Views.MainWindow` in `App.xaml.cs` to prevent any future ambiguity.

---

## Previous Release: v1.2.0 / v1.1.5

### Major Features

**Repository Health Analysis Engine**
- Comprehensive health scoring across 6 dimensions
- Activity, Maintenance, Security, Community, Popularity, and Health scores
- Documentation checklist (README, CONTRIBUTING, LICENSE, SECURITY, Code of Conduct)
- Actionable recommendations and warnings

**AI-Powered Insights**
- OpenAI GPT-4o-mini integration for cloud AI
- Ollama support for local, private AI processing
- Repository summaries, adoption recommendations, risk reports, improvement suggestions
- Free-form Q&A about any repository

**Security Center**
- Automated security risk detection
- Archived repository warnings
- Inactive project detection
- Missing license detection
- AI-generated risk reports

**Professional Reporting**
- PDF reports via QuestPDF
- Markdown reports for technical documentation
- HTML reports for web publishing
- Repository, Security, Organization, and Comparison report types

**Multilingual Support**
- English, Turkish, Russian — 100% coverage
- Instant switching without application restart
- All UI elements, notifications, and dialogs translated

### Technical
- Clean Architecture with 7 separate projects
- .NET 10 with WPF MVVM pattern
- Entity Framework Core + SQLite local database
- CommunityToolkit.Mvvm for reactive bindings
- DPAPI encryption for GitHub token storage
- Memory caching with configurable TTL
- Comprehensive logging system

---

## Installation

Download the appropriate package:
- `OpenSourceHub-v1.2.1-portable.zip` — No installation required
- `OpenSourceHub-v1.2.1-Setup.exe` — Windows installer

## Requirements

- Windows 10 version 1903 or later
- Windows 11 (any version)
- .NET 10 Desktop Runtime
- Internet connection for GitHub API
- GitHub Personal Access Token

## Known Limitations

- LiveChartsCore charts require compatible GPU drivers
- Ollama AI requires local Ollama installation
- GitHub API rate limits apply (5000 requests/hour authenticated)

---

*© 2026 XinPari Software. All rights reserved.*
