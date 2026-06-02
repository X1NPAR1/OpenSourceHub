using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using System.Diagnostics;
using System.IO;

namespace OpenSourceHub.UI.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;
    private readonly IReportService _reportService;

    [ObservableProperty] private string _repositoryQuery = string.Empty;
    [ObservableProperty] private RepositoryInfo? _repository;
    [ObservableProperty] private RepositoryAnalysis? _analysis;
    [ObservableProperty] private ReportFormat _selectedFormat = ReportFormat.PDF;
    [ObservableProperty] private string _outputPath = string.Empty;
    [ObservableProperty] private string? _lastGeneratedFile;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _selectedReportType = "Repository";

    public ReportsViewModel(IRepositoryService repoService, IReportService reportService)
    {
        _repoService = repoService;
        _reportService = reportService;
        OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OpenSourceHub Reports");
    }

    [RelayCommand]
    private async Task LoadRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(RepositoryQuery)) return;
        var parts = RepositoryQuery.Trim().Replace("https://github.com/", "").Split('/');
        if (parts.Length < 2) { Notifications.Error("Invalid format."); return; }

        SetLoading(true, "Loading repository...");
        try
        {
            Repository = await _repoService.GetRepositoryAsync(parts[0], parts[1]);
            if (Repository == null) { Notifications.Error("Repository not found."); return; }
            Analysis = await _repoService.AnalyzeRepositoryAsync(parts[0], parts[1]);
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        if (Repository == null || Analysis == null)
        {
            Notifications.Warning("Please load a repository first.");
            return;
        }

        Directory.CreateDirectory(OutputPath);
        IsGenerating = true;
        try
        {
            var alerts = await _repoService.GetSecurityAlertsAsync(Repository.Owner, Repository.Name);
            LastGeneratedFile = SelectedReportType switch
            {
                "Security" => await _reportService.GenerateSecurityReportAsync(Repository, alerts, SelectedFormat, OutputPath),
                _ => await _reportService.GenerateRepositoryReportAsync(Repository, Analysis, SelectedFormat, OutputPath)
            };
            Notifications.Success($"Report generated: {Path.GetFileName(LastGeneratedFile)}");
        }
        catch (Exception ex)
        {
            SetError($"Report generation failed: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void OpenReport()
    {
        if (!string.IsNullOrEmpty(LastGeneratedFile) && File.Exists(LastGeneratedFile))
            Process.Start(new ProcessStartInfo(LastGeneratedFile) { UseShellExecute = true });
    }

    [RelayCommand]
    private void BrowseOutputPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Reports Output Folder",
            InitialDirectory = OutputPath
        };
        if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FolderName))
            OutputPath = dialog.FolderName;
    }
}
