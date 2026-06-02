using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Localization;

namespace OpenSourceHub.UI.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settings;
    private readonly ILogService _logService;
    private readonly IMemoryCache _cache;

    [ObservableProperty] private AppTheme _selectedTheme;
    [ObservableProperty] private AppLanguage _selectedLanguage;
    [ObservableProperty] private AiProvider _selectedAiProvider;
    [ObservableProperty] private string _openAiApiKey = string.Empty;
    [ObservableProperty] private string _openAiModel = "gpt-4o-mini";
    [ObservableProperty] private string _ollamaEndpoint = "http://localhost:11434";
    [ObservableProperty] private string _ollamaModel = "llama3.2";
    [ObservableProperty] private bool _enableNotifications = true;
    [ObservableProperty] private bool _enableAnimations = true;
    [ObservableProperty] private bool _enableAutoUpdate = true;
    [ObservableProperty] private bool _enableTelemetry;
    [ObservableProperty] private int _cacheDuration = 30;
    [ObservableProperty] private bool _enableCaching = true;
    [ObservableProperty] private string _reportsOutputPath = string.Empty;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _minimizeToTray;

    public SettingsViewModel(ISettingsService settings, ILogService logService, IMemoryCache cache)
    {
        _settings = settings;
        _logService = logService;
        _cache = cache;
        _reportsOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OpenSourceHub Reports");
        _settings.SettingsChanged += (_, s) => ApplyLanguage(s.Language);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var s = await _settings.GetSettingsAsync();
        SelectedTheme = s.Theme;
        SelectedLanguage = s.Language;
        SelectedAiProvider = s.AiProvider;
        OpenAiApiKey = s.OpenAiApiKey ?? string.Empty;
        OpenAiModel = s.OpenAiModel ?? "gpt-4o-mini";
        OllamaEndpoint = s.OllamaEndpoint;
        OllamaModel = s.OllamaModel ?? "llama3.2";
        EnableNotifications = s.EnableNotifications;
        EnableAnimations = s.EnableAnimations;
        EnableAutoUpdate = s.EnableAutoUpdate;
        EnableTelemetry = s.EnableTelemetry;
        CacheDuration = s.CacheDurationMinutes;
        EnableCaching = s.EnableCaching;
        ReportsOutputPath = s.ReportsOutputPath ?? ReportsOutputPath;
        StartWithWindows = s.StartWithWindows;
        MinimizeToTray = s.MinimizeToTray;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            Theme = SelectedTheme,
            Language = SelectedLanguage,
            AiProvider = SelectedAiProvider,
            OpenAiApiKey = string.IsNullOrWhiteSpace(OpenAiApiKey) ? null : OpenAiApiKey,
            OpenAiModel = OpenAiModel,
            OllamaEndpoint = OllamaEndpoint,
            OllamaModel = OllamaModel,
            EnableNotifications = EnableNotifications,
            EnableAnimations = EnableAnimations,
            EnableAutoUpdate = EnableAutoUpdate,
            EnableTelemetry = EnableTelemetry,
            CacheDurationMinutes = CacheDuration,
            EnableCaching = EnableCaching,
            ReportsOutputPath = ReportsOutputPath,
            StartWithWindows = StartWithWindows,
            MinimizeToTray = MinimizeToTray
        };

        await _settings.SaveSettingsAsync(settings);
        ApplyLanguage(SelectedLanguage);
        Notifications.Success("Settings saved successfully!");
        await _logService.LogAsync("Info", "Settings", "Settings saved by user");
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        await _settings.ResetToDefaultsAsync();
        await LoadAsync();
        Notifications.Info("Settings reset to defaults.");
    }

    [RelayCommand]
    private void ClearCache()
    {
        if (_cache is MemoryCache mc)
        {
            mc.Clear();
            Notifications.Success("Cache cleared.");
        }
        else
        {
            Notifications.Info("Cache cleared.");
        }
    }

    [RelayCommand]
    private async Task ExportLogsAsync()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var file = await _logService.ExportLogsAsync(path);
        Notifications.Success($"Logs exported to: {file}");
    }

    [RelayCommand]
    private void BrowseReportsPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Reports Output Folder",
            InitialDirectory = ReportsOutputPath
        };
        if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FolderName))
            ReportsOutputPath = dialog.FolderName;
    }

    private static void ApplyLanguage(AppLanguage lang)
        => LocalizationManager.Instance.CurrentLanguage = lang;
}
