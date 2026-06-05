using Microsoft.Extensions.Logging;
using Octokit;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Text;

namespace OpenSourceHub.Infrastructure.GitHub;

public class GitHubRepositoryManagementService : IRepositoryManagementService
{
    private readonly IGitHubAuthService _auth;
    private readonly ILogger<GitHubRepositoryManagementService> _logger;

    public GitHubRepositoryManagementService(IGitHubAuthService auth, ILogger<GitHubRepositoryManagementService> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    private GitHubClient CreateClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("OpenSourceHub", "1.4.0"));
        if (_auth.IsAuthenticated && _auth.CurrentToken != null)
            client.Credentials = new Credentials(_auth.CurrentToken);
        return client;
    }

    public async Task<List<GitBranch>> GetBranchesAsync(string owner, string repo, CancellationToken ct = default)
    {
        try
        {
            var branches = await CreateClient().Repository.Branch.GetAll(owner, repo);
            return branches.Select(b => new GitBranch
            {
                Name = b.Name,
                Sha = b.Commit?.Sha ?? "",
                IsProtected = b.Protected
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBranches failed for {Owner}/{Repo}", owner, repo);
            return [];
        }
    }

    public async Task<List<GitFileEntry>> GetFilesAsync(string owner, string repo, string path, string? branch = null, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            IReadOnlyList<RepositoryContent> contents = string.IsNullOrEmpty(branch)
                ? await client.Repository.Content.GetAllContents(owner, repo, string.IsNullOrEmpty(path) ? "/" : path)
                : await client.Repository.Content.GetAllContentsByRef(owner, repo, string.IsNullOrEmpty(path) ? "/" : path, branch);

            return contents
                .Select(c => new GitFileEntry
                {
                    Path = c.Path,
                    Name = c.Name,
                    Type = c.Type == ContentType.Dir ? "dir" : "file",
                    Size = c.Size,
                    Sha = c.Sha
                })
                .OrderByDescending(e => e.IsDirectory)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFiles failed for {Owner}/{Repo}/{Path}", owner, repo, path);
            return [];
        }
    }

    public async Task<GitFileContent?> GetFileContentAsync(string owner, string repo, string path, string? branch = null, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var contents = string.IsNullOrEmpty(branch)
                ? await client.Repository.Content.GetAllContents(owner, repo, path)
                : await client.Repository.Content.GetAllContentsByRef(owner, repo, path, branch);

            var file = contents.FirstOrDefault();
            if (file == null) return null;

            var isBinary = file.Content == null;
            return new GitFileContent
            {
                Path = file.Path,
                Sha = file.Sha,
                IsBinary = isBinary,
                Content = file.Content ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFileContent failed for {Owner}/{Repo}/{Path}", owner, repo, path);
            return null;
        }
    }

    public async Task<bool> SaveFileAsync(string owner, string repo, string path, string content, string commitMessage, string? sha, string? branch, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            if (string.IsNullOrEmpty(sha))
            {
                var req = new CreateFileRequest(commitMessage, content) { Branch = branch };
                await client.Repository.Content.CreateFile(owner, repo, path, req);
            }
            else
            {
                var req = new UpdateFileRequest(commitMessage, content, sha) { Branch = branch };
                await client.Repository.Content.UpdateFile(owner, repo, path, req);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveFile failed for {Owner}/{Repo}/{Path}", owner, repo, path);
            throw new InvalidOperationException(FriendlyError(ex), ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string owner, string repo, string path, string commitMessage, string sha, string? branch, CancellationToken ct = default)
    {
        try
        {
            var req = new DeleteFileRequest(commitMessage, sha) { Branch = branch };
            await CreateClient().Repository.Content.DeleteFile(owner, repo, path, req);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteFile failed for {Owner}/{Repo}/{Path}", owner, repo, path);
            throw new InvalidOperationException(FriendlyError(ex), ex);
        }
    }

    public async Task<bool> CreateBranchAsync(string owner, string repo, string newBranch, string fromBranch, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var baseRef = await client.Git.Reference.Get(owner, repo, $"heads/{fromBranch}");
            await client.Git.Reference.Create(owner, repo, new NewReference($"refs/heads/{newBranch}", baseRef.Object.Sha));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateBranch failed for {Owner}/{Repo}", owner, repo);
            throw new InvalidOperationException(FriendlyError(ex), ex);
        }
    }

    public async Task<List<PullRequestInfo>> GetPullRequestsAsync(string owner, string repo, string state = "open", CancellationToken ct = default)
    {
        try
        {
            var req = new PullRequestRequest
            {
                State = state.Equals("closed", StringComparison.OrdinalIgnoreCase) ? ItemStateFilter.Closed
                      : state.Equals("all", StringComparison.OrdinalIgnoreCase) ? ItemStateFilter.All
                      : ItemStateFilter.Open
            };
            var prs = await CreateClient().PullRequest.GetAllForRepository(owner, repo, req);
            return prs.Select(p => new PullRequestInfo
            {
                Number = p.Number,
                Title = p.Title,
                State = p.State.StringValue,
                Author = p.User?.Login ?? "",
                AuthorAvatarUrl = p.User?.AvatarUrl,
                HtmlUrl = p.HtmlUrl,
                BaseBranch = p.Base?.Ref ?? "",
                HeadBranch = p.Head?.Ref ?? "",
                IsDraft = p.Draft,
                Comments = p.Comments,
                CreatedAt = p.CreatedAt.UtcDateTime,
                UpdatedAt = p.UpdatedAt.UtcDateTime
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPullRequests failed for {Owner}/{Repo}", owner, repo);
            return [];
        }
    }

    private static string FriendlyError(Exception ex) => ex switch
    {
        AuthorizationException => "Not authorized. Your token needs the 'repo' scope to make changes.",
        ForbiddenException => "Forbidden — you may not have write access to this repository.",
        NotFoundException => "Not found — check the repository, path, and branch.",
        ApiValidationException v => v.ApiError?.Errors?.FirstOrDefault()?.Message ?? "GitHub rejected the request.",
        _ => ex.Message
    };
}
