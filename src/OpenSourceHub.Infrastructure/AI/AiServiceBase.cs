using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;

namespace OpenSourceHub.Infrastructure.AI;

/// <summary>
/// Shared prompt-engineering for every AI provider. Concrete providers only
/// implement <see cref="CompleteAsync"/> and <see cref="IsAvailable"/>; the
/// rich prompts live here once (DRY — no per-provider duplication).
/// </summary>
public abstract class AiServiceBase : IAiService
{
    protected const string SystemPrompt =
        "You are an expert software architect and GitHub repository analyst. " +
        "Provide concise, professional, and actionable insights.";

    public abstract bool IsAvailable { get; }

    /// <summary>Send a single user prompt with the shared system prompt and return the text.</summary>
    protected abstract Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);

    public Task<string> GenerateRepositorySummaryAsync(RepositoryInfo repo, RepositoryAnalysis? analysis = null, CancellationToken ct = default)
    {
        var prompt = $"""
            Analyze this GitHub repository and provide a concise professional summary:

            Repository: {repo.FullName}
            Description: {repo.Description ?? "N/A"}
            Language: {repo.Language ?? "N/A"}
            Stars: {repo.StargazersCount:N0}
            Forks: {repo.ForksCount:N0}
            Open Issues: {repo.OpenIssuesCount:N0}
            License: {repo.License ?? "None"}
            Topics: {string.Join(", ", repo.Topics)}
            Created: {repo.CreatedAt:yyyy-MM-dd}
            Last Updated: {repo.UpdatedAt:yyyy-MM-dd}
            {(analysis != null ? $"Health Score: {analysis.HealthScore}/100" : "")}
            {(analysis != null ? $"Active Contributors: {analysis.ActiveContributors}" : "")}

            Provide a 3-4 sentence professional summary covering: what the project does, its quality/health, community engagement, and whether it's production-ready.
            """;
        return CompleteAsync(SystemPrompt, prompt, ct);
    }

    public Task<string> GenerateAdoptionRecommendationAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
    {
        var prompt = $"""
            Should my team adopt this GitHub repository? Provide a professional recommendation.

            Repository: {repo.FullName}
            Health Score: {analysis.HealthScore}/100
            Maintenance Score: {analysis.MaintenanceScore}/100
            Security Score: {analysis.SecurityScore}/100
            Community Score: {analysis.CommunityScore}/100
            Days Since Last Commit: {analysis.DaysSinceLastCommit}
            Active Contributors: {analysis.ActiveContributors}
            Open Issues: {analysis.OpenIssues}
            Has License: {analysis.HasLicense}
            Has Security Policy: {analysis.HasSecurityPolicy}
            Warnings: {string.Join("; ", analysis.Warnings)}

            Provide: 1) Overall recommendation (Adopt/Evaluate/Hold/Avoid), 2) Key reasons, 3) Risks to consider, 4) Alternatives to evaluate if applicable.
            """;
        return CompleteAsync(SystemPrompt, prompt, ct);
    }

    public Task<string> GenerateRiskReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, List<SecurityAlert> alerts, CancellationToken ct = default)
    {
        var prompt = $"""
            Generate a security risk report for this GitHub repository:

            Repository: {repo.FullName}
            Security Score: {analysis.SecurityScore}/100
            Is Archived: {repo.IsArchived}
            Has Security Policy: {analysis.HasSecurityPolicy}
            Has License: {analysis.HasLicense}
            Days Since Last Commit: {analysis.DaysSinceLastCommit}

            Security Alerts:
            {string.Join("\n", alerts.Select(a => $"- [{a.RiskLevel}] {a.Title}: {a.Description}"))}

            Provide a structured security risk assessment with: Risk Level, Key Vulnerabilities, Mitigation Strategies, and Final Security Verdict.
            """;
        return CompleteAsync(SystemPrompt, prompt, ct);
    }

    public Task<string> GenerateImprovementSuggestionsAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
    {
        var prompt = $"""
            Provide specific improvement suggestions for this GitHub repository:

            Repository: {repo.FullName}
            Health Score: {analysis.HealthScore}/100
            Missing: {string.Join(", ", GetMissing(analysis))}
            Issue Close Ratio: {analysis.IssueCloseRatio:P0}
            PR Merge Ratio: {analysis.PrMergeRatio:P0}
            Days Since Last Commit: {analysis.DaysSinceLastCommit}

            Provide 5-7 actionable, specific improvement suggestions with priority levels (High/Medium/Low) and estimated impact.
            """;
        return CompleteAsync(SystemPrompt, prompt, ct);
    }

    public Task<string> AskAboutRepositoryAsync(RepositoryInfo repo, string question, CancellationToken ct = default)
    {
        var prompt = $"""
            Context - GitHub Repository: {repo.FullName}
            Description: {repo.Description ?? "N/A"}
            Language: {repo.Language ?? "N/A"}
            Stars: {repo.StargazersCount:N0}, Forks: {repo.ForksCount:N0}
            License: {repo.License ?? "None"}
            Topics: {string.Join(", ", repo.Topics)}

            User Question: {question}

            Provide a helpful, accurate answer based on the repository context.
            """;
        return CompleteAsync(SystemPrompt, prompt, ct);
    }

    protected static IEnumerable<string> GetMissing(RepositoryAnalysis a)
    {
        if (!a.HasReadme) yield return "README";
        if (!a.HasContributing) yield return "CONTRIBUTING";
        if (!a.HasLicense) yield return "LICENSE";
        if (!a.HasSecurityPolicy) yield return "SECURITY.md";
        if (!a.HasCodeOfConduct) yield return "CODE_OF_CONDUCT";
    }
}

/// <summary>Catalog of selectable models per provider (shown in Settings).</summary>
public static class AiModelCatalog
{
    public static readonly IReadOnlyList<string> OpenAi =
        ["gpt-4o", "gpt-4o-mini", "gpt-4.1", "gpt-4.1-mini", "o3-mini"];

    public static readonly IReadOnlyList<string> Claude =
        ["claude-opus-4-5", "claude-sonnet-4-5", "claude-3-5-haiku-latest"];

    public static readonly IReadOnlyList<string> Gemini =
        ["gemini-2.0-flash", "gemini-2.0-flash-lite", "gemini-1.5-pro"];

    public static readonly IReadOnlyList<string> DeepSeek =
        ["deepseek-chat", "deepseek-reasoner"];

    public static readonly IReadOnlyList<string> Ollama =
        ["llama3.2", "llama3.1", "qwen2.5-coder", "mistral", "gemma2"];
}
