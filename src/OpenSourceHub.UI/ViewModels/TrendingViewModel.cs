using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace OpenSourceHub.UI.ViewModels;

public partial class TrendingViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;

    [ObservableProperty] private ObservableCollection<RepositoryInfo> _trendingRepositories = [];
    [ObservableProperty] private TrendPeriod _selectedPeriod = TrendPeriod.Weekly;
    [ObservableProperty] private string? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<string> _popularLanguages = [];

    private static readonly List<string> Languages = [
        "All", "JavaScript", "TypeScript", "Python", "Java", "Go", "Rust", "C#",
        "C++", "C", "PHP", "Ruby", "Swift", "Kotlin", "Dart", "Shell",
        "HTML", "CSS", "Scala", "R", "Julia", "Elixir", "Haskell", "Clojure"
    ];

    public TrendingViewModel(IRepositoryService repoService)
    {
        _repoService = repoService;
        PopularLanguages = new ObservableCollection<string>(Languages);
        SelectedLanguage = "All";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        SetLoading(true, "Loading trending repositories...");
        try
        {
            var lang = SelectedLanguage == "All" ? null : SelectedLanguage;
            var repos = await _repoService.GetTrendingRepositoriesAsync(lang, SelectedPeriod, 30);
            TrendingRepositories = new ObservableCollection<RepositoryInfo>(repos);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load trending: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period switch
        {
            "Daily" => TrendPeriod.Daily,
            "Monthly" => TrendPeriod.Monthly,
            _ => TrendPeriod.Weekly
        };
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SelectLanguageAsync(string lang)
    {
        SelectedLanguage = lang;
        await LoadAsync();
    }

    [RelayCommand]
    private void OpenRepository(RepositoryInfo? repo)
    {
        if (repo != null)
            Process.Start(new ProcessStartInfo(repo.HtmlUrl) { UseShellExecute = true });
    }
}
