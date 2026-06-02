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
    public async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        var parts = SearchQuery.Trim().Replace("https://github.com/", "").Split('/');
        if (parts.Length < 2) { Notifications.Error("Invalid format."); return; }

        SetLoading(true, "Running security analysis...");
        try
        {
            Repository = await _repoService.GetRepositoryAsync(parts[0], parts[1]);
            if (Repository == null) { Notifications.Error("Repository not found."); return; }

            var alertsTask = _repoService.GetSecurityAlertsAsync(parts[0], parts[1]);
            var analysisTask = _repoService.AnalyzeRepositoryAsync(parts[0], parts[1]);
            await Task.WhenAll(alertsTask, analysisTask);

            Analysis = await analysisTask;
            var alertList = await alertsTask;
            Alerts = new ObservableCollection<SecurityAlert>(alertList);

            CriticalCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Critical);
            HighCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.High);
            MediumCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Medium);
            LowCount = alertList.Count(a => a.RiskLevel == Domain.Enums.SecurityRiskLevel.Low);
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
