using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.ViewModels;

public partial class SecurityViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;
    private readonly AiServiceFactory _aiFactory;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private RepositoryInfo? _repository;
    [ObservableProperty] private RepositoryAnalysis? _analysis;
    [ObservableProperty] private ObservableCollection<SecurityAlert> _alerts = [];
    [ObservableProperty] private string? _aiRiskReport;
    [ObservableProperty] private bool _isGeneratingReport;
    [ObservableProperty] private int _criticalCount;
    [ObservableProperty] private int _highCount;
    [ObservableProperty] private int _mediumCount;
    [ObservableProperty] private int _lowCount;

    public SecurityViewModel(IRepositoryService repoService, AiServiceFactory aiFactory)
    {
        _repoService = repoService;
        _aiFactory = aiFactory;
    }

    [RelayCommand]
    private async Task LoadDemoAsync()
    {
        SearchQuery = "OWASP/NodeGoat";
        await AnalyzeAsync();
    }

    [RelayCommand]
    public async Task AnalyzeAsync()
    {
        if (IsLoading) return;
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var (owner, name) = ParseRepositoryInput(SearchQuery);
        if (owner == null || name == null)
        {
            Notifications.Error("Invalid repository format. Use 'owner/repo' or a GitHub URL.");
            return;
        }

        SetLoading(true, "Running security analysis...");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var ct = cts.Token;
        try
        {
            var repo = await _repoService.GetRepositoryAsync(owner, name, ct);
            if (repo == null)
            {
                Repository = null;
                SetError("Repository not found. Check the owner/name and that you have access.");
                return;
            }
            Repository = repo;

            var alertsTask = _repoService.GetSecurityAlertsAsync(owner, name, ct);
            var analysisTask = _repoService.AnalyzeRepositoryAsync(owner, name, ct);
            await Task.WhenAll(alertsTask, analysisTask);

            Analysis = analysisTask.Result;
            var alertList = alertsTask.Result;
            Alerts = new ObservableCollection<SecurityAlert>(alertList);

            CriticalCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Critical);
            HighCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.High);
            MediumCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Medium);
            LowCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Low);

            ClearError();
        }
        catch (OperationCanceledException)
        {
            SetError("Security analysis timed out — GitHub may be slow or rate-limited.");
        }
        catch (Exception ex)
        {
            SetError($"Security analysis failed: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private static (string? owner, string? name) ParseRepositoryInput(string input)
    {
        input = input.Trim();
        if (input.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
        {
            input = input["https://github.com/".Length..].TrimEnd('/');
            if (input.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) input = input[..^4];
        }
        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? (parts[0], parts[1]) : (null, null);
    }

    [RelayCommand]
    private async Task GenerateAiReportAsync()
    {
        if (Repository == null || Analysis == null) return;
        IsGeneratingReport = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            AiRiskReport = await ai.GenerateRiskReportAsync(Repository, Analysis, Alerts.ToList());
        }
        finally
        {
            IsGeneratingReport = false;
        }
    }
}
