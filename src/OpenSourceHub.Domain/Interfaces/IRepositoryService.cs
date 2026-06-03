using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Domain.Interfaces;

public interface IRepositoryService
{
    Task<RepositoryInfo?> GetRepositoryAsync(string owner, string name, CancellationToken ct = default);
    Task<List<RepositoryInfo>> GetUserRepositoriesAsync(string username, RepositorySortBy sort = RepositorySortBy.Stars, int count = 30, CancellationToken ct = default);
    Task<List<RepositoryInfo>> SearchRepositoriesAsync(string query, string? language = null, RepositorySortBy sort = RepositorySortBy.Stars, int count = 30, CancellationToken ct = default);
    Task<List<RepositoryInfo>> GetTrendingRepositoriesAsync(string? language = null, TrendPeriod period = TrendPeriod.Weekly, int count = 30, CancellationToken ct = default);
    Task<RepositoryAnalysis> AnalyzeRepositoryAsync(string owner, string name, CancellationToken ct = default);
    Task<List<ContributorInfo>> GetContributorsAsync(string owner, string name, int count = 30, CancellationToken ct = default);
    Task<List<SecurityAlert>> GetSecurityAlertsAsync(string owner, string name, CancellationToken ct = default);
    Task SaveAnalysisAsync(RepositoryAnalysis analysis, CancellationToken ct = default);
}
