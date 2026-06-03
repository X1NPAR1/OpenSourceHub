using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenSourceHub.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Exception = table.Column<string>(type: "TEXT", nullable: true),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Theme = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    AiProvider = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenAiApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    OpenAiModel = table.Column<string>(type: "TEXT", nullable: true),
                    OllamaEndpoint = table.Column<string>(type: "TEXT", nullable: false),
                    OllamaModel = table.Column<string>(type: "TEXT", nullable: true),
                    EnableNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableAnimations = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableAutoUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableTelemetry = table.Column<bool>(type: "INTEGER", nullable: false),
                    CacheDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableCaching = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReportsOutputPath = table.Column<string>(type: "TEXT", nullable: true),
                    StartWithWindows = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinimizeToTray = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteRepositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Owner = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Stars = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteRepositories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepositoryAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepositoryFullName = table.Column<string>(type: "TEXT", nullable: false),
                    HealthScore = table.Column<double>(type: "REAL", nullable: false),
                    MaintenanceScore = table.Column<double>(type: "REAL", nullable: false),
                    SecurityScore = table.Column<double>(type: "REAL", nullable: false),
                    CommunityScore = table.Column<double>(type: "REAL", nullable: false),
                    PopularityScore = table.Column<double>(type: "REAL", nullable: false),
                    ActivityScore = table.Column<double>(type: "REAL", nullable: false),
                    HealthLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CommitFrequencyPerMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveContributors = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalContributors = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenIssues = table.Column<int>(type: "INTEGER", nullable: false),
                    ClosedIssues = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenPullRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    MergedPullRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalReleases = table.Column<int>(type: "INTEGER", nullable: false),
                    BranchCount = table.Column<int>(type: "INTEGER", nullable: false),
                    HasReadme = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasContributing = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasLicense = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCodeOfConduct = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasSecurityPolicy = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasIssueTemplates = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPrTemplates = table.Column<bool>(type: "INTEGER", nullable: false),
                    DaysSinceLastCommit = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysSinceLastRelease = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueCloseRatio = table.Column<double>(type: "REAL", nullable: false),
                    PrMergeRatio = table.Column<double>(type: "REAL", nullable: false),
                    AiSummary = table.Column<string>(type: "TEXT", nullable: true),
                    Recommendations = table.Column<string>(type: "TEXT", nullable: false),
                    Warnings = table.Column<string>(type: "TEXT", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepositoryFullName = table.Column<string>(type: "TEXT", nullable: false),
                    RiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Recommendation = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Company = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Blog = table.Column<string>(type: "TEXT", nullable: true),
                    Followers = table.Column<int>(type: "INTEGER", nullable: false),
                    Following = table.Column<int>(type: "INTEGER", nullable: false),
                    PublicRepos = table.Column<int>(type: "INTEGER", nullable: false),
                    PublicGists = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalContributions = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: false),
                    TokenSavedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "AiProvider", "CacheDurationMinutes", "EnableAnimations", "EnableAutoUpdate", "EnableCaching", "EnableNotifications", "EnableTelemetry", "Language", "MinimizeToTray", "OllamaEndpoint", "OllamaModel", "OpenAiApiKey", "OpenAiModel", "ReportsOutputPath", "StartWithWindows", "Theme", "UpdatedAt" },
                values: new object[] { 1, 0, 30, true, true, true, true, false, 0, false, "http://localhost:11434", "llama3.2", null, "gpt-4o-mini", null, false, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLogs");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "FavoriteRepositories");

            migrationBuilder.DropTable(
                name: "RepositoryAnalyses");

            migrationBuilder.DropTable(
                name: "SecurityAlerts");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
