using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;
using System.Windows;

namespace OpenSourceHub.UI.ViewModels;

public partial class AiViewModel : BaseViewModel
{
    private readonly IRepositoryService _repoService;
    private readonly AiServiceFactory _aiFactory;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private RepositoryInfo? _repository;
    [ObservableProperty] private RepositoryAnalysis? _analysis;
    [ObservableProperty] private string? _summary;
    [ObservableProperty] private string? _adoptionRecommendation;
    [ObservableProperty] private string? _improvementSuggestions;
    [ObservableProperty] private string _userQuestion = string.Empty;
    [ObservableProperty] private string? _questionAnswer;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _selectedTab = "Summary";

    public AiViewModel(IRepositoryService repoService, AiServiceFactory aiFactory)
    {
        _repoService = repoService;
        _aiFactory = aiFactory;
    }

    [RelayCommand]
    public async Task LoadRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        var parts = SearchQuery.Trim().Replace("https://github.com/", "").Split('/');
        if (parts.Length < 2) { Notifications.Error("Invalid format."); return; }

        SetLoading(true, "Loading repository...");
        try
        {
            Repository = await _repoService.GetRepositoryAsync(parts[0], parts[1]);
            if (Repository == null) { Notifications.Error("Repository not found."); return; }
            Analysis = await _repoService.AnalyzeRepositoryAsync(parts[0], parts[1]);
            Summary = null; AdoptionRecommendation = null; ImprovementSuggestions = null;
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private async Task GenerateSummaryAsync()
    {
        if (Repository == null) return;
        IsGenerating = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            Summary = await ai.GenerateRepositorySummaryAsync(Repository, Analysis);
        }
        finally { IsGenerating = false; }
    }

    [RelayCommand]
    private async Task GenerateAdoptionAsync()
    {
        if (Repository == null || Analysis == null) return;
        IsGenerating = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            AdoptionRecommendation = await ai.GenerateAdoptionRecommendationAsync(Repository, Analysis);
        }
        finally { IsGenerating = false; }
    }

    [RelayCommand]
    private async Task GenerateImprovementsAsync()
    {
        if (Repository == null || Analysis == null) return;
        IsGenerating = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            ImprovementSuggestions = await ai.GenerateImprovementSuggestionsAsync(Repository, Analysis);
        }
        finally { IsGenerating = false; }
    }

    [RelayCommand]
    private async Task AskQuestionAsync()
    {
        if (Repository == null || string.IsNullOrWhiteSpace(UserQuestion)) return;
        IsGenerating = true;
        try
        {
            var ai = await _aiFactory.GetServiceAsync();
            QuestionAnswer = await ai.AskAboutRepositoryAsync(Repository, UserQuestion);
        }
        finally { IsGenerating = false; }
    }

    [RelayCommand]
    private void CopyText(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Clipboard.SetText(text);
            Notifications.Success("Copied to clipboard!");
        }
    }
}
