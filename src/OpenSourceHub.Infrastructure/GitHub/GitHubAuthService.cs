using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace OpenSourceHub.Infrastructure.GitHub;

public class GitHubAuthService : IGitHubAuthService
{
    private readonly AppDbContext _db;
    private readonly ILogger<GitHubAuthService> _logger;
    private GitHubClient? _client;
    private UserProfile? _currentUser;

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(CurrentToken);
    public string? CurrentToken { get; private set; }
    public event EventHandler<UserProfile?>? AuthStateChanged;

    public GitHubAuthService(AppDbContext db, ILogger<GitHubAuthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task<string> GetAuthorizationUrlAsync()
    {
        var url = "https://github.com/settings/tokens/new?description=OpenSourceHub&scopes=repo,read:org,read:user,user:email";
        return Task.FromResult(url);
    }

    public async Task<UserProfile?> AuthenticateWithTokenAsync(string token, CancellationToken ct = default)
    {
        try
        {
            _client = new GitHubClient(new ProductHeaderValue("OpenSourceHub", "1.1.5"))
            {
                Credentials = new Credentials(token)
            };

            var ghUser = await _client.User.Current();
            var profile = MapToUserProfile(ghUser, token);

            var existing = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Login == profile.Login, ct);
            if (existing == null)
                _db.UserProfiles.Add(profile);
            else
            {
                UpdateUserProfile(existing, profile);
                profile = existing;
            }

            await _db.SaveChangesAsync(ct);
            _currentUser = profile;
            CurrentToken = token;
            AuthStateChanged?.Invoke(this, _currentUser);
            _logger.LogInformation("User {Login} authenticated successfully", profile.Login);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return null;
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (_currentUser != null) return _currentUser;

        var stored = await _db.UserProfiles.Where(u => u.IsActive).OrderByDescending(u => u.TokenSavedAt).FirstOrDefaultAsync(ct);
        if (stored != null && !string.IsNullOrEmpty(stored.AccessToken))
        {
            var decrypted = DecryptToken(stored.AccessToken);
            return await AuthenticateWithTokenAsync(decrypted, ct);
        }
        return null;
    }

    public async Task SignOutAsync()
    {
        var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.IsActive);
        if (user != null)
        {
            user.IsActive = false;
            await _db.SaveChangesAsync();
        }
        _currentUser = null;
        CurrentToken = null;
        _client = null;
        AuthStateChanged?.Invoke(this, null);
    }

    private UserProfile MapToUserProfile(Octokit.User ghUser, string token) => new()
    {
        Login = ghUser.Login,
        Name = ghUser.Name ?? ghUser.Login,
        Email = ghUser.Email,
        Bio = ghUser.Bio,
        AvatarUrl = ghUser.AvatarUrl,
        Company = ghUser.Company,
        Location = ghUser.Location,
        Blog = ghUser.Blog,
        Followers = ghUser.Followers,
        Following = ghUser.Following,
        PublicRepos = ghUser.PublicRepos,
        PublicGists = ghUser.PublicGists,
        CreatedAt = ghUser.CreatedAt.UtcDateTime,
        UpdatedAt = ghUser.UpdatedAt.UtcDateTime,
        AccessToken = EncryptToken(token),
        TokenSavedAt = DateTime.UtcNow,
        IsActive = true
    };

    private static void UpdateUserProfile(UserProfile existing, UserProfile updated)
    {
        existing.Name = updated.Name;
        existing.Email = updated.Email;
        existing.Bio = updated.Bio;
        existing.AvatarUrl = updated.AvatarUrl;
        existing.Company = updated.Company;
        existing.Location = updated.Location;
        existing.Blog = updated.Blog;
        existing.Followers = updated.Followers;
        existing.Following = updated.Following;
        existing.PublicRepos = updated.PublicRepos;
        existing.UpdatedAt = updated.UpdatedAt;
        existing.AccessToken = updated.AccessToken;
        existing.TokenSavedAt = updated.TokenSavedAt;
        existing.IsActive = true;
    }

    private static string EncryptToken(string token)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(token);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }
        catch
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }
    }

    private static string DecryptToken(string encrypted)
    {
        try
        {
            var data = Convert.FromBase64String(encrypted);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
        }
    }
}
