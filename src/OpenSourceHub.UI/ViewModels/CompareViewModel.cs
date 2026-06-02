using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.ViewModels;

public partial class CompareViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;

    [ObservableProperty] private string _newRepoQuery = string.Empty;
    [ObservableProperty] private ObservableCollection<RepositoryInfo> _repositories = [];
    [ObservableProperty] private ObservableCollection<RepositoryAnalysis> _analyses = [];
    [ObservableProperty] private bool _hasResults;

    public CompareViewModel(IRepositoryService repoService) => _repoService = repoService;

    [RelayCommand]
    private async Task AddRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRepoQuery)) return;
        if (Repositories.Count >= 4)
        {
            Notifications.Warning("Maximum 4 repositories can be compared.");
            return;
        }

        var input = NewRepoQuery.Trim().Replace("https://github.com/", "").TrimEnd('/');
        var parts = input.Split('/');
        if (parts.Length < 2) { Notifications.Error("Invalid format. Use owner/repo."); return; }

        SetLoading(true, $"Loading {input}...");
        try
        {
            var repo = await _repoService.GetRepositoryAsync(parts[0], parts[1]);
            if (repo == null) { Notifications.Error("Repository not found."); return; }
            if (Repositories.Any(r => r.FullName == repo.FullName))
            {
                Notifications.Warning("Repository already added.");
                return;
            }

            Repositories.Add(repo);
            NewRepoQuery = string.Empty;

            var analysis = await _repoService.AnalyzeRepositoryAsync(parts[0], parts[1]);
            Analyses.Add(analysis);

            HasResults = Repositories.Count >= 2;
        }
        catch (Exception ex)
        {
            SetError($"Failed to add: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    [RelayCommand]
    private void RemoveRepository(RepositoryInfo? repo)
    {
        if (repo == null) return;
        var idx = Repositories.IndexOf(repo);
        Repositories.Remove(repo);
        if (idx < Analyses.Count) Analyses.RemoveAt(idx);
        HasResults = Repositories.Count >= 2;
    }

    [RelayCommand]
    private void ClearAll()
    {
        Repositories.Clear();
        Analyses.Clear();
        HasResults = false;
    }
}
