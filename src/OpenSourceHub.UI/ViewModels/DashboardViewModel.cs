using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IGitHubAuthService _auth;
    private readonly IRepositoryService _repoService;

    [ObservableProperty] private UserProfile? _user;
    [ObservableProperty] private int _totalRepositories;
    [ObservableProperty] private long _totalStars;
    [ObservableProperty] private long _totalForks;
    [ObservableProperty] private int _totalOpenIssues;
    [ObservableProperty] private int _totalWatchers;
    [ObservableProperty] private int _followers;
    [ObservableProperty] private int _following;
    [ObservableProperty] private string _welcomeMessage = "Welcome!";
    [ObservableProperty] private ObservableCollection<RepositoryInfo> _topRepositories = [];
    [ObservableProperty] private ObservableCollection<RepositoryInfo> _recentRepositories = [];

    public DashboardViewModel(IGitHubAuthService auth, IRepositoryService repoService)
    {
        _auth = auth;
        _repoService = repoService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        SetLoading(true, "Loading dashboard...");
        try
        {
            User = await _auth.GetCurrentUserAsync();
            if (User == null) return;

            WelcomeMessage = $"Welcome back, {User.Name}!";
            Followers = User.Followers;
            Following = User.Following;
            TotalRepositories = User.PublicRepos;

            var repos = await _repoService.GetUserRepositoriesAsync(User.Login, RepositorySortBy.Stars, 30);
            TotalStars = repos.Sum(r => r.StargazersCount);
            TotalForks = repos.Sum(r => r.ForksCount);
            TotalOpenIssues = repos.Sum(r => r.OpenIssuesCount);
            TotalWatchers = repos.Sum(r => r.WatchersCount);

            TopRepositories = new ObservableCollection<RepositoryInfo>(
                repos.OrderByDescending(r => r.StargazersCount).Take(5));
            RecentRepositories = new ObservableCollection<RepositoryInfo>(
                repos.OrderByDescending(r => r.UpdatedAt).Take(10));
        }
        catch (Exception ex)
        {
            SetError($"Failed to load dashboard: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();
}
