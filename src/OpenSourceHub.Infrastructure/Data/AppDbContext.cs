using Microsoft.EntityFrameworkCore;
using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<RepositoryAnalysis> RepositoryAnalyses => Set<RepositoryAnalysis>();
    public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();
    public DbSet<FavoriteRepository> FavoriteRepositories => Set<FavoriteRepository>();
    public DbSet<AppLog> AppLogs => Set<AppLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RepositoryAnalysis>(e =>
        {
            e.Property(r => r.Recommendations).HasConversion(
                v => string.Join("|||", v),
                v => v == null ? new List<string>() : v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList());
            e.Property(r => r.Warnings).HasConversion(
                v => string.Join("|||", v),
                v => v == null ? new List<string>() : v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        modelBuilder.Entity<AppSettings>().HasData(new AppSettings { Id = 1 });
    }
}
