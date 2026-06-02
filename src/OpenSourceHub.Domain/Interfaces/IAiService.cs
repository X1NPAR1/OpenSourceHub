using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface IAiService
{
    Task<string> GenerateRepositorySummaryAsync(RepositoryInfo repo, RepositoryAnalysis? analysis = null, CancellationToken ct = default);
    Task<string> GenerateAdoptionRecommendationAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default);
    Task<string> GenerateRiskReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, List<SecurityAlert> alerts, CancellationToken ct = default);
    Task<string> GenerateImprovementSuggestionsAsync(RepositoryInfo repo, RepositoryAnalysis analysis, CancellationToken ct = default);
    Task<string> AskAboutRepositoryAsync(RepositoryInfo repo, string question, CancellationToken ct = default);
    bool IsAvailable { get; }
}
