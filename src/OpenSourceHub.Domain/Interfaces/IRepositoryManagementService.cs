using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

/// <summary>
/// Write-capable repository operations (requires a token with the <c>repo</c> scope).
/// </summary>
public interface IRepositoryManagementService
{
    Task<List<GitBranch>> GetBranchesAsync(string owner, string repo, CancellationToken ct = default);
    Task<List<GitFileEntry>> GetFilesAsync(string owner, string repo, string path, string? branch = null, CancellationToken ct = default);
    Task<GitFileContent?> GetFileContentAsync(string owner, string repo, string path, string? branch = null, CancellationToken ct = default);

    /// <summary>Create or update a file and commit it. Pass <paramref name="sha"/> for updates, null/empty to create.</summary>
    Task<bool> SaveFileAsync(string owner, string repo, string path, string content, string commitMessage, string? sha, string? branch, CancellationToken ct = default);
    Task<bool> DeleteFileAsync(string owner, string repo, string path, string commitMessage, string sha, string? branch, CancellationToken ct = default);

    Task<bool> CreateBranchAsync(string owner, string repo, string newBranch, string fromBranch, CancellationToken ct = default);
    Task<List<PullRequestInfo>> GetPullRequestsAsync(string owner, string repo, string state = "open", CancellationToken ct = default);
}
