using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.ViewModels;

public partial class OrganizationViewModel : BaseViewModel
{
    private readonly IOrganizationService _orgService;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private OrganizationInfo? _organization;
    [ObservableProperty] private ObservableCollection<ContributorInfo> _members = [];
    [ObservableProperty] private ObservableCollection<RepositoryInfo> _repositories = [];

    /// <summary>Curated popular organizations shown as quick-pick suggestions.</summary>
    public IReadOnlyList<PopularOrg> PopularOrganizations { get; } =
    [
        new("microsoft", "Microsoft"),
        new("google", "Google"),
        new("facebook", "Meta"),
        new("apple", "Apple"),
        new("amzn", "Amazon"),
        new("netflix", "Netflix"),
        new("docker", "Docker"),
        new("kubernetes", "Kubernetes"),
        new("vercel", "Vercel"),
        new("openai", "OpenAI"),
        new("github", "GitHub"),
        new("torvalds", "Linus Torvalds"),
    ];

    public OrganizationViewModel(IOrganizationService orgService) => _orgService = orgService;

    [RelayCommand]
    private async Task SelectOrgAsync(string? login)
    {
        if (string.IsNullOrWhiteSpace(login)) return;
        SearchQuery = login;
        await SearchAsync();
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (IsLoading) return;
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var org = SearchQuery.Trim();
        SetLoading(true, "Loading organization...");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var ct = cts.Token;
        try
        {
            var info = await _orgService.GetOrganizationAsync(org, ct);
            if (info == null)
            {
                Organization = null;
                SetError($"Organization \"{org}\" not found.");
                return;
            }
            Organization = info;

            try
            {
                var membersTask = _orgService.GetOrganizationMembersAsync(org, 30, ct);
                var reposTask = _orgService.GetOrganizationRepositoriesAsync(org, 30, ct);
                await Task.WhenAll(membersTask, reposTask);

                Members = new ObservableCollection<ContributorInfo>(membersTask.Result);
                Repositories = new ObservableCollection<RepositoryInfo>(
                    reposTask.Result.OrderByDescending(r => r.StargazersCount));
                Organization.MembersCount = Members.Count;
                Organization.TopRepositories = Repositories.Take(5).ToList();
            }
            catch (Exception ex)
            {
                Members = [];
                Repositories = [];
                Notifications.Warning($"Some organization details could not be loaded: {ex.Message}");
            }

            ClearError();
        }
        catch (OperationCanceledException)
        {
            SetError("Organization lookup timed out — GitHub may be slow or rate-limited.");
        }
        catch (Exception ex)
        {
            SetError($"Failed to load organization: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }
}

public record PopularOrg(string Login, string DisplayName);
