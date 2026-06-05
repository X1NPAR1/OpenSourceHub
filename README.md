<div align="center">

<img src="https://img.shields.io/badge/version-1.4.0-0078D4?style=flat-square" alt="Version"/>
<img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square" alt=".NET"/>
<img src="https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square" alt="Platform"/>
<img src="https://img.shields.io/badge/license-MIT-10B981?style=flat-square" alt="License"/>

# OpenSourceHub

**Professional GitHub Intelligence Platform**

Analyze. Discover. Build Better.

[Download v1.4.0](https://github.com/X1NPAR1/OpenSourceHub/releases/latest) · [Report Bug](https://github.com/X1NPAR1/OpenSourceHub/issues) · [Request Feature](https://github.com/X1NPAR1/OpenSourceHub/issues)

</div>

---

## Overview

OpenSourceHub is a professional Windows desktop application for analyzing GitHub repositories. It combines GitHub API data with AI-powered insights, security assessments, and comprehensive reporting — all in a modern dark-theme WPF interface.

Built with **.NET 10**, **WPF**, and **Clean Architecture**, it provides everything you need to evaluate, compare, and monitor open-source projects.

---

## Features

| Feature | Description |
|---------|-------------|
| **Repository Analysis** | 6-dimensional health scoring: Activity, Maintenance, Security, Community, Popularity, Health |
| **AI Insights** | OpenAI · Claude · Gemini · DeepSeek · Ollama summaries, adoption recommendations, improvement suggestions |
| **Security Center** | Risk detection across maintenance, license, and activity dimensions |
| **Trending** | Discover rising repositories by language and time period |
| **Compare** | Side-by-side comparison of up to 4 repositories |
| **Organizations** | Explore GitHub organizations, members, and repositories |
| **Repository Management** | Browse files, edit & commit, delete files, create branches, view pull requests (requires `repo` scope) |
| **Favorites** | Bookmark, categorize, and annotate repositories |
| **Reports** | Export PDF, Markdown, and HTML reports |
| **Logs** | Application event log with filtering and export |
| **Settings** | Theme, language (EN/TR/RU/DE/NL), AI provider, cache, and notifications |

---

## Screenshots

> Coming soon — UI screenshots will be added after first stable release.

---

## Getting Started

### Requirements

- Windows 10 x64 or later
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- A GitHub Personal Access Token

### Installation

1. Download the latest release from [Releases](https://github.com/X1NPAR1/OpenSourceHub/releases/latest)
2. Extract the ZIP file
3. Run `OpenSourceHub.exe`

### GitHub Token Setup

OpenSourceHub requires a GitHub Personal Access Token to access the GitHub API.

1. Go to **GitHub → Settings → Developer settings → Personal access tokens**
2. Generate a new token (Classic) with the following scopes:
   ```
   repo, read:org, read:user, user:email
   ```
3. Copy the token (starts with `ghp_` or `github_pat_`)
4. Paste it in the Sign In screen and click **Sign in to GitHub**

---

## Build from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 x64 or later
- Visual Studio 2022+ or JetBrains Rider

### Steps

```bash
git clone https://github.com/X1NPAR1/OpenSourceHub.git
cd OpenSourceHub
dotnet restore
dotnet build
dotnet run --project src/OpenSourceHub.UI
```

### Publish (Release Build)

```bash
dotnet publish src/OpenSourceHub.UI/OpenSourceHub.UI.csproj \
  -c Release -r win-x64 --self-contained false \
  -o ./release/output
```

---

## Architecture

```
OpenSourceHub/
├── src/
│   ├── OpenSourceHub.Domain          # Entities, enums, interfaces
│   ├── OpenSourceHub.Application     # Use cases, application logic
│   ├── OpenSourceHub.Infrastructure  # GitHub API, AI services, SQLite, EF Core
│   ├── OpenSourceHub.Localization    # EN / TR / RU strings
│   ├── OpenSourceHub.Reporting       # PDF / Markdown / HTML generation
│   └── OpenSourceHub.UI              # WPF desktop application (MVVM)
└── tests/
    └── OpenSourceHub.Tests           # xUnit unit & integration tests
```

**Patterns:** Clean Architecture · MVVM (CommunityToolkit.Mvvm) · Dependency Injection · Repository Pattern

**Key Libraries:**
- [Octokit](https://github.com/octokit/octokit.net) — GitHub REST API
- [OpenAI SDK](https://github.com/openai/openai-dotnet) — AI integration
- [Entity Framework Core 10](https://learn.microsoft.com/ef/core/) + SQLite
- [QuestPDF](https://www.questpdf.com/) — PDF generation
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — MVVM helpers

---

## AI Configuration

OpenSourceHub supports **five AI providers** — pick one in **Settings → AI**, choose a model (or type your own), and enter the provider's API key. Each provider keeps its own key + model.

| Provider | Auth | Example models |
|----------|------|----------------|
| **OpenAI** (cloud) | API key | gpt-4o, gpt-4o-mini, gpt-4.1 |
| **Claude** (Anthropic) | API key | claude-opus-4-5, claude-sonnet-4-5 |
| **Gemini** (Google) | API key | gemini-2.0-flash, gemini-1.5-pro |
| **DeepSeek** | API key | deepseek-chat, deepseek-reasoner |
| **Ollama** (local) | endpoint | llama3.2, qwen2.5-coder, mistral |

For Ollama, install it locally, `ollama pull llama3.2`, then set the endpoint (default `http://localhost:11434`).

---

## Repository Management

Open the **Repository** tab (requires a token with the `repo` scope) to:
- pick one of your repositories and a branch,
- browse the file tree and open files,
- **edit & commit** a file, **create** a new file, **delete** a file,
- **create a branch** from the current one,
- view **open pull requests** (click to open on GitHub).

---

## Theme & Localization

- **Theme**: Dark / Light / System — switches instantly at runtime (no restart).
- **Languages** (runtime switch in Settings → Appearance):

| Language | Code |
|----------|------|
| English  | EN   |
| Türkçe   | TR   |
| Русский  | RU   |
| Deutsch  | DE   |
| Nederlands | NL |

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## Contributing

Contributions are welcome. Please open an issue before submitting a pull request to discuss the proposed change.

---

<div align="center">
Made with ❤️ by <a href="https://github.com/X1NPAR1">X1NPAR1</a> · XinPari Software
</div>
