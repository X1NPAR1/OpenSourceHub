using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace OpenSourceHub.UI.ViewModels;

public partial class RepositoryManagementViewModel : BaseViewModel
{
    private readonly IRepositoryManagementService _mgmt;
    private readonly IRepositoryService _repoService;
    private readonly IGitHubAuthService _auth;

    [ObservableProperty] private ObservableCollection<RepositoryInfo> _myRepositories = [];
    [ObservableProperty] private RepositoryInfo? _selectedRepository;
    [ObservableProperty] private ObservableCollection<GitBranch> _branches = [];
    [ObservableProperty] private GitBranch? _selectedBranch;
    [ObservableProperty] private ObservableCollection<GitFileEntry> _files = [];
    [ObservableProperty] private ObservableCollection<PullRequestInfo> _pullRequests = [];
    [ObservableProperty] private string _currentPath = string.Empty;

    // File editor
    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private string _editorPath = string.Empty;
    [ObservableProperty] private string _editorContent = string.Empty;
    [ObservableProperty] private string? _editorSha;
    [ObservableProperty] private string _commitMessage = string.Empty;
    [ObservableProperty] private bool _isNewFile;
    [ObservableProperty] private string _newBranchName = string.Empty;

    public bool CanGoUp => !string.IsNullOrEmpty(CurrentPath);
    public string OwnerLogin => SelectedRepository?.Owner ?? string.Empty;
    public string RepoName => SelectedRepository?.Name ?? string.Empty;

    public RepositoryManagementViewModel(IRepositoryManagementService mgmt, IRepositoryService repoService, IGitHubAuthService auth)
    {
        _mgmt = mgmt;
        _repoService = repoService;
        _auth = auth;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var user = await _auth.GetCurrentUserAsync();
            if (user == null) return;
            var repos = await _repoService.GetUserRepositoriesAsync(user.Login, RepositorySortBy.Updated, 100);
            MyRepositories = new ObservableCollection<RepositoryInfo>(repos);
        }
        catch (Exception ex)
        {
            Notifications.Warning($"Could not load your repositories: {ex.Message}");
        }
    }

    partial void OnSelectedRepositoryChanged(RepositoryInfo? value)
    {
        if (value != null) _ = OpenRepositoryAsync();
    }

    private async Task OpenRepositoryAsync()
    {
        if (SelectedRepository == null) return;
        SetLoading(true, "Loading repository...");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        try
        {
            CurrentPath = string.Empty;
            var branches = await _mgmt.GetBranchesAsync(OwnerLogin, RepoName, cts.Token);
            Branches = new ObservableCollection<GitBranch>(branches);
            SelectedBranch = branches.FirstOrDefault(b => b.Name == SelectedRepository.DefaultBranch)
                             ?? branches.FirstOrDefault();
            await RefreshFilesAsync();
            await RefreshPullRequestsAsync();
            OnPropertyChanged(nameof(OwnerLogin));
            OnPropertyChanged(nameof(RepoName));
        }
        catch (Exception ex)
        {
            SetError($"Failed to open repository: {ex.Message}");
        }
        finally { SetLoading(false); }
    }

    partial void OnSelectedBranchChanged(GitBranch? value)
    {
        if (value != null && SelectedRepository != null)
        {
            CurrentPath = string.Empty;
            _ = RefreshFilesAsync();
            _ = RefreshPullRequestsAsync();
        }
    }

    private async Task RefreshFilesAsync()
    {
        if (SelectedRepository == null) return;
        var files = await _mgmt.GetFilesAsync(OwnerLogin, RepoName, CurrentPath, SelectedBranch?.Name);
        Files = new ObservableCollection<GitFileEntry>(files);
        OnPropertyChanged(nameof(CanGoUp));
    }

    private async Task RefreshPullRequestsAsync()
    {
        if (SelectedRepository == null) return;
        var prs = await _mgmt.GetPullRequestsAsync(OwnerLogin, RepoName, "open");
        PullRequests = new ObservableCollection<PullRequestInfo>(prs);
    }

    [RelayCommand]
    private async Task OpenEntryAsync(GitFileEntry? entry)
    {
        if (entry == null || SelectedRepository == null) return;
        if (entry.IsDirectory)
        {
            CurrentPath = entry.Path;
            await RefreshFilesAsync();
        }
        else
        {
            SetLoading(true, "Loading file...");
            try
            {
                var content = await _mgmt.GetFileContentAsync(OwnerLogin, RepoName, entry.Path, SelectedBranch?.Name);
                if (content == null) { Notifications.Error("Could not load file."); return; }
                if (content.IsBinary) { Notifications.Warning("Binary files cannot be edited here."); return; }

                EditorPath = content.Path;
                EditorContent = content.Content;
                EditorSha = content.Sha;
                IsNewFile = false;
                CommitMessage = $"Update {entry.Name}";
                IsEditorOpen = true;
            }
            finally { SetLoading(false); }
        }
    }

    [RelayCommand]
    private async Task GoUpAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath)) return;
        var idx = CurrentPath.LastIndexOf('/');
        CurrentPath = idx > 0 ? CurrentPath[..idx] : string.Empty;
        await RefreshFilesAsync();
    }

    [RelayCommand]
    private void NewFile()
    {
        if (SelectedRepository == null) return;
        EditorPath = string.IsNullOrEmpty(CurrentPath) ? "new-file.txt" : $"{CurrentPath}/new-file.txt";
        EditorContent = string.Empty;
        EditorSha = null;
        IsNewFile = true;
        CommitMessage = "Add new file";
        IsEditorOpen = true;
    }

    [RelayCommand]
    private async Task SaveFileAsync()
    {
        if (SelectedRepository == null || string.IsNullOrWhiteSpace(EditorPath)) return;
        if (string.IsNullOrWhiteSpace(CommitMessage)) { Notifications.Warning("Enter a commit message."); return; }

        SetLoading(true, "Committing...");
        try
        {
            await _mgmt.SaveFileAsync(OwnerLogin, RepoName, EditorPath.Trim(), EditorContent,
                CommitMessage.Trim(), EditorSha, SelectedBranch?.Name);
            Notifications.Success($"Committed {EditorPath}");
            IsEditorOpen = false;
            await RefreshFilesAsync();
        }
        catch (Exception ex)
        {
            Notifications.Error(ex.Message);
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(GitFileEntry? entry)
    {
        if (entry == null || entry.IsDirectory || SelectedRepository == null) return;
        var confirm = MessageBox.Show($"Delete \"{entry.Path}\" and commit the change?",
            "Delete File", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        SetLoading(true, "Deleting...");
        try
        {
            await _mgmt.DeleteFileAsync(OwnerLogin, RepoName, entry.Path,
                $"Delete {entry.Name}", entry.Sha ?? "", SelectedBranch?.Name);
            Notifications.Success($"Deleted {entry.Path}");
            await RefreshFilesAsync();
        }
        catch (Exception ex)
        {
            Notifications.Error(ex.Message);
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private async Task CreateBranchAsync()
    {
        if (SelectedRepository == null || SelectedBranch == null) return;
        var name = NewBranchName.Trim();
        if (string.IsNullOrWhiteSpace(name)) { Notifications.Warning("Enter a branch name first."); return; }

        SetLoading(true, "Creating branch...");
        try
        {
            await _mgmt.CreateBranchAsync(OwnerLogin, RepoName, name, SelectedBranch.Name);
            Notifications.Success($"Created branch {name}");
            NewBranchName = string.Empty;
            var branches = await _mgmt.GetBranchesAsync(OwnerLogin, RepoName);
            Branches = new ObservableCollection<GitBranch>(branches);
            SelectedBranch = branches.FirstOrDefault(b => b.Name == name);
        }
        catch (Exception ex)
        {
            Notifications.Error(ex.Message);
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private void CancelEditor() => IsEditorOpen = false;

    [RelayCommand]
    private void OpenPullRequest(PullRequestInfo? pr)
    {
        if (pr == null) return;
        Process.Start(new ProcessStartInfo(pr.HtmlUrl) { UseShellExecute = true });
    }
}
