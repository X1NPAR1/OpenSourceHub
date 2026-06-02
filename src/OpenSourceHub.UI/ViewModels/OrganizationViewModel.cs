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

    public OrganizationViewModel(IOrganizationService orgService) => _orgService = orgService;

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        SetLoading(true, "Loading organization...");
        try
        {
            Organization = await _orgService.GetOrganizationAsync(SearchQuery.Trim());
            if (Organization == null)
            {
                Notifications.Error("Organization not found.");
                return;
            }

            var membersTask = _orgService.GetOrganizationMembersAsync(SearchQuery, 30);
            var reposTask = _orgService.GetOrganizationRepositoriesAsync(SearchQuery, 30);
            await Task.WhenAll(membersTask, reposTask);

            Members = new ObservableCollection<ContributorInfo>(await membersTask);
            Repositories = new ObservableCollection<RepositoryInfo>(await reposTask);
            Organization.MembersCount = Members.Count;
            Organization.TopRepositories = (await reposTask).Take(5).ToList();
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
