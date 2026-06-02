namespace OpenSourceHub.Domain.Entities;

public class RepositoryInfo
{
    public long Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? Homepage { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public int WatchersCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public int SubscribersCount { get; set; }
    public int NetworkCount { get; set; }
    public long Size { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsFork { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDisabled { get; set; }
    public bool HasIssues { get; set; }
    public bool HasProjects { get; set; }
    public bool HasWiki { get; set; }
    public bool HasPages { get; set; }
    public bool HasDownloads { get; set; }
    public string? DefaultBranch { get; set; }
    public string? License { get; set; }
    public List<string> Topics { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PushedAt { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
