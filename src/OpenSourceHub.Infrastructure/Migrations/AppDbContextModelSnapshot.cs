using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenSourceHub.Infrastructure.Data;

#nullable disable

namespace OpenSourceHub.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.8");

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.AppLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Exception")
                        .HasColumnType("TEXT");

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("StackTrace")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("AppLogs");
                });

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.AppSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AiProvider")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CacheDurationMinutes")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableAnimations")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableAutoUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableCaching")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableNotifications")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableTelemetry")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Language")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MinimizeToTray")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OllamaEndpoint")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OllamaModel")
                        .HasColumnType("TEXT");

                    b.Property<string>("OpenAiApiKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("OpenAiModel")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReportsOutputPath")
                        .HasColumnType("TEXT");

                    b.Property<bool>("StartWithWindows")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Theme")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("AppSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AiProvider = 0,
                            CacheDurationMinutes = 30,
                            EnableAnimations = true,
                            EnableAutoUpdate = true,
                            EnableCaching = true,
                            EnableNotifications = true,
                            EnableTelemetry = false,
                            Language = 0,
                            MinimizeToTray = false,
                            OllamaEndpoint = "http://localhost:11434",
                            OllamaModel = "llama3.2",
                            OpenAiModel = "gpt-4o-mini",
                            StartWithWindows = false,
                            Theme = 0,
                            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                        });
                });

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.FavoriteRepository", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Language")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasColumnType("TEXT");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Stars")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("FavoriteRepositories");
                });

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.RepositoryAnalysis", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActiveContributors")
                        .HasColumnType("INTEGER");

                    b.Property<double>("ActivityScore")
                        .HasColumnType("REAL");

                    b.Property<string>("AiSummary")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("AnalyzedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("BranchCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClosedIssues")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CommitFrequencyPerMonth")
                        .HasColumnType("INTEGER");

                    b.Property<double>("CommunityScore")
                        .HasColumnType("REAL");

                    b.Property<int>("DaysSinceLastCommit")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DaysSinceLastRelease")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasCodeOfConduct")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasContributing")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasIssueTemplates")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasLicense")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasPrTemplates")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasReadme")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasSecurityPolicy")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HealthLevel")
                        .HasColumnType("INTEGER");

                    b.Property<double>("HealthScore")
                        .HasColumnType("REAL");

                    b.Property<double>("IssueCloseRatio")
                        .HasColumnType("REAL");

                    b.Property<double>("MaintenanceScore")
                        .HasColumnType("REAL");

                    b.Property<int>("MergedPullRequests")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OpenIssues")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OpenPullRequests")
                        .HasColumnType("INTEGER");

                    b.Property<double>("PopularityScore")
                        .HasColumnType("REAL");

                    b.Property<double>("PrMergeRatio")
                        .HasColumnType("REAL");

                    b.Property<string>("Recommendations")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RepositoryFullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("SecurityScore")
                        .HasColumnType("REAL");

                    b.Property<int>("TotalContributors")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TotalReleases")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Warnings")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RepositoryAnalyses");
                });

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.SecurityAlert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DetectedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsResolved")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Recommendation")
                        .HasColumnType("TEXT");

                    b.Property<string>("RepositoryFullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("RiskLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SecurityAlerts");
                });

            modelBuilder.Entity("OpenSourceHub.Domain.Entities.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Bio")
                        .HasColumnType("TEXT");

                    b.Property<string>("Blog")
                        .HasColumnType("TEXT");

                    b.Property<string>("Company")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<int>("Followers")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Following")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Location")
                        .HasColumnType("TEXT");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PublicGists")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PublicRepos")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TokenSavedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("TotalContributions")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UserProfiles");
                });
#pragma warning restore 612, 618
        }
    }
}
