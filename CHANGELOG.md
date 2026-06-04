# Changelog

All notable changes to OpenSourceHub will be documented in this file.

## [1.3.6] - 2026-06-04

### Changed — Trending algorithm (Stage 3)
- **Smarter trending ranking**: instead of sorting purely by star count, the Trending page now pulls a 100-repo candidate pool and re-ranks it with a composite **trend score**:
  - **star velocity** (stars gained per day since creation) — surfaces fast-rising projects over old giants,
  - **fork engagement** (log-dampened fork weight),
  - **activity recency** (a bonus that decays the longer it's been since the last push, relative to the selected period).
- Covered by unit tests (`TrendScoreTests`): a young fast-growing repo outranks an old high-star one; recent pushes add a recency bonus; scores are non-negative. **38 tests passing.**

---

## [1.3.5] - 2026-06-04

### Tests (Stage 16)
- **Test suite restored to green and expanded — 35 passing, 0 failing.**
  - Fixed `SettingsServiceTests`: the service now takes an `IServiceScopeFactory`; added a `TestDb` helper that provides one backed by a unique in-memory database per test.
  - Added AI-provider tests: `AiServiceFactory` returns the right service for each of the 5 providers; `AiModelCatalog` has models for every provider; `IsAvailable` reflects whether an API key is configured.
  - Added Favorites category tests (assign, blank→General fallback, default category).
  - Persisted-settings test now also covers the new Claude/Gemini/DeepSeek fields.
  - Removed the placeholder `UnitTest1`.

---

## [1.3.4] - 2026-06-04

### Added — Light / Dark theme switching (Stage 12)
- **Runtime theme switch, no restart**: choosing Dark, Light, or System in Settings → Appearance now repaints the whole app instantly. Picking the theme applies it live (preview), and System follows the Windows app theme.
- **Implementation**: `ThemeManager` mutates the shared `SolidColorBrush` resources' `Color` in place. Because every control references the same brush instance via `StaticResource`, changing the brush color propagates to the entire UI live — no dictionary swap, no `DynamicResource` migration, no restart. 14 surface/text/border brushes flip between curated dark and light palettes; brand/semantic colors stay constant.

---

## [1.3.3] - 2026-06-04

### Added — Favorites categorization (Stage 8)
- **Categories**: every favorite can now be assigned a category (default "General"). New `Category` column persisted via EF Core migration `AddFavoriteCategory`.
- **Category filter**: a dropdown in the toolbar filters the list by category (auto-built from the categories in use, plus "All Categories").
- **Assign in the editor**: the favorite editor now has an editable category picker (choose an existing one or type a new one) alongside the note.
- **Category badge** shown on each favorite card.

---

## [1.3.2] - 2026-06-04

### Fixed / Improved — Security module (Stage 6)
- **Full localization**: every previously hardcoded English string on the Security page (subtitle, empty-state title/description, scanning text, "no issues" message) is now translated in all 5 languages (EN/TR/RU/DE/NL).
- **Robust scan**: 45-second timeout + re-entrancy guard so the spinner can never hang; proper `owner/repo` & URL parsing shared with the Analysis module; empty state hidden while loading.
- **Try-a-demo shortcut**: one click runs a security scan on `OWASP/NodeGoat` (a deliberately vulnerable reference app) so the module can be explored without entering a repo.

---

## [1.3.1] - 2026-06-04

### Added — Organizations module (Stage 5)
- **Popular organization quick-picks**: the Organizations page now greets you with a grid of curated organizations (Microsoft, Google, Meta, Apple, Amazon, Netflix, Docker, Kubernetes, Vercel, OpenAI, GitHub, Torvalds), each with its avatar — click one to load it instantly.
- **Robust lookup**: 45-second timeout + re-entrancy guard so the spinner can never hang; secondary data (members, repos) loads in its own try/catch and repos are sorted by stars.
- Localized the loading text; empty state is hidden while loading (MultiDataTrigger).

---

## [1.3.0] - 2026-06-04

### Added — Multi-provider AI (Stage 7 + 12)
- **Five AI providers**: OpenAI, **Claude (Anthropic)**, **Gemini (Google)**, **DeepSeek**, and Ollama. Each repository insight (summary, adoption, risk, improvements, Q&A) now works through any of them.
  - Claude via the Anthropic Messages API; Gemini via the Google Generative Language API; DeepSeek via its OpenAI-compatible endpoint (reuses the OpenAI SDK with a custom base URL).
- **Provider-aware Settings**: a single AI Provider dropdown drives a cascading **Model** picker (editable — choose a listed model or type your own) and a provider-specific **API key** field. Ollama shows its local endpoint instead of a key. Each provider keeps its own key + model.
- **Per-provider persistence**: new `ClaudeApiKey/Model`, `GeminiApiKey/Model`, `DeepSeekApiKey/Model` columns (EF Core migration `AddAiProviderSettings`).

### Changed — Architecture (SOLID / DRY)
- Extracted all prompt engineering into a shared `AiServiceBase`; every provider now implements only `CompleteAsync` + `IsAvailable`. This removed the large prompt duplication that previously lived in both `OpenAiService` and `OllamaService`.
- Added `AiModelCatalog` listing the selectable models per provider.

---

## [1.2.10] - 2026-06-04

### Fixed — Localization (Stage 4 + 9)
- **Raw localization keys showing on screen**: 10 keys were referenced in XAML but never defined, so the UI rendered the literal key text (e.g. `Reports.Title`, `Compare.Add`, `Security.Scan`, `AI.Generate`, and the six `Score.*` dimension labels). All are now defined and translated in **all 5 languages** (EN/TR/RU/DE/NL).
- Added a build-time verification that every `{loc:Localization …}` key used in XAML has a definition — the set is now complete (0 missing).
- Confirmed runtime language switching propagates to every localized binding (the indexer binding re-evaluates on the manager's `PropertyChanged`).

---

## [1.2.9] - 2026-06-04

### Fixed — Analysis module (Stage 1 + 2)
- **"Analyzing repository…" infinite spinner**: the GitHub API calls had no timeout, so a network stall or rate-limit left the `await` hanging forever and the `finally` (which resets the spinner) never ran. Added a hard 45-second `CancellationTokenSource` and a re-entrancy guard; the loading state is now **always** reset, even on timeout or error.
- **Brittle analysis**: secondary data (contributors, security alerts) is now loaded in its own `try/catch` — a failure there shows a warning but no longer aborts the whole analysis or leaves the page stuck.
- **State overlap**: the empty state is now hidden while loading (MultiDataTrigger on `Repository == null` AND `IsLoading == false`), so the spinner and the "Enter a repository" placeholder no longer render on top of each other.

### Added — Analysis module
- **Analyze your own repositories**: the page now loads your GitHub repositories on open and offers a picker next to the search box — selecting one analyzes it immediately. Falls back gracefully when not signed in.

---

## [1.2.8] - 2026-06-04

### Fixed (definitive root cause from crash.log)
- **Recurring `InvalidOperationException: '#FF1A1A2E' is not a valid value for 'Background'`** — finally traced to the true source: two style triggers assigned a **`Color` resource** (`{StaticResource BgSidebarItem}`, a `<Color>`) to the **`Background` property** (which requires a `Brush`). WPF stored the colour string and threw at render time. This fired whenever a sidebar nav item was pressed or a ComboBox dropdown item was hovered — which is why it appeared "randomly" across many screens and survived the previous Logs/DataGrid change. Both triggers now use the matching `SidebarItemBrush`. Swept the entire codebase to confirm no other `Color`-as-`Brush` misuse remains.

### Quality
- **0 warnings, 0 errors** across the whole solution:
  - `CS8603` (InverseBoolConverter possible-null return) — returns a definite `bool`.
  - `CS0618` (Octokit `Repository.WatchersCount` deprecated) — mapped to `SubscribersCount` (the real watcher count); popularity score updated accordingly.
  - `CS0618` (`DateRange.GreaterThan(DateTime)` deprecated) — switched to the `DateTimeOffset` overload.
  - `CA1416` (DPAPI `ProtectedData` is Windows-only) — guarded with `OperatingSystem.IsWindows()` and a cross-platform base64 fallback.
  - `NU1701` (LiveCharts' transitive OpenTK/SkiaSharp target .NET Framework) — documented and suppressed in the UI project.

---

## [1.2.7] - 2026-06-03

### Fixed (diagnosed from crash.log — exact root causes)
- **TrendingPage crash (`XamlParseException` → `ArgumentException: a Style may not set the Style property`)**: the period filter buttons used `<Setter Property="Style" Value="{StaticResource PrimaryButtonStyle}"/>` inside a `DataTrigger` — WPF forbids a Style from setting the `Style` property of the element it is applied to. Replaced with `Background`/`BorderBrush`/`Foreground` setters (picked up via the template's TemplateBindings). Trending now opens correctly.
- **Logs page render crash (`InvalidOperationException: '#FF1A1A2E' is not a valid value for 'Background'`)**: the global `DataGrid` style assigned a string/colour literal to `AlternatingRowBackground`, which WPF mis-typed when rendering rows. Replaced the entire Logs `DataGrid` with a lightweight, fully-localized custom list — no more DataGrid, no more crash.

### Changed / Improved
- **Logs page redesigned**: dedicated column header row, wider monospace timestamp column (no more cramped date/time), per-level coloured badges, hover highlight, inline red exception panel, and an empty state. Fully localized (subtitle, column titles, stat labels) in all 5 languages.
- **Logo redesigned**: `AppLogoControl` is now a clean, static hexagon-hub mark — removed the perpetually-spinning dashed orbit ring and scattered glow nodes that looked cluttered in the sidebar and Settings.
- **Sidebar footer cleaned up**: removed the tiny cramped logo block under "Sign Out"; replaced with a clean "XinPari Software · MIT" text line.
- Localized the remaining hardcoded Trending strings (Refresh, loading).

### Added
- **Trending card actions**: each trending repository now has Add-to-Favorites, Copy-clone-URL, and Open-on-GitHub buttons. `TrendingViewModel` now injects `IFavoriteService`.

---

## [1.2.6] - 2026-06-03

### Fixed
- **XamlParseException "FrameworkElement.Style" — TRUE ROOT CAUSE**: `CardStyle`, `GlassCardStyle`, `PrimaryButtonStyle`, and `StatCardStyle` each placed a `ScaleTransform` directly in a `Setter.Value` and animated it via hover storyboards. A `Setter.Value` Freezable is shared across every element using the style; on multi-card pages (Analyze, Favorites) WPF could not assign the same `ScaleTransform` to a second element's `RenderTransform`, throwing `InvalidOperationException` surfaced as the Style special-case exception. Removed all shared `ScaleTransform` setters and scale-hover storyboards; replaced with lightweight border/background hover states.
- **Language ComboBox not selectable by click (only mouse wheel)**: The custom `ComboBox` ControlTemplate was missing the `ToggleButton` that toggles `IsDropDownOpen` on click — so clicking did nothing while the wheel still changed the index natively. Added a transparent overlay `ToggleButton` (ClickMode=Press, two-way bound to `IsDropDownOpen`) covering the whole control. Dropdown now opens on click everywhere. Also made the popup width match the control.
- **Weird/overlapping perpetual animations** (sidebar top & bottom, Settings About): Removed the forever-spinning orbit ring + pulse storyboards from `AppLogoControl`; the logo is now clean and static.

### Added
- **Richer Favorites page**: live search box, sort dropdown (Recently Added / Name / Stars), per-card action buttons (Open on GitHub, Copy clone URL, Edit note, Remove), owner avatar, and a modal note editor. Remove now asks for confirmation.
- **Crash diagnostics**: unhandled exceptions are now written with full inner-exception chains and stack traces to `%LOCALAPPDATA%\OpenSourceHub\crash.log`, plus an `UnobservedTaskException` handler. Error dialogs now show the inner-exception detail and the log path.
- **App icon**: regenerated as a standard modern multi-resolution ICO (16/24/32/48/64 DIB + 128/256 PNG) so Windows Explorer and the taskbar render it correctly.

> If the old icon still appears in Explorer, it is the Windows icon cache — it refreshes on its own or after `ie4uinit.exe -show`.

---

## [1.2.5] - 2026-06-03

### Fixed
- **XamlParseException "FrameworkElement.Style" (all remaining)**: Added `x:Shared="False"` to all four shadow resources (`ShadowSM`, `ShadowMD`, `ShadowLG`, `ShadowBrand`) in AppTheme.xaml. WPF was trying to set the same `DropShadowEffect` instance on multiple elements via style triggers, which throws `InvalidOperationException` (an element can only belong to one visual tree). `x:Shared="False"` forces a new instance per use.
- **InvalidOperationException: ObservableCollection.Count TwoWay binding**: In .NET, `Run.Text` has `BindsTwoWayByDefault=true`. Added `Mode=OneWay` to all `<Run Text="{Binding ...}"/>` bindings across all pages (FavoritesPage, LogsPage, OrganizationPage, RepositoryAnalysisPage, AiPage, SettingsPage).
- **InvalidOperationException: AppVersion TwoWay binding**: `SettingsViewModel.AppVersion` is read-only. Fixed by adding `Mode=OneWay` to the binding via the Run.Text fix above.
- **Language ComboBox not selectable by click**: Replaced `SelectedValuePath="Tag"` + inline `{x:Static}` items with `ItemsSource="{Binding AvailableLanguages}"` + `SelectedItem="{Binding SelectedLanguage}"` + `EnumToLanguageNameConverter`. The `SelectedValuePath` approach with x:Static enum tags has a known WPF click-selection bug.

### Added
- `EnumToLanguageNameConverter` for displaying language enum values as human-readable names in ComboBox.
- `AvailableLanguages` property on `SettingsViewModel` exposing all supported languages.

---

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
