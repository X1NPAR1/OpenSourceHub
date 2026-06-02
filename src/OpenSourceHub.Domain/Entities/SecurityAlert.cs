using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Domain.Entities;

public class SecurityAlert
{
    public int Id { get; set; }
    public string RepositoryFullName { get; set; } = string.Empty;
    public SecurityRiskLevel RiskLevel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
