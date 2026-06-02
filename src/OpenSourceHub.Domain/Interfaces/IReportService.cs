using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Domain.Interfaces;

public interface IReportService
{
    Task<string> GenerateRepositoryReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, ReportFormat format, string outputPath, CancellationToken ct = default);
    Task<string> GenerateOrganizationReportAsync(OrganizationInfo org, ReportFormat format, string outputPath, CancellationToken ct = default);
    Task<string> GenerateComparisonReportAsync(List<RepositoryInfo> repos, List<RepositoryAnalysis> analyses, ReportFormat format, string outputPath, CancellationToken ct = default);
    Task<string> GenerateSecurityReportAsync(RepositoryInfo repo, List<SecurityAlert> alerts, ReportFormat format, string outputPath, CancellationToken ct = default);
}
