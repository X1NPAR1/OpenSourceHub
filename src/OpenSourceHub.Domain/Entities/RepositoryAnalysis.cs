using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Domain.Entities;

public class RepositoryAnalysis
{
    public int Id { get; set; }
    public string RepositoryFullName { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public double MaintenanceScore { get; set; }
    public double SecurityScore { get; set; }
    public double CommunityScore { get; set; }
    public double PopularityScore { get; set; }
    public double ActivityScore { get; set; }
    public RepositoryHealthLevel HealthLevel { get; set; }
    public int CommitFrequencyPerMonth { get; set; }
    public int ActiveContributors { get; set; }
    public int TotalContributors { get; set; }
    public int OpenIssues { get; set; }
    public int ClosedIssues { get; set; }
    public int OpenPullRequests { get; set; }
    public int MergedPullRequests { get; set; }
    public int TotalReleases { get; set; }
    public int BranchCount { get; set; }
    public bool HasReadme { get; set; }
    public bool HasContributing { get; set; }
    public bool HasLicense { get; set; }
    public bool HasCodeOfConduct { get; set; }
    public bool HasSecurityPolicy { get; set; }
    public bool HasIssueTemplates { get; set; }
    public bool HasPrTemplates { get; set; }
    public int DaysSinceLastCommit { get; set; }
    public int DaysSinceLastRelease { get; set; }
    public double IssueCloseRatio { get; set; }
    public double PrMergeRatio { get; set; }
    public string? AiSummary { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}
