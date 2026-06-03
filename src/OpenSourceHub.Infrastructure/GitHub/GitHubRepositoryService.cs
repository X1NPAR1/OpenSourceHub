using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Octokit;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.Data;

namespace OpenSourceHub.Infrastructure.GitHub;

public class GitHubRepositoryService : IRepositoryService
{
    private readonly IGitHubAuthService _auth;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GitHubRepositoryService> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _db;

    public GitHubRepositoryService(IGitHubAuthService auth, IMemoryCache cache, ILogger<GitHubRepositoryService> logger, HttpClient httpClient, AppDbContext db)
    {
        _auth = auth;
        _cache = cache;
        _db = db;
        _logger = logger;
        _httpClient = httpClient;
    }

    private GitHubClient CreateClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("OpenSourceHub", "1.1.5"));
        if (_auth.IsAuthenticated && _auth.CurrentToken != null)
            client.Credentials = new Credentials(_auth.CurrentToken);
        return client;
    }

    public async Task<Domain.Entities.RepositoryInfo?> GetRepositoryAsync(string owner, string name, CancellationToken ct = default)
    {
        var key = $"repo||{owner}||{name}";
        if (_cache.TryGetValue(key, out Domain.Entities.RepositoryInfo? cached)) return cached;

        try
        {
            var client = CreateClient();
            var repo = await client.Repository.Get(owner, name);
            var info = MapRepository(repo);
            _cache.Set(key, info, TimeSpan.FromMinutes(30));
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository {Owner}/{Name}", owner, name);
            return null;
        }
    }

    public async Task<List<Domain.Entities.RepositoryInfo>> GetUserRepositoriesAsync(string username, RepositorySortBy sort = RepositorySortBy.Stars, int count = 30, CancellationToken ct = default)
    {
        var key = $"user_repos||{username}||{sort}||{count}";
        if (_cache.TryGetValue(key, out List<Domain.Entities.RepositoryInfo>? cached)) return cached!;

        try
        {
            var client = CreateClient();
            var request = new RepositoryRequest
            {
                Sort = RepositorySort.Updated,
                Direction = SortDirection.Descending
            };

            IReadOnlyList<Octokit.Repository> repos;
            if (username == (await _auth.GetCurrentUserAsync())?.Login)
                repos = await client.Repository.GetAllForCurrent(new ApiOptions { PageSize = count, PageCount = 1 });
            else
                repos = await client.Repository.GetAllForUser(username, new ApiOptions { PageSize = count, PageCount = 1 });

            var result = repos.Select(MapRepository).ToList();
            result = sort switch
            {
                RepositorySortBy.Stars => result.OrderByDescending(r => r.StargazersCount).ToList(),
                RepositorySortBy.Forks => result.OrderByDescending(r => r.ForksCount).ToList(),
                RepositorySortBy.Issues => result.OrderByDescending(r => r.OpenIssuesCount).ToList(),
                RepositorySortBy.Name => result.OrderBy(r => r.Name).ToList(),
                _ => result.OrderByDescending(r => r.UpdatedAt).ToList()
            };

            _cache.Set(key, result, TimeSpan.FromMinutes(15));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repositories for {Username}", username);
            return [];
        }
    }

    public async Task<List<Domain.Entities.RepositoryInfo>> SearchRepositoriesAsync(string query, string? language = null, RepositorySortBy sort = RepositorySortBy.Stars, int count = 30, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var q = language != null ? $"{query} language:{language}" : query;
            var searchReq = new SearchRepositoriesRequest(q)
            {
                SortField = sort == RepositorySortBy.Stars ? RepoSearchSort.Stars :
                            sort == RepositorySortBy.Forks ? RepoSearchSort.Forks :
                            sort == RepositorySortBy.Updated ? RepoSearchSort.Updated : RepoSearchSort.Stars,
                Order = SortDirection.Descending,
                PerPage = Math.Min(count, 100)
            };

            var results = await client.Search.SearchRepo(searchReq);
            return results.Items.Select(MapRepository).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository search failed for query: {Query}", query);
            return [];
        }
    }

    public async Task<List<Domain.Entities.RepositoryInfo>> GetTrendingRepositoriesAsync(string? language = null, TrendPeriod period = TrendPeriod.Weekly, int count = 30, CancellationToken ct = default)
    {
        var key = $"trending||{language}||{period}";
        if (_cache.TryGetValue(key, out List<Domain.Entities.RepositoryInfo>? cached)) return cached!;

        try
        {
            var client = CreateClient();
            var since = period switch
            {
                TrendPeriod.Daily => DateTimeOffset.UtcNow.AddDays(-1),
                TrendPeriod.Weekly => DateTimeOffset.UtcNow.AddDays(-7),
                TrendPeriod.Monthly => DateTimeOffset.UtcNow.AddDays(-30),
                _ => DateTimeOffset.UtcNow.AddDays(-7)
            };

            var langFilter = language != null ? $" language:{language}" : "";
            var searchReq = new SearchRepositoriesRequest($"stars:>10{langFilter}")
            {
                Created = DateRange.GreaterThan(since),
                SortField = RepoSearchSort.Stars,
                Order = SortDirection.Descending,
                PerPage = Math.Min(count, 100)
            };

            var results = await client.Search.SearchRepo(searchReq);
            var list = results.Items.Select(MapRepository).ToList();
            _cache.Set(key, list, TimeSpan.FromHours(1));
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trending repositories");
            return [];
        }
    }

    public async Task<RepositoryAnalysis> AnalyzeRepositoryAsync(string owner, string name, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var repo = await client.Repository.Get(owner, name);

            var commitsTask = GetRecentCommitCountAsync(client, owner, name);
            var contributorsTask = GetAllContributorStatsAsync(client, owner, name);
            var issuesTask = GetIssueStatsAsync(client, owner, name);
            var prTask = GetPullRequestStatsAsync(client, owner, name);
            var releasesTask = GetReleaseInfoAsync(client, owner, name);
            var branchesTask = GetBranchCountAsync(client, owner, name);
            var contentsTask = CheckDocumentationAsync(client, owner, name);

            await Task.WhenAll(commitsTask, contributorsTask, issuesTask, prTask, releasesTask, branchesTask, contentsTask);

            var (openIssues, closedIssues) = await issuesTask;
            var (openPrs, mergedPrs) = await prTask;
            var (hasReadme, hasContrib, hasLicense, hasCodeOfConduct, hasSecurity, hasIssueTemplate, hasPrTemplate) = await contentsTask;
            var (totalContributors, activeContributors) = await contributorsTask;
            var (releaseCount, daysSinceLastRelease) = await releasesTask;

            var daysSinceLastPush = repo.PushedAt.HasValue
                ? (int)(DateTime.UtcNow - repo.PushedAt.Value.UtcDateTime).TotalDays
                : 9999;

            var issueCloseRatio = (openIssues + closedIssues) > 0 ? (double)closedIssues / (openIssues + closedIssues) : 0.5;
            var prMergeRatio = (openPrs + mergedPrs) > 0 ? (double)mergedPrs / (openPrs + mergedPrs) : 0.5;

            var activityScore = CalculateActivityScore(await commitsTask, daysSinceLastPush, openIssues, openPrs);
            var maintenanceScore = CalculateMaintenanceScore(daysSinceLastPush, issueCloseRatio, prMergeRatio, releaseCount);
            var communityScore = CalculateCommunityScore(repo.StargazersCount, repo.ForksCount, totalContributors, hasReadme, hasContrib, hasCodeOfConduct);
            var securityScore = CalculateSecurityScore(hasSecurity, hasLicense, repo.Archived, daysSinceLastPush);
            var popularityScore = CalculatePopularityScore(repo.StargazersCount, repo.ForksCount, repo.SubscribersCount);
            var healthScore = (activityScore * 0.25 + maintenanceScore * 0.25 + communityScore * 0.2 + securityScore * 0.15 + popularityScore * 0.15);

            var analysis = new RepositoryAnalysis
            {
                RepositoryFullName = $"{owner}/{name}",
                HealthScore = Math.Round(healthScore, 1),
                MaintenanceScore = Math.Round(maintenanceScore, 1),
                SecurityScore = Math.Round(securityScore, 1),
                CommunityScore = Math.Round(communityScore, 1),
                PopularityScore = Math.Round(popularityScore, 1),
                ActivityScore = Math.Round(activityScore, 1),
                HealthLevel = GetHealthLevel(healthScore),
                CommitFrequencyPerMonth = await commitsTask,
                ActiveContributors = activeContributors,
                TotalContributors = totalContributors,
                OpenIssues = openIssues,
                ClosedIssues = closedIssues,
                OpenPullRequests = openPrs,
                MergedPullRequests = mergedPrs,
                TotalReleases = releaseCount,
                BranchCount = await branchesTask,
                HasReadme = hasReadme,
                HasContributing = hasContrib,
                HasLicense = hasLicense,
                HasCodeOfConduct = hasCodeOfConduct,
                HasSecurityPolicy = hasSecurity,
                HasIssueTemplates = hasIssueTemplate,
                HasPrTemplates = hasPrTemplate,
                DaysSinceLastCommit = daysSinceLastPush,
                DaysSinceLastRelease = daysSinceLastRelease,
                IssueCloseRatio = Math.Round(issueCloseRatio, 2),
                PrMergeRatio = Math.Round(prMergeRatio, 2),
                Recommendations = BuildRecommendations(hasReadme, hasContrib, hasLicense, hasCodeOfConduct, hasSecurity, daysSinceLastPush, issueCloseRatio),
                Warnings = BuildWarnings(repo.Archived, daysSinceLastPush, openIssues, securityScore),
                AnalyzedAt = DateTime.UtcNow
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze repository {Owner}/{Name}", owner, name);
            return new RepositoryAnalysis
            {
                RepositoryFullName = $"{owner}/{name}",
                HealthScore = 0,
                Warnings = ["Analysis failed: " + ex.Message]
            };
        }
    }

    public async Task<List<ContributorInfo>> GetContributorsAsync(string owner, string name, int count = 30, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var contributors = await client.Repository.Statistics.GetContributors(owner, name);
            return contributors
                .OrderByDescending(c => c.Total)
                .Take(count)
                .Select(c => new ContributorInfo
                {
                    Login = c.Author.Login,
                    AvatarUrl = c.Author.AvatarUrl,
                    HtmlUrl = c.Author.HtmlUrl,
                    Contributions = c.Total,
                    MonthlyActivity = c.Weeks
                        .Where(w => w.Commits > 0)
                        .Select(w => new MonthlyContribution
                        {
                            Year = w.Week.Year,
                            Month = w.Week.Month,
                            CommitCount = w.Commits
                        }).ToList()
                }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get contributors for {Owner}/{Name}", owner, name);
            return [];
        }
    }

    public async Task<List<Domain.Entities.SecurityAlert>> GetSecurityAlertsAsync(string owner, string name, CancellationToken ct = default)
    {
        var alerts = new List<Domain.Entities.SecurityAlert>();
        try
        {
            var client = CreateClient();
            var repo = await client.Repository.Get(owner, name);

            if (repo.Archived)
                alerts.Add(new Domain.Entities.SecurityAlert
                {
                    RepositoryFullName = $"{owner}/{name}",
                    RiskLevel = Domain.Enums.SecurityRiskLevel.Medium,
                    Title = "Archived Repository",
                    Description = "This repository is archived and no longer maintained.",
                    Recommendation = "Consider using an actively maintained alternative.",
                    Category = "Maintenance"
                });

            var daysSinceUpdate = (DateTime.UtcNow - repo.UpdatedAt.UtcDateTime).TotalDays;
            if (daysSinceUpdate > 365)
                alerts.Add(new Domain.Entities.SecurityAlert
                {
                    RepositoryFullName = $"{owner}/{name}",
                    RiskLevel = Domain.Enums.SecurityRiskLevel.High,
                    Title = "Inactive Repository",
                    Description = $"No activity for {(int)daysSinceUpdate} days.",
                    Recommendation = "Security vulnerabilities may not be patched.",
                    Category = "Activity"
                });

            if (string.IsNullOrEmpty(repo.License?.Key))
                alerts.Add(new Domain.Entities.SecurityAlert
                {
                    RepositoryFullName = $"{owner}/{name}",
                    RiskLevel = Domain.Enums.SecurityRiskLevel.Low,
                    Title = "No License",
                    Description = "Repository has no license file.",
                    Recommendation = "Add an appropriate open-source license.",
                    Category = "Legal"
                });

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security alerts for {Owner}/{Name}", owner, name);
            return alerts;
        }
    }

    private static async Task<int> GetRecentCommitCountAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-30);
            var commits = await client.Repository.Commit.GetAll(owner, name,
                new CommitRequest { Since = since },
                new ApiOptions { PageSize = 100, PageCount = 1 });
            return commits.Count;
        }
        catch { return 0; }
    }

    private static async Task<(int total, int active)> GetAllContributorStatsAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var contributors = await client.Repository.Statistics.GetContributors(owner, name);
            if (contributors == null) return (0, 0);
            var cutoff = DateTimeOffset.UtcNow.AddDays(-90);
            int total = contributors.Count;
            int active = contributors.Count(c =>
                c.Weeks.Any(w => w.Week >= cutoff && w.Commits > 0));
            return (total, active);
        }
        catch { return (0, 0); }
    }

    private static async Task<(int open, int closed)> GetIssueStatsAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var openIssues = await client.Issue.GetAllForRepository(owner, name,
                new RepositoryIssueRequest { State = ItemStateFilter.Open },
                new ApiOptions { PageSize = 1, PageCount = 1 });
            var closedIssues = await client.Issue.GetAllForRepository(owner, name,
                new RepositoryIssueRequest { State = ItemStateFilter.Closed },
                new ApiOptions { PageSize = 1, PageCount = 1 });
            return (openIssues.Count, closedIssues.Count);
        }
        catch { return (0, 0); }
    }

    private static async Task<(int open, int merged)> GetPullRequestStatsAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var open = await client.PullRequest.GetAllForRepository(owner, name,
                new PullRequestRequest { State = ItemStateFilter.Open },
                new ApiOptions { PageSize = 1, PageCount = 1 });
            var closed = await client.PullRequest.GetAllForRepository(owner, name,
                new PullRequestRequest { State = ItemStateFilter.Closed },
                new ApiOptions { PageSize = 1, PageCount = 1 });
            return (open.Count, closed.Count);
        }
        catch { return (0, 0); }
    }

    private static async Task<(int count, int daysSinceLatest)> GetReleaseInfoAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var releases = await client.Repository.Release.GetAll(owner, name, new ApiOptions { PageSize = 10, PageCount = 1 });
            if (releases.Count == 0) return (0, 9999);
            var latest = releases.OrderByDescending(r => r.PublishedAt).First();
            int days = latest.PublishedAt.HasValue
                ? (int)(DateTime.UtcNow - latest.PublishedAt.Value.UtcDateTime).TotalDays
                : 9999;
            return (releases.Count, days);
        }
        catch { return (0, 9999); }
    }

    private static async Task<int> GetBranchCountAsync(GitHubClient client, string owner, string name)
    {
        try
        {
            var branches = await client.Repository.Branch.GetAll(owner, name, new ApiOptions { PageSize = 100, PageCount = 1 });
            return branches.Count;
        }
        catch { return 0; }
    }

    private static async Task<(bool readme, bool contributing, bool license, bool coc, bool security, bool issueTemplate, bool prTemplate)> CheckDocumentationAsync(GitHubClient client, string owner, string name)
    {
        bool readme = false, contributing = false, license = false, coc = false, security = false, issueTemplate = false, prTemplate = false;
        try
        {
            var contents = await client.Repository.Content.GetAllContents(owner, name);
            foreach (var content in contents)
            {
                var n = content.Name.ToLowerInvariant();
                if (n.StartsWith("readme")) readme = true;
                if (n is "contributing" or "contributing.md") contributing = true;
                if (n is "license" or "license.md" or "license.txt") license = true;
                if (n is "code_of_conduct.md" or "code-of-conduct.md") coc = true;
                if (n is "security.md" or "security_policy.md") security = true;
            }
        }
        catch { }

        try
        {
            var githubDir = await client.Repository.Content.GetAllContents(owner, name, ".github");
            foreach (var item in githubDir)
            {
                var n = item.Name.ToLowerInvariant();
                if (item.Type == ContentType.Dir && n is "issue_template" or "issue-template") issueTemplate = true;
                if (item.Type == ContentType.Dir && n is "pull_request_template" or "pull-request-template") prTemplate = true;
                if (n is "security.md" or "security_policy.md") security = true;
                if (n is "contributing.md") contributing = true;
                if (n.StartsWith("pull_request_template") || n.StartsWith("pull-request-template")) prTemplate = true;
            }
        }
        catch { }

        return (readme, contributing, license, coc, security, issueTemplate, prTemplate);
    }

    private static double CalculateActivityScore(int commitsPerMonth, int daysSinceLastPush, int openIssues, int openPrs)
    {
        var commitScore = Math.Min(commitsPerMonth * 2, 50);
        var freshnessScore = daysSinceLastPush switch
        {
            < 7 => 30,
            < 30 => 25,
            < 90 => 15,
            < 180 => 5,
            _ => 0
        };
        var engagementScore = Math.Min((openIssues + openPrs) * 0.5, 20);
        return Math.Min(commitScore + freshnessScore + engagementScore, 100);
    }

    private static double CalculateMaintenanceScore(int daysSinceLastPush, double issueRatio, double prRatio, int releases)
    {
        var freshnessScore = daysSinceLastPush < 30 ? 40 : daysSinceLastPush < 90 ? 25 : daysSinceLastPush < 180 ? 10 : 0;
        var issueScore = issueRatio * 30;
        var prScore = prRatio * 20;
        var releaseScore = Math.Min(releases * 2, 10);
        return Math.Min(freshnessScore + issueScore + prScore + releaseScore, 100);
    }

    private static double CalculateCommunityScore(int stars, int forks, int contributors, bool readme, bool contrib, bool coc)
    {
        var starScore = Math.Min(Math.Log10(Math.Max(stars, 1)) * 15, 40);
        var forkScore = Math.Min(Math.Log10(Math.Max(forks, 1)) * 10, 25);
        var contribScore = Math.Min(contributors * 2, 20);
        var docScore = (readme ? 5 : 0) + (contrib ? 5 : 0) + (coc ? 5 : 0);
        return Math.Min(starScore + forkScore + contribScore + docScore, 100);
    }

    private static double CalculateSecurityScore(bool hasSecurityPolicy, bool hasLicense, bool isArchived, int daysSinceLastPush)
    {
        var score = 60.0;
        if (hasSecurityPolicy) score += 20;
        if (hasLicense) score += 10;
        if (isArchived) score -= 30;
        if (daysSinceLastPush > 365) score -= 20;
        return Math.Max(0, Math.Min(score, 100));
    }

    private static double CalculatePopularityScore(int stars, int forks, int subscribers)
    {
        var starScore = Math.Min(Math.Log10(Math.Max(stars, 1)) * 20, 60);
        var forkScore = Math.Min(Math.Log10(Math.Max(forks, 1)) * 15, 25);
        var watcherScore = Math.Min(Math.Log10(Math.Max(subscribers, 1)) * 5, 15);
        return Math.Min(starScore + forkScore + watcherScore, 100);
    }

    private static RepositoryHealthLevel GetHealthLevel(double score) => score switch
    {
        >= 80 => RepositoryHealthLevel.Excellent,
        >= 60 => RepositoryHealthLevel.Good,
        >= 40 => RepositoryHealthLevel.Fair,
        >= 20 => RepositoryHealthLevel.Poor,
        _ => RepositoryHealthLevel.Critical
    };

    private static List<string> BuildRecommendations(bool readme, bool contrib, bool license, bool coc, bool security, int daysSinceLastPush, double issueRatio)
    {
        var recs = new List<string>();
        if (!readme) recs.Add("Add a comprehensive README.md file");
        if (!contrib) recs.Add("Add a CONTRIBUTING.md guide for contributors");
        if (!license) recs.Add("Add an open-source license (e.g., MIT, Apache 2.0)");
        if (!coc) recs.Add("Add a Code of Conduct to foster inclusive community");
        if (!security) recs.Add("Add a SECURITY.md with vulnerability reporting instructions");
        if (daysSinceLastPush > 90) recs.Add("Increase commit frequency to show active maintenance");
        if (issueRatio < 0.5) recs.Add("Improve issue response rate — many open issues may deter users");
        return recs;
    }

    private static List<string> BuildWarnings(bool isArchived, int daysSinceLastPush, int openIssues, double securityScore)
    {
        var warnings = new List<string>();
        if (isArchived) warnings.Add("⚠️ Repository is archived — no longer actively maintained");
        if (daysSinceLastPush > 180) warnings.Add($"⚠️ No commits in {daysSinceLastPush} days");
        if (openIssues > 500) warnings.Add($"⚠️ {openIssues} open issues — may indicate maintenance issues");
        if (securityScore < 30) warnings.Add("⚠️ Low security score — missing critical security documentation");
        return warnings;
    }

    public async Task SaveAnalysisAsync(RepositoryAnalysis analysis, CancellationToken ct = default)
    {
        try
        {
            var existing = await _db.RepositoryAnalyses
                .FirstOrDefaultAsync(a => a.RepositoryFullName == analysis.RepositoryFullName, ct);
            if (existing != null)
            {
                existing.AiSummary = analysis.AiSummary;
                existing.HealthScore = analysis.HealthScore;
                existing.AnalyzedAt = analysis.AnalyzedAt;
            }
            else
            {
                _db.RepositoryAnalyses.Add(analysis);
            }
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save analysis for {Repo}", analysis.RepositoryFullName);
        }
    }

    private static Domain.Entities.RepositoryInfo MapRepository(Octokit.Repository r) => new()
    {
        Id = r.Id,
        Owner = r.Owner.Login,
        Name = r.Name,
        FullName = r.FullName,
        Description = r.Description,
        Language = r.Language,
        Homepage = r.Homepage,
        HtmlUrl = r.HtmlUrl,
        CloneUrl = r.CloneUrl,
        StargazersCount = r.StargazersCount,
        ForksCount = r.ForksCount,
        WatchersCount = r.SubscribersCount,
        OpenIssuesCount = r.OpenIssuesCount,
        SubscribersCount = r.SubscribersCount,
        NetworkCount = 0,
        Size = r.Size,
        IsPrivate = r.Private,
        IsFork = r.Fork,
        IsArchived = r.Archived,
        IsDisabled = false,
        HasIssues = r.HasIssues,
        HasProjects = false,
        HasWiki = r.HasWiki,
        HasPages = r.HasPages,
        HasDownloads = r.HasDownloads,
        DefaultBranch = r.DefaultBranch,
        License = r.License?.Name,
        Topics = r.Topics?.ToList() ?? [],
        CreatedAt = r.CreatedAt.UtcDateTime,
        UpdatedAt = r.UpdatedAt.UtcDateTime,
        PushedAt = r.PushedAt?.UtcDateTime,
        CachedAt = DateTime.UtcNow
    };
}
