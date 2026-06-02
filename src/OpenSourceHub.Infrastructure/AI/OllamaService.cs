using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenSourceHub.Infrastructure.AI;

public class OllamaService : IAiService
{
    private readonly ISettingsService _settings;
    private readonly HttpClient _http;
    private readonly ILogger<OllamaService> _logger;
    private bool? _available;

    public bool IsAvailable => _available ?? false;

    public OllamaService(ISettingsService settings, HttpClient http, ILogger<OllamaService> logger)
    {
        _settings = settings;
        _http = http;
        _logger = logger;
    }

    public async Task<string> GenerateRepositorySummaryAsync(RepositoryInfo repo, RepositoryAnalysis? analysis = null, CancellationToken ct = default)
    {
        var prompt = $"Analyze this GitHub repository and write a short professional summary.\nRepo: {repo.FullName}\nDesc: {repo.Description}\nStars: {repo.StargazersCount}\nLanguage: {repo.Language}\nHealth: {analysis?.HealthScore ?? 0}/100";
        return await GenerateAsync(prompt, ct);
    }

    public async Task<string> GenerateAdoptionRecommendationAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
    {
        var prompt = $"Should my team adopt {repo.FullName}? Health: {analysis.HealthScore}/100, Security: {analysis.SecurityScore}/100, Days since commit: {analysis.DaysSinceLastCommit}. Give a clear recommendation.";
        return await GenerateAsync(prompt, ct);
    }

    public async Task<string> GenerateRiskReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, List<SecurityAlert> alerts, CancellationToken ct = default)
    {
        var alertsText = string.Join("; ", alerts.Select(a => $"{a.Title}({a.RiskLevel})"));
        var prompt = $"Security risk report for {repo.FullName}. Security score: {analysis.SecurityScore}/100. Alerts: {alertsText}. Provide risk assessment.";
        return await GenerateAsync(prompt, ct);
    }

    public async Task<string> GenerateImprovementSuggestionsAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default)
    {
        var prompt = $"Suggest improvements for {repo.FullName}. Health: {analysis.HealthScore}/100. Issue close ratio: {analysis.IssueCloseRatio:P0}. Days since commit: {analysis.DaysSinceLastCommit}.";
        return await GenerateAsync(prompt, ct);
    }

    public async Task<string> AskAboutRepositoryAsync(RepositoryInfo repo, string question, CancellationToken ct = default)
    {
        var prompt = $"Repository: {repo.FullName} ({repo.Description}). Stars: {repo.StargazersCount}. Question: {question}";
        return await GenerateAsync(prompt, ct);
    }

    private async Task<string> GenerateAsync(string prompt, CancellationToken ct)
    {
        try
        {
            var settings = await _settings.GetSettingsAsync();
            var endpoint = settings.OllamaEndpoint.TrimEnd('/');
            var model = settings.OllamaModel ?? "llama3.2";

            var request = new { model, prompt, stream = false };
            var response = await _http.PostAsJsonAsync($"{endpoint}/api/generate", request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            _available = true;
            return doc.RootElement.GetProperty("response").GetString() ?? "No response from Ollama.";
        }
        catch (Exception ex)
        {
            _available = false;
            _logger.LogError(ex, "Ollama API call failed");
            return $"Ollama not available: {ex.Message}";
        }
    }
}
