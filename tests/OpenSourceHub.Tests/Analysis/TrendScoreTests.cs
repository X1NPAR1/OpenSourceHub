using FluentAssertions;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Infrastructure.GitHub;

namespace OpenSourceHub.Tests.Analysis;

public class TrendScoreTests
{
    [Fact]
    public void RecentFastGrowingRepo_OutranksOldHighStarRepo()
    {
        // A young repo that gained 1,000 stars in 5 days (200/day) and was just pushed.
        var rising = new RepositoryInfo
        {
            StargazersCount = 1000,
            ForksCount = 50,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            PushedAt = DateTime.UtcNow.AddHours(-6)
        };

        // An old, large repo: 50k stars but created 8 years ago (~17/day), stale push.
        var veteran = new RepositoryInfo
        {
            StargazersCount = 50000,
            ForksCount = 8000,
            CreatedAt = DateTime.UtcNow.AddYears(-8),
            PushedAt = DateTime.UtcNow.AddDays(-40)
        };

        var risingScore = GitHubRepositoryService.CalculateTrendScore(rising, 7.0);
        var veteranScore = GitHubRepositoryService.CalculateTrendScore(veteran, 7.0);

        risingScore.Should().BeGreaterThan(veteranScore);
    }

    [Fact]
    public void RecentPush_AddsRecencyBonus()
    {
        var baseRepo = new RepositoryInfo
        {
            StargazersCount = 100,
            ForksCount = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            PushedAt = DateTime.UtcNow.AddDays(-30)
        };
        var freshlyPushed = new RepositoryInfo
        {
            StargazersCount = 100,
            ForksCount = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            PushedAt = DateTime.UtcNow.AddHours(-2)
        };

        GitHubRepositoryService.CalculateTrendScore(freshlyPushed, 7.0)
            .Should().BeGreaterThan(GitHubRepositoryService.CalculateTrendScore(baseRepo, 7.0));
    }

    [Fact]
    public void Score_IsNonNegative()
    {
        var repo = new RepositoryInfo
        {
            StargazersCount = 0,
            ForksCount = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PushedAt = null
        };

        GitHubRepositoryService.CalculateTrendScore(repo, 7.0).Should().BeGreaterThanOrEqualTo(0);
    }
}
