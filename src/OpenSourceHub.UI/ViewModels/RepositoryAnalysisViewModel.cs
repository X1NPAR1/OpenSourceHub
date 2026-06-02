using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace OpenSourceHub.UI.ViewModels;

public partial class RepositoryAnalysisViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;
    private readonly IFavoriteService _favoriteService;
    private readonly AiServiceFactory _aiFactory;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private RepositoryInfo? _repository;
    [ObservableProperty] private RepositoryAnalysis? _analysis;
    [ObservableProperty] private ObservableCollection<ContributorInfo> _contributors = [];
    [ObservableProperty] private ObservableCollection<SecurityAlert> _securityAlerts = [];
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isAnalyzing;
    [ObservableProperty] private string? _aiSummary;
    [ObservableProperty] private bool _isGeneratingAi;
    [ObservableProperty] private string _aiQuestion = string.Empty;
    [ObservableProperty] private string? _aiAnswer;

    public RepositoryAnalysisViewModel(IRepositoryService repoService, IFavoriteService favoriteService, AiServiceFactory aiFactory)
    {
        _repoService = repoService;
        _favoriteService = favoriteService;
        _aiFactory = aiFactory;
    }

    [RelayCommand]
    public async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var (owner, name) = ParseRepositoryInput(SearchQuery);
        if (owner == null || name == null)
        {
            Notifications.Error("Invalid repository format. Use 'owner/repo' or a GitHub URL.");
            return;
        }

        SetLoading(true, "Analyzing repository...");
        IsAnalyzing = true;
        try
        {
            Repository = await _repoService.GetRepositoryAsync(owner, name);
            if (Repository == null)
            {
                Notifications.Error("Repository not found.");
                return;
            }

            Analysis = await _repoService.AnalyzeRepositoryAsync(owner, name);
            IsFavorite = await _favoriteService.IsFavoriteAsync($"{owner}/{name}");

            var contributorsTask = _repoService.GetContributorsAsync(owner, name, 20);
            var alertsTask = _repoService.GetSecurityAlertsAsync(owner, name);

            await Task.WhenAll(contributorsTask, alertsTask);

            Contributors = new ObservableCollection<ContributorInfo>(await contributorsTask);
            SecurityAlerts = new ObservableCollection<SecurityAlert>(await alertsTask);

            Notifications.Success($"Analysis complete for {Repository.FullName}");
        }
        catch (Exception ex)
        {
            SetError($"Analysis failed: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task GenerateAiSummaryAsync()
    {
        if (Repository == null) return;
        IsGeneratingAi = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            AiSummary = await ai.GenerateRepositorySummaryAsync(Repository, Analysis);
            if (Analysis != null) Analysis.AiSummary = AiSummary;
        }
        catch (Exception ex)
        {
            AiSummary = $"AI analysis failed: {ex.Message}";
        }
        finally
        {
            IsGeneratingAi = false;
        }
    }

    [RelayCommand]
    private async Task AskAiAsync()
    {
        if (Repository == null || string.IsNullOrWhiteSpace(AiQuestion)) return;
        IsGeneratingAi = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            AiAnswer = await ai.AskAboutRepositoryAsync(Repository, AiQuestion);
        }
        finally
        {
            IsGeneratingAi = false;
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Repository == null || Analysis == null) return;
        if (IsFavorite)
        {
            await _favoriteService.RemoveFavoriteAsync(Repository.FullName);
            Notifications.Info($"Removed {Repository.Name} from favorites");
        }
        else
        {
            await _favoriteService.AddFavoriteAsync(new FavoriteRepository
            {
                FullName = Repository.FullName,
                Owner = Repository.Owner,
                Name = Repository.Name,
                Description = Repository.Description,
                Language = Repository.Language,
                Stars = Repository.StargazersCount
            });
            Notifications.Success($"Added {Repository.Name} to favorites");
        }
        IsFavorite = !IsFavorite;
    }

    [RelayCommand]
    private void OpenOnGitHub()
    {
        if (Repository != null)
            Process.Start(new ProcessStartInfo(Repository.HtmlUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void CopyCloneUrl()
    {
        if (Repository != null)
        {
            Clipboard.SetText(Repository.CloneUrl);
            Notifications.Success("Clone URL copied to clipboard!");
        }
    }

    private static (string? owner, string? name) ParseRepositoryInput(string input)
    {
        input = input.Trim();
        if (input.StartsWith("https://github.com/"))
        {
            input = input.Replace("https://github.com/", "").TrimEnd('/');
            if (input.EndsWith(".git")) input = input[..^4];
        }

        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return (parts[0], parts[1]);
        return (null, null);
    }
}
