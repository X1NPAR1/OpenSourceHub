using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
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
    private readonly IGitHubAuthService _auth;
    private readonly AiServiceFactory _aiFactory;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private RepositoryInfo? _repository;
    [ObservableProperty] private RepositoryAnalysis? _analysis;
    [ObservableProperty] private ObservableCollection<ContributorInfo> _contributors = [];
    [ObservableProperty] private ObservableCollection<SecurityAlert> _securityAlerts = [];
    [ObservableProperty] private ObservableCollection<RepositoryInfo> _myRepositories = [];
    [ObservableProperty] private RepositoryInfo? _selectedMyRepository;
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isAnalyzing;
    [ObservableProperty] private string? _aiSummary;
    [ObservableProperty] private bool _isGeneratingAi;
    [ObservableProperty] private string _aiQuestion = string.Empty;
    [ObservableProperty] private string? _aiAnswer;

    public RepositoryAnalysisViewModel(IRepositoryService repoService, IFavoriteService favoriteService,
        IGitHubAuthService auth, AiServiceFactory aiFactory)
    {
        _repoService = repoService;
        _favoriteService = favoriteService;
        _auth = auth;
        _aiFactory = aiFactory;
    }

    [RelayCommand]
    public async Task LoadMyRepositoriesAsync()
    {
        try
        {
            var user = await _auth.GetCurrentUserAsync();
            if (user == null) return;
            var repos = await _repoService.GetUserRepositoriesAsync(user.Login, RepositorySortBy.Updated, 50);
            MyRepositories = new ObservableCollection<RepositoryInfo>(repos);
        }
        catch (Exception ex)
        {
            Notifications.Warning($"Could not load your repositories: {ex.Message}");
        }
    }

    partial void OnSelectedMyRepositoryChanged(RepositoryInfo? value)
    {
        if (value == null) return;
        SearchQuery = value.FullName;
        _ = AnalyzeAsync();
    }

    [RelayCommand]
    public async Task AnalyzeAsync()
    {
        if (IsAnalyzing) return; // guard against re-entrancy
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var (owner, name) = ParseRepositoryInput(SearchQuery);
        if (owner == null || name == null)
        {
            Notifications.Error("Invalid repository format. Use 'owner/repo' or a GitHub URL.");
            return;
        }

        SetLoading(true, "Analyzing repository...");
        IsAnalyzing = true;

        // Hard timeout so the spinner can never hang forever (network stall / rate limit).
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

            // Core analysis (critical).
            Analysis = await _repoService.AnalyzeRepositoryAsync(owner, name, ct);
            IsFavorite = await _favoriteService.IsFavoriteAsync($"{owner}/{name}");

            // Secondary data — failures here must NOT abort the whole analysis.
            try
            {
                var contributorsTask = _repoService.GetContributorsAsync(owner, name, 20, ct);
                var alertsTask = _repoService.GetSecurityAlertsAsync(owner, name, ct);
                await Task.WhenAll(contributorsTask, alertsTask);
                Contributors = new ObservableCollection<ContributorInfo>(contributorsTask.Result);
                SecurityAlerts = new ObservableCollection<SecurityAlert>(alertsTask.Result);
            }
            catch (Exception ex)
            {
                Contributors = [];
                SecurityAlerts = [];
                Notifications.Warning($"Some details could not be loaded: {ex.Message}");
            }

            ClearError();
            Notifications.Success($"Analysis complete for {Repository.FullName}");
        }
        catch (OperationCanceledException)
        {
            SetError("Analysis timed out. GitHub may be slow or rate-limited — please try again.");
        }
        catch (Exception ex)
        {
            SetError($"Analysis failed: {ex.Message}");
        }
        finally
        {
            // ALWAYS reset state, even on timeout/exception — kills the infinite spinner.
            SetLoading(false);
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task GenerateAiSummaryAsync()
    {
        if (Repository == null) return;
        var ai = await _aiFactory.GetServiceAsync();
        if (!ai.IsAvailable)
        {
            Notifications.Warning("AI service is not available. Please configure it in Settings.");
            return;
        }
        IsGeneratingAi = true;
        try
        {
            AiSummary = await ai.GenerateRepositorySummaryAsync(Repository, Analysis);
            if (Analysis != null)
            {
                Analysis.AiSummary = AiSummary;
                await _repoService.SaveAnalysisAsync(Analysis);
            }
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
