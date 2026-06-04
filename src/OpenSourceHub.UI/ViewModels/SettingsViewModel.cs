using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;
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
    [ObservableProperty] private string _claudeApiKey = string.Empty;
    [ObservableProperty] private string _claudeModel = "claude-sonnet-4-5";
    [ObservableProperty] private string _geminiApiKey = string.Empty;
    [ObservableProperty] private string _geminiModel = "gemini-2.0-flash";
    [ObservableProperty] private string _deepSeekApiKey = string.Empty;
    [ObservableProperty] private string _deepSeekModel = "deepseek-chat";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOllama), nameof(CurrentApiKey), nameof(CurrentModel))]
    private ObservableCollection<string> _availableModels = [];

    [ObservableProperty] private bool _enableNotifications = true;
    [ObservableProperty] private bool _enableAnimations = true;
    [ObservableProperty] private bool _enableAutoUpdate = true;
    [ObservableProperty] private bool _enableTelemetry;
    [ObservableProperty] private int _cacheDuration = 30;
    [ObservableProperty] private bool _enableCaching = true;
    [ObservableProperty] private string _reportsOutputPath = string.Empty;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _minimizeToTray;

    public string AppVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.2.5";

    public IReadOnlyList<AppLanguage> AvailableLanguages { get; } =
        Enum.GetValues<AppLanguage>().ToArray();

    public IReadOnlyList<AiProvider> AvailableProviders { get; } =
        Enum.GetValues<AiProvider>().ToArray();

    public bool IsOllama => SelectedAiProvider == AiProvider.Ollama;

    /// <summary>The API key field bound in the UI — routes to the selected provider's key.</summary>
    public string CurrentApiKey
    {
        get => SelectedAiProvider switch
        {
            AiProvider.Claude => ClaudeApiKey,
            AiProvider.Gemini => GeminiApiKey,
            AiProvider.DeepSeek => DeepSeekApiKey,
            _ => OpenAiApiKey
        };
        set
        {
            switch (SelectedAiProvider)
            {
                case AiProvider.Claude: ClaudeApiKey = value; break;
                case AiProvider.Gemini: GeminiApiKey = value; break;
                case AiProvider.DeepSeek: DeepSeekApiKey = value; break;
                default: OpenAiApiKey = value; break;
            }
        }
    }

    /// <summary>The selected model — routes to the selected provider's model field.</summary>
    public string CurrentModel
    {
        get => SelectedAiProvider switch
        {
            AiProvider.Claude => ClaudeModel,
            AiProvider.Gemini => GeminiModel,
            AiProvider.DeepSeek => DeepSeekModel,
            AiProvider.Ollama => OllamaModel,
            _ => OpenAiModel
        };
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            switch (SelectedAiProvider)
            {
                case AiProvider.Claude: ClaudeModel = value; break;
                case AiProvider.Gemini: GeminiModel = value; break;
                case AiProvider.DeepSeek: DeepSeekModel = value; break;
                case AiProvider.Ollama: OllamaModel = value; break;
                default: OpenAiModel = value; break;
            }
        }
    }

    partial void OnSelectedThemeChanged(AppTheme value) => Helpers.ThemeManager.Apply(value);

    partial void OnSelectedAiProviderChanged(AiProvider value)
    {
        var models = value switch
        {
            AiProvider.Claude => AiModelCatalog.Claude,
            AiProvider.Gemini => AiModelCatalog.Gemini,
            AiProvider.DeepSeek => AiModelCatalog.DeepSeek,
            AiProvider.Ollama => AiModelCatalog.Ollama,
            _ => AiModelCatalog.OpenAi
        };
        AvailableModels = new ObservableCollection<string>(models);
        OnPropertyChanged(nameof(IsOllama));
        OnPropertyChanged(nameof(CurrentApiKey));
        OnPropertyChanged(nameof(CurrentModel));
    }

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
        ClaudeApiKey = s.ClaudeApiKey ?? string.Empty;
        ClaudeModel = s.ClaudeModel ?? "claude-sonnet-4-5";
        GeminiApiKey = s.GeminiApiKey ?? string.Empty;
        GeminiModel = s.GeminiModel ?? "gemini-2.0-flash";
        DeepSeekApiKey = s.DeepSeekApiKey ?? string.Empty;
        DeepSeekModel = s.DeepSeekModel ?? "deepseek-chat";
        // Populate the cascading model list + key/model bindings for the loaded provider.
        OnSelectedAiProviderChanged(SelectedAiProvider);
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
            ClaudeApiKey = string.IsNullOrWhiteSpace(ClaudeApiKey) ? null : ClaudeApiKey,
            ClaudeModel = ClaudeModel,
            GeminiApiKey = string.IsNullOrWhiteSpace(GeminiApiKey) ? null : GeminiApiKey,
            GeminiModel = GeminiModel,
            DeepSeekApiKey = string.IsNullOrWhiteSpace(DeepSeekApiKey) ? null : DeepSeekApiKey,
            DeepSeekModel = DeepSeekModel,
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
