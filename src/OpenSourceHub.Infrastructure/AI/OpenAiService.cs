using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;

namespace OpenSourceHub.Infrastructure.AI;

public class OpenAiService : IAiService
{
    private readonly ISettingsService _settings;
    private readonly ILogger<OpenAiService> _logger;

    public bool IsAvailable
    {
        get
        {
            var s = _settings.GetSettingsAsync().GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(s.OpenAiApiKey);
        }
    }

    public OpenAiService(ISettingsService settings, ILogger<OpenAiService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    private async Task<ChatClient?> GetClientAsync()
    {
        var s = await _settings.GetSettingsAsync();
        if (string.IsNullOrEmpty(s.OpenAiApiKey)) return null;
        var openAi = new OpenAIClient(s.OpenAiApiKey);
        return openAi.GetChatClient(s.OpenAiModel ?? "gpt-4o-mini");
    }

    public async Task<string> GenerateRepositorySummaryAsync(RepositoryInfo repo, RepositoryAnalysis? analysis = null, CancellationToken ct = default)
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

        return await SendMessageAsync(prompt, ct);
    }

    public async Task<string> GenerateAdoptionRecommendationAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
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

        return await SendMessageAsync(prompt, ct);
    }

    public async Task<string> GenerateRiskReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, List<SecurityAlert> alerts, CancellationToken ct = default)
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

        return await SendMessageAsync(prompt, ct);
    }

    public async Task<string> GenerateImprovementSuggestionsAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
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

        return await SendMessageAsync(prompt, ct);
    }

    public async Task<string> AskAboutRepositoryAsync(RepositoryInfo repo, string question, CancellationToken ct = default)
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

        return await SendMessageAsync(prompt, ct);
    }

    private async Task<string> SendMessageAsync(string prompt, CancellationToken ct)
    {
        try
        {
            var client = await GetClientAsync();
            if (client == null) return "OpenAI API key not configured. Please set it in Settings > AI.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert software architect and GitHub repository analyst. Provide concise, professional, and actionable insights."),
                new UserChatMessage(prompt)
            };

            var response = await client.CompleteChatAsync(messages, cancellationToken: ct);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API call failed");
            return $"AI analysis failed: {ex.Message}";
        }
    }

    private static IEnumerable<string> GetMissing(RepositoryAnalysis a)
    {
        if (!a.HasReadme) yield return "README";
        if (!a.HasContributing) yield return "CONTRIBUTING";
        if (!a.HasLicense) yield return "LICENSE";
        if (!a.HasSecurityPolicy) yield return "SECURITY.md";
        if (!a.HasCodeOfConduct) yield return "CODE_OF_CONDUCT";
    }
}
