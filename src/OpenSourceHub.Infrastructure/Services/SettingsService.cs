using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.Data;

namespace OpenSourceHub.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SettingsService> _logger;
    private AppSettings? _cache;

    public event EventHandler<AppSettings>? SettingsChanged;

    public SettingsService(IServiceScopeFactory scopeFactory, ILogger<SettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cache != null) return _cache;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cache = await db.AppSettings.FirstOrDefaultAsync() ?? new AppSettings { Id = 1 };
        return _cache;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await db.AppSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            settings.Id = 1;
            db.AppSettings.Add(settings);
        }
        else
        {
            existing.Theme = settings.Theme;
            existing.Language = settings.Language;
            existing.AiProvider = settings.AiProvider;
            existing.OpenAiApiKey = settings.OpenAiApiKey;
            existing.OpenAiModel = settings.OpenAiModel;
            existing.OllamaEndpoint = settings.OllamaEndpoint;
            existing.OllamaModel = settings.OllamaModel;
            existing.ClaudeApiKey = settings.ClaudeApiKey;
            existing.ClaudeModel = settings.ClaudeModel;
            existing.GeminiApiKey = settings.GeminiApiKey;
            existing.GeminiModel = settings.GeminiModel;
            existing.DeepSeekApiKey = settings.DeepSeekApiKey;
            existing.DeepSeekModel = settings.DeepSeekModel;
            existing.EnableNotifications = settings.EnableNotifications;
            existing.EnableAnimations = settings.EnableAnimations;
            existing.EnableAutoUpdate = settings.EnableAutoUpdate;
            existing.EnableTelemetry = settings.EnableTelemetry;
            existing.CacheDurationMinutes = settings.CacheDurationMinutes;
            existing.EnableCaching = settings.EnableCaching;
            existing.ReportsOutputPath = settings.ReportsOutputPath;
            existing.StartWithWindows = settings.StartWithWindows;
            existing.MinimizeToTray = settings.MinimizeToTray;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        _cache = settings;
        SettingsChanged?.Invoke(this, settings);
        _logger.LogInformation("Settings saved successfully");
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaults = new AppSettings { Id = 1 };
        await SaveSettingsAsync(defaults);
    }
}
