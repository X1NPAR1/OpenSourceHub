using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;
using OpenSourceHub.Infrastructure.Data;
using OpenSourceHub.Infrastructure.GitHub;
using OpenSourceHub.Infrastructure.Services;
using System.IO;

namespace OpenSourceHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenSourceHub");
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "opensourcehub.db");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddHttpClient<OllamaService>();

        services.AddSingleton<IGitHubAuthService, GitHubAuthService>();
        services.AddScoped<IRepositoryService, GitHubRepositoryService>();
        services.AddScoped<IOrganizationService, GitHubOrganizationService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IFavoriteService, FavoriteService>();

        services.AddScoped<OpenAiService>();
        services.AddScoped<OllamaService>();
        services.AddScoped<AiServiceFactory>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
