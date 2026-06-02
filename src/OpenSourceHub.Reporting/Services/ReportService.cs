using Markdig;
using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace OpenSourceHub.Reporting.Services;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateRepositoryReportAsync(RepositoryInfo repo, RepositoryAnalysis analysis, ReportFormat format, string outputPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputPath);
        var fileName = $"Repository_{repo.Owner}_{repo.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";

        return format switch
        {
            ReportFormat.PDF => await GenerateRepositoryPdfAsync(repo, analysis, outputPath, fileName, ct),
            ReportFormat.Markdown => await GenerateRepositoryMarkdownAsync(repo, analysis, outputPath, fileName, ct),
            ReportFormat.HTML => await GenerateRepositoryHtmlAsync(repo, analysis, outputPath, fileName, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task<string> GenerateOrganizationReportAsync(OrganizationInfo org, ReportFormat format, string outputPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputPath);
        var fileName = $"Organization_{org.Login}_{DateTime.Now:yyyyMMdd_HHmmss}";
        var md = BuildOrganizationMarkdown(org);
        return format switch
        {
            ReportFormat.Markdown => await SaveTextAsync(md, outputPath, fileName + ".md", ct),
            ReportFormat.HTML => await SaveTextAsync(ConvertToHtml(md, $"Organization: {org.Login}"), outputPath, fileName + ".html", ct),
            _ => await SaveTextAsync(md, outputPath, fileName + ".md", ct)
        };
    }

    public async Task<string> GenerateComparisonReportAsync(List<RepositoryInfo> repos, List<RepositoryAnalysis> analyses, ReportFormat format, string outputPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputPath);
        var fileName = $"Comparison_{DateTime.Now:yyyyMMdd_HHmmss}";
        var md = BuildComparisonMarkdown(repos, analyses);
        return format switch
        {
            ReportFormat.Markdown => await SaveTextAsync(md, outputPath, fileName + ".md", ct),
            ReportFormat.HTML => await SaveTextAsync(ConvertToHtml(md, "Repository Comparison"), outputPath, fileName + ".html", ct),
            _ => await SaveTextAsync(md, outputPath, fileName + ".md", ct)
        };
    }

    public async Task<string> GenerateSecurityReportAsync(RepositoryInfo repo, List<SecurityAlert> alerts, ReportFormat format, string outputPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputPath);
        var fileName = $"Security_{repo.Owner}_{repo.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
        var md = BuildSecurityMarkdown(repo, alerts);
        return format switch
        {
            ReportFormat.Markdown => await SaveTextAsync(md, outputPath, fileName + ".md", ct),
            ReportFormat.HTML => await SaveTextAsync(ConvertToHtml(md, $"Security Report: {repo.FullName}"), outputPath, fileName + ".html", ct),
            _ => await SaveTextAsync(md, outputPath, fileName + ".md", ct)
        };
    }

    private async Task<string> GenerateRepositoryPdfAsync(RepositoryInfo repo, RepositoryAnalysis analysis, string outputPath, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(outputPath, fileName + ".pdf");
        try
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("OpenSourceHub").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"Repository Analysis Report").FontSize(12).FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(120).AlignRight().Column(col =>
                            {
                                col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Medium);
                                col.Item().Text("© 2026 XinPari Software").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });
                        header.LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Item().Text(repo.FullName).FontSize(18).Bold();
                        if (!string.IsNullOrEmpty(repo.Description))
                            col.Item().PaddingTop(4).Text(repo.Description).FontSize(11).FontColor(Colors.Grey.Darken1);

                        col.Item().PaddingTop(16).Text("Repository Statistics").FontSize(14).Bold();
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });
                            AddTableRow(table, "⭐ Stars", repo.StargazersCount.ToString("N0"), "🍴 Forks", repo.ForksCount.ToString("N0"));
                            AddTableRow(table, "🐛 Open Issues", repo.OpenIssuesCount.ToString("N0"), "👥 Watchers", repo.WatchersCount.ToString("N0"));
                            AddTableRow(table, "💻 Language", repo.Language ?? "N/A", "📄 License", repo.License ?? "None");
                            AddTableRow(table, "📅 Created", repo.CreatedAt.ToString("yyyy-MM-dd"), "🔄 Updated", repo.UpdatedAt.ToString("yyyy-MM-dd"));
                        });

                        col.Item().PaddingTop(20).Text("Health Scores").FontSize(14).Bold();
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.RelativeColumn();
                                cols.RelativeColumn(3);
                            });
                            AddScoreRow(table, "Health Score", analysis.HealthScore);
                            AddScoreRow(table, "Maintenance Score", analysis.MaintenanceScore);
                            AddScoreRow(table, "Security Score", analysis.SecurityScore);
                            AddScoreRow(table, "Community Score", analysis.CommunityScore);
                            AddScoreRow(table, "Popularity Score", analysis.PopularityScore);
                            AddScoreRow(table, "Activity Score", analysis.ActivityScore);
                        });

                        if (analysis.Recommendations.Count > 0)
                        {
                            col.Item().PaddingTop(20).Text("Recommendations").FontSize(14).Bold();
                            foreach (var rec in analysis.Recommendations)
                                col.Item().PaddingTop(4).Text($"• {rec}").FontSize(10);
                        }

                        if (analysis.Warnings.Count > 0)
                        {
                            col.Item().PaddingTop(16).Text("Warnings").FontSize(14).Bold().FontColor(Colors.Orange.Medium);
                            foreach (var warn in analysis.Warnings)
                                col.Item().PaddingTop(4).Text(warn).FontSize(10).FontColor(Colors.Orange.Darken2);
                        }

                        if (!string.IsNullOrEmpty(analysis.AiSummary))
                        {
                            col.Item().PaddingTop(20).Text("AI Analysis").FontSize(14).Bold();
                            col.Item().PaddingTop(8).Text(analysis.AiSummary).FontSize(10);
                        }
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                        txt.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                        txt.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                        txt.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf(filePath);

            _logger.LogInformation("PDF report generated: {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed, falling back to Markdown");
            return await GenerateRepositoryMarkdownAsync(repo, analysis, outputPath, fileName, ct);
        }
    }

    private static void AddTableRow(TableDescriptor table, string k1, string v1, string k2, string v2)
    {
        table.Cell().Padding(4).Text(k1).Bold();
        table.Cell().Padding(4).Text(v1);
        table.Cell().Padding(4).Text(k2).Bold();
        table.Cell().Padding(4).Text(v2);
    }

    private static void AddScoreRow(TableDescriptor table, string label, double score)
    {
        var color = score >= 80 ? Colors.Green.Darken1 : score >= 60 ? Colors.Blue.Darken1 : score >= 40 ? Colors.Orange.Medium : Colors.Red.Medium;
        table.Cell().Padding(4).Text(label).Bold();
        table.Cell().Padding(4).Text($"{score:F1}/100").FontColor(color).Bold();
        table.Cell().Padding(4).Height(12).Background(Colors.Grey.Lighten3).Element(e =>
            e.Width((float)(score / 100 * 200)).Background(color));
    }

    private async Task<string> GenerateRepositoryMarkdownAsync(RepositoryInfo repo, RepositoryAnalysis analysis, string outputPath, string fileName, CancellationToken ct)
    {
        var md = BuildRepositoryMarkdown(repo, analysis);
        return await SaveTextAsync(md, outputPath, fileName + ".md", ct);
    }

    private async Task<string> GenerateRepositoryHtmlAsync(RepositoryInfo repo, RepositoryAnalysis analysis, string outputPath, string fileName, CancellationToken ct)
    {
        var md = BuildRepositoryMarkdown(repo, analysis);
        var html = ConvertToHtml(md, $"Repository Analysis: {repo.FullName}");
        return await SaveTextAsync(html, outputPath, fileName + ".html", ct);
    }

    private static string BuildRepositoryMarkdown(RepositoryInfo repo, RepositoryAnalysis analysis)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Repository Analysis: {repo.FullName}");
        sb.AppendLine();
        sb.AppendLine($"> Generated by OpenSourceHub v1.1.5 | {DateTime.Now:yyyy-MM-dd HH:mm} | © 2026 XinPari Software");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(repo.Description)) sb.AppendLine($"**{repo.Description}**\n");
        sb.AppendLine($"🔗 [{repo.HtmlUrl}]({repo.HtmlUrl})");
        sb.AppendLine();
        sb.AppendLine("## Repository Statistics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| ⭐ Stars | {repo.StargazersCount:N0} |");
        sb.AppendLine($"| 🍴 Forks | {repo.ForksCount:N0} |");
        sb.AppendLine($"| 👁️ Watchers | {repo.WatchersCount:N0} |");
        sb.AppendLine($"| 🐛 Open Issues | {repo.OpenIssuesCount:N0} |");
        sb.AppendLine($"| 💻 Language | {repo.Language ?? "N/A"} |");
        sb.AppendLine($"| 📄 License | {repo.License ?? "None"} |");
        sb.AppendLine($"| 📅 Created | {repo.CreatedAt:yyyy-MM-dd} |");
        sb.AppendLine($"| 🔄 Last Updated | {repo.UpdatedAt:yyyy-MM-dd} |");
        sb.AppendLine($"| 📦 Size | {repo.Size:N0} KB |");
        if (repo.Topics.Count > 0) sb.AppendLine($"| 🏷️ Topics | {string.Join(", ", repo.Topics)} |");
        sb.AppendLine();
        sb.AppendLine("## Health Scores");
        sb.AppendLine();
        sb.AppendLine("| Score | Value | Level |");
        sb.AppendLine("|-------|-------|-------|");
        sb.AppendLine($"| 🏥 Health Score | **{analysis.HealthScore:F1}/100** | {analysis.HealthLevel} |");
        sb.AppendLine($"| 🔧 Maintenance | {analysis.MaintenanceScore:F1}/100 | |");
        sb.AppendLine($"| 🛡️ Security | {analysis.SecurityScore:F1}/100 | |");
        sb.AppendLine($"| 👥 Community | {analysis.CommunityScore:F1}/100 | |");
        sb.AppendLine($"| 🌟 Popularity | {analysis.PopularityScore:F1}/100 | |");
        sb.AppendLine($"| ⚡ Activity | {analysis.ActivityScore:F1}/100 | |");
        sb.AppendLine();
        sb.AppendLine("## Activity Metrics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Commits/Month | {analysis.CommitFrequencyPerMonth} |");
        sb.AppendLine($"| Active Contributors | {analysis.ActiveContributors} |");
        sb.AppendLine($"| Open Issues | {analysis.OpenIssues} |");
        sb.AppendLine($"| Closed Issues | {analysis.ClosedIssues} |");
        sb.AppendLine($"| Open PRs | {analysis.OpenPullRequests} |");
        sb.AppendLine($"| Merged PRs | {analysis.MergedPullRequests} |");
        sb.AppendLine($"| Total Releases | {analysis.TotalReleases} |");
        sb.AppendLine($"| Branches | {analysis.BranchCount} |");
        sb.AppendLine($"| Days Since Last Commit | {analysis.DaysSinceLastCommit} |");
        sb.AppendLine($"| Issue Close Ratio | {analysis.IssueCloseRatio:P0} |");
        sb.AppendLine($"| PR Merge Ratio | {analysis.PrMergeRatio:P0} |");
        sb.AppendLine();
        sb.AppendLine("## Documentation");
        sb.AppendLine();
        sb.AppendLine("| Document | Status |");
        sb.AppendLine("|----------|--------|");
        sb.AppendLine($"| README | {(analysis.HasReadme ? "✅" : "❌")} |");
        sb.AppendLine($"| CONTRIBUTING | {(analysis.HasContributing ? "✅" : "❌")} |");
        sb.AppendLine($"| LICENSE | {(analysis.HasLicense ? "✅" : "❌")} |");
        sb.AppendLine($"| CODE OF CONDUCT | {(analysis.HasCodeOfConduct ? "✅" : "❌")} |");
        sb.AppendLine($"| SECURITY | {(analysis.HasSecurityPolicy ? "✅" : "❌")} |");

        if (analysis.Recommendations.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            foreach (var rec in analysis.Recommendations) sb.AppendLine($"- {rec}");
        }

        if (analysis.Warnings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## ⚠️ Warnings");
            sb.AppendLine();
            foreach (var warn in analysis.Warnings) sb.AppendLine($"- {warn}");
        }

        if (!string.IsNullOrEmpty(analysis.AiSummary))
        {
            sb.AppendLine();
            sb.AppendLine("## AI Analysis");
            sb.AppendLine();
            sb.AppendLine(analysis.AiSummary);
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine($"*Report generated by OpenSourceHub v1.1.5 on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        return sb.ToString();
    }

    private static string BuildOrganizationMarkdown(OrganizationInfo org)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Organization Report: {org.Login}");
        sb.AppendLine();
        sb.AppendLine($"> Generated by OpenSourceHub v1.1.5 | {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(org.Description)) sb.AppendLine($"**{org.Description}**\n");
        sb.AppendLine("## Organization Details");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Name | {org.Name ?? org.Login} |");
        sb.AppendLine($"| Public Repos | {org.PublicRepos} |");
        sb.AppendLine($"| Followers | {org.Followers} |");
        sb.AppendLine($"| Members | {org.MembersCount} |");
        sb.AppendLine($"| Verified | {(org.IsVerified ? "✅" : "❌")} |");
        if (org.CreatedAt.HasValue) sb.AppendLine($"| Created | {org.CreatedAt:yyyy-MM-dd} |");

        if (org.TopRepositories.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Top Repositories");
            sb.AppendLine();
            sb.AppendLine("| Repository | Stars | Forks | Language |");
            sb.AppendLine("|-----------|-------|-------|----------|");
            foreach (var r in org.TopRepositories.Take(10))
                sb.AppendLine($"| [{r.Name}]({r.HtmlUrl}) | {r.StargazersCount:N0} | {r.ForksCount:N0} | {r.Language ?? "N/A"} |");
        }
        return sb.ToString();
    }

    private static string BuildComparisonMarkdown(List<RepositoryInfo> repos, List<RepositoryAnalysis> analyses)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Repository Comparison Report");
        sb.AppendLine();
        sb.AppendLine($"> Generated by OpenSourceHub v1.1.5 | {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        sb.Append("| Metric |");
        foreach (var r in repos) sb.Append($" {r.Name} |");
        sb.AppendLine();
        sb.Append("|--------|");
        foreach (var _ in repos) sb.Append("--------|");
        sb.AppendLine();

        void Row(string metric, Func<RepositoryInfo, string> val)
        {
            sb.Append($"| {metric} |");
            foreach (var r in repos) sb.Append($" {val(r)} |");
            sb.AppendLine();
        }

        Row("⭐ Stars", r => r.StargazersCount.ToString("N0"));
        Row("🍴 Forks", r => r.ForksCount.ToString("N0"));
        Row("🐛 Open Issues", r => r.OpenIssuesCount.ToString("N0"));
        Row("💻 Language", r => r.Language ?? "N/A");
        Row("📄 License", r => r.License ?? "None");

        if (analyses.Count == repos.Count)
        {
            sb.AppendLine();
            sb.AppendLine("## Health Scores");
            sb.AppendLine();
            sb.Append("| Score |");
            foreach (var r in repos) sb.Append($" {r.Name} |");
            sb.AppendLine();
            sb.Append("|-------|");
            foreach (var _ in repos) sb.Append("--------|");
            sb.AppendLine();

            void ScoreRow(string metric, Func<RepositoryAnalysis, double> val)
            {
                sb.Append($"| {metric} |");
                for (int i = 0; i < analyses.Count; i++) sb.Append($" {val(analyses[i]):F1} |");
                sb.AppendLine();
            }

            ScoreRow("🏥 Health", a => a.HealthScore);
            ScoreRow("🔧 Maintenance", a => a.MaintenanceScore);
            ScoreRow("🛡️ Security", a => a.SecurityScore);
            ScoreRow("👥 Community", a => a.CommunityScore);
            ScoreRow("⚡ Activity", a => a.ActivityScore);
        }

        return sb.ToString();
    }

    private static string BuildSecurityMarkdown(RepositoryInfo repo, List<SecurityAlert> alerts)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Security Report: {repo.FullName}");
        sb.AppendLine();
        sb.AppendLine($"> Generated by OpenSourceHub v1.1.5 | {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        sb.AppendLine($"**Total Alerts:** {alerts.Count}");
        sb.AppendLine($"**Critical:** {alerts.Count(a => a.RiskLevel == SecurityRiskLevel.Critical)}");
        sb.AppendLine($"**High:** {alerts.Count(a => a.RiskLevel == SecurityRiskLevel.High)}");
        sb.AppendLine($"**Medium:** {alerts.Count(a => a.RiskLevel == SecurityRiskLevel.Medium)}");
        sb.AppendLine($"**Low:** {alerts.Count(a => a.RiskLevel == SecurityRiskLevel.Low)}");
        sb.AppendLine();

        if (alerts.Count == 0)
        {
            sb.AppendLine("✅ No security issues detected.");
        }
        else
        {
            sb.AppendLine("## Security Alerts");
            sb.AppendLine();
            foreach (var alert in alerts.OrderByDescending(a => a.RiskLevel))
            {
                sb.AppendLine($"### [{alert.RiskLevel}] {alert.Title}");
                sb.AppendLine();
                sb.AppendLine($"**Category:** {alert.Category}");
                sb.AppendLine();
                sb.AppendLine(alert.Description);
                if (!string.IsNullOrEmpty(alert.Recommendation))
                {
                    sb.AppendLine();
                    sb.AppendLine($"**Recommendation:** {alert.Recommendation}");
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private static string ConvertToHtml(string markdown, string title)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var body = Markdown.ToHtml(markdown, pipeline);
        var css = "body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 900px; margin: 0 auto; padding: 20px 40px; color: #24292e; line-height: 1.6; } h1 { color: #0366d6; border-bottom: 2px solid #0366d6; padding-bottom: 8px; } h2 { color: #24292e; border-bottom: 1px solid #e1e4e8; padding-bottom: 4px; margin-top: 24px; } table { border-collapse: collapse; width: 100%; margin: 16px 0; } th, td { border: 1px solid #dfe2e5; padding: 8px 12px; text-align: left; } th { background: #f6f8fa; font-weight: 600; } tr:nth-child(even) { background: #f6f8fa; } code { background: #f6f8fa; padding: 2px 6px; border-radius: 3px; font-family: monospace; } blockquote { border-left: 4px solid #0366d6; padding-left: 16px; color: #6a737d; margin: 0; } .footer { margin-top: 40px; padding-top: 16px; border-top: 1px solid #e1e4e8; color: #6a737d; font-size: 12px; text-align: center; }";
        return $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>{title}</title><style>{css}</style></head><body>{body}<div class=\"footer\">Generated by OpenSourceHub v1.1.5 &copy; 2026 XinPari Software</div></body></html>";
    }

    private static async Task<string> SaveTextAsync(string content, string outputPath, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(outputPath, fileName);
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, ct);
        return filePath;
    }
}
