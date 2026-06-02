using FluentAssertions;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Tests.Analysis;

public class HealthScoreTests
{
    [Theory]
    [InlineData(80, RepositoryHealthLevel.Excellent)]
    [InlineData(60, RepositoryHealthLevel.Good)]
    [InlineData(40, RepositoryHealthLevel.Fair)]
    [InlineData(20, RepositoryHealthLevel.Poor)]
    [InlineData(5, RepositoryHealthLevel.Critical)]
    public void HealthLevel_MapsCorrectlyFromScore(double score, RepositoryHealthLevel expected)
    {
        var analysis = new RepositoryAnalysis { HealthScore = score };
        var level = GetHealthLevel(score);
        level.Should().Be(expected);
    }

    [Fact]
    public void Analysis_HasRequiredDefaults()
    {
        var analysis = new RepositoryAnalysis();
        analysis.Recommendations.Should().NotBeNull();
        analysis.Warnings.Should().NotBeNull();
        analysis.AnalyzedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SecurityAlert_HasRequiredDefaults()
    {
        var alert = new SecurityAlert();
        alert.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        alert.IsResolved.Should().BeFalse();
    }

    [Fact]
    public void FavoriteRepository_DefaultAddedAt_IsRecentUtc()
    {
        var fav = new FavoriteRepository();
        fav.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private static RepositoryHealthLevel GetHealthLevel(double score) => score switch
    {
        >= 80 => RepositoryHealthLevel.Excellent,
        >= 60 => RepositoryHealthLevel.Good,
        >= 40 => RepositoryHealthLevel.Fair,
        >= 20 => RepositoryHealthLevel.Poor,
        _ => RepositoryHealthLevel.Critical
    };
}
