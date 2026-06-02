using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.Domain.Entities;

public class AppSettings
{
    public int Id { get; set; }
    public AppTheme Theme { get; set; } = AppTheme.System;
    public AppLanguage Language { get; set; } = AppLanguage.English;
    public AiProvider AiProvider { get; set; } = AiProvider.OpenAI;
    public string? OpenAiApiKey { get; set; }
    public string? OpenAiModel { get; set; } = "gpt-4o-mini";
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string? OllamaModel { get; set; } = "llama3.2";
    public bool EnableNotifications { get; set; } = true;
    public bool EnableAnimations { get; set; } = true;
    public bool EnableAutoUpdate { get; set; } = true;
    public bool EnableTelemetry { get; set; } = false;
    public int CacheDurationMinutes { get; set; } = 30;
    public bool EnableCaching { get; set; } = true;
    public string? ReportsOutputPath { get; set; }
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
    public DateTime UpdatedAt { get; set; }
}
