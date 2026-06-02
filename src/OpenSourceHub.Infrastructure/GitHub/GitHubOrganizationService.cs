using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Octokit;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;

namespace OpenSourceHub.Infrastructure.GitHub;

public class GitHubOrganizationService : IOrganizationService
{
    private readonly IGitHubAuthService _auth;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GitHubOrganizationService> _logger;

    public GitHubOrganizationService(IGitHubAuthService auth, IMemoryCache cache, ILogger<GitHubOrganizationService> logger)
    {
        _auth = auth;
        _cache = cache;
        _logger = logger;
    }

    private GitHubClient CreateClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("OpenSourceHub", "1.1.5"));
        if (_auth.IsAuthenticated && _auth.CurrentToken != null)
            client.Credentials = new Credentials(_auth.CurrentToken);
        return client;
    }

    public async Task<OrganizationInfo?> GetOrganizationAsync(string orgName, CancellationToken ct = default)
    {
        var key = $"org_{orgName}";
        if (_cache.TryGetValue(key, out OrganizationInfo? cached)) return cached;

        try
        {
            var client = CreateClient();
            var org = await client.Organization.Get(orgName);
            var info = new OrganizationInfo
            {
                Login = org.Login,
                Name = org.Name,
                Description = org.Description,
                AvatarUrl = org.AvatarUrl,
                HtmlUrl = org.HtmlUrl,
                Blog = org.Blog,
                Location = org.Location,
                Email = org.Email,
                PublicRepos = org.PublicRepos,
                Followers = org.Followers,
                Following = org.Following,
                IsVerified = org.IsVerified,
                CreatedAt = org.CreatedAt.UtcDateTime,
                UpdatedAt = org.UpdatedAt.UtcDateTime
            };

            _cache.Set(key, info, TimeSpan.FromMinutes(30));
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get organization {OrgName}", orgName);
            return null;
        }
    }

    public async Task<List<ContributorInfo>> GetOrganizationMembersAsync(string orgName, int count = 30, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var members = await client.Organization.Member.GetAll(orgName, new ApiOptions { PageSize = count, PageCount = 1 });
            return members.Select(m => new ContributorInfo
            {
                Login = m.Login,
                AvatarUrl = m.AvatarUrl,
                HtmlUrl = m.HtmlUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get members for {OrgName}", orgName);
            return [];
        }
    }

    public async Task<List<Domain.Entities.RepositoryInfo>> GetOrganizationRepositoriesAsync(string orgName, int count = 30, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var repos = await client.Repository.GetAllForOrg(orgName, new ApiOptions { PageSize = count, PageCount = 1 });
            return repos.Select(r => new Domain.Entities.RepositoryInfo
            {
                Id = r.Id,
                Owner = r.Owner.Login,
                Name = r.Name,
                FullName = r.FullName,
                Description = r.Description,
                Language = r.Language,
                StargazersCount = r.StargazersCount,
                ForksCount = r.ForksCount,
                OpenIssuesCount = r.OpenIssuesCount,
                IsArchived = r.Archived,
                CreatedAt = r.CreatedAt.UtcDateTime,
                UpdatedAt = r.UpdatedAt.UtcDateTime,
                HtmlUrl = r.HtmlUrl,
                CloneUrl = r.CloneUrl
            }).OrderByDescending(r => r.StargazersCount).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repositories for {OrgName}", orgName);
            return [];
        }
    }

    public async Task<List<OrganizationInfo>> GetUserOrganizationsAsync(string username, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var orgs = await client.Organization.GetAllForUser(username);
            var result = new List<OrganizationInfo>();
            foreach (var org in orgs.Take(20))
            {
                var info = await GetOrganizationAsync(org.Login, ct);
                if (info != null) result.Add(info);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get organizations for {Username}", username);
            return [];
        }
    }
}
