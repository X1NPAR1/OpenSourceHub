using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenSourceHub.Infrastructure.Data;

namespace OpenSourceHub.Tests;

/// <summary>
/// Test helpers for an isolated in-memory <see cref="AppDbContext"/>.
/// Services that take an <see cref="IServiceScopeFactory"/> (e.g. SettingsService,
/// GitHubAuthService) get one backed by a unique in-memory database per test.
/// </summary>
public static class TestDb
{
    public static AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    /// <summary>Builds a service provider whose AppDbContext uses a single shared in-memory database name.</summary>
    public static ServiceProvider NewProvider()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName));
        var provider = services.BuildServiceProvider();
        // Ensure the schema/seed exists.
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        return provider;
    }

    public static IServiceScopeFactory ScopeFactory(this ServiceProvider provider)
        => provider.GetRequiredService<IServiceScopeFactory>();
}
