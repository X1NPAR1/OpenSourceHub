using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GitHubAuthService> _logger;
    private GitHubClient? _client;
    private UserProfile? _currentUser;

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(CurrentToken);
    public string? CurrentToken { get; private set; }
    public event EventHandler<UserProfile?>? AuthStateChanged;

    public GitHubAuthService(IServiceScopeFactory scopeFactory, ILogger<GitHubAuthService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task<string> GetAuthorizationUrlAsync()
    {
        const string url = "https://github.com/settings/tokens/new?description=OpenSourceHub&scopes=repo,read:org,read:user,user:email";
        return Task.FromResult(url);
    }

    public async Task<UserProfile?> AuthenticateWithTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tempClient = new GitHubClient(new ProductHeaderValue("OpenSourceHub", "1.1.5"))
            {
                Credentials = new Credentials(token.Trim())
            };

            var ghUser = await tempClient.User.Current();
            var profile = MapToUserProfile(ghUser, token.Trim());

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = await db.UserProfiles.FirstOrDefaultAsync(u => u.Login == profile.Login, ct);
            if (existing == null)
                db.UserProfiles.Add(profile);
            else
            {
                UpdateUserProfile(existing, profile);
                profile = existing;
            }

            await db.SaveChangesAsync(ct);

            _client = tempClient;
            _currentUser = profile;
            CurrentToken = token.Trim();
            AuthStateChanged?.Invoke(this, _currentUser);
            _logger.LogInformation("User {Login} authenticated successfully", profile.Login);
            return profile;
        }
        catch (AuthorizationException ex)
        {
            _logger.LogWarning(ex, "GitHub authorization failed — bad credentials");
            throw new InvalidOperationException("Invalid or expired GitHub token. Please generate a new token.", ex);
        }
        catch (RateLimitExceededException ex)
        {
            _logger.LogWarning(ex, "GitHub rate limit exceeded");
            throw new InvalidOperationException("GitHub API rate limit exceeded. Please wait a moment and try again.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Authentication failed");
            throw new InvalidOperationException($"Authentication failed: {ex.Message}", ex);
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (_currentUser != null) return _currentUser;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var stored = await db.UserProfiles
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.TokenSavedAt)
                .FirstOrDefaultAsync(ct);

            if (stored == null || string.IsNullOrEmpty(stored.AccessToken))
                return null;

            var decrypted = DecryptToken(stored.AccessToken);
            if (string.IsNullOrWhiteSpace(decrypted))
                return null;

            return await AuthenticateWithTokenAsync(decrypted, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auto-login failed, requiring manual sign-in");
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await db.UserProfiles.FirstOrDefaultAsync(u => u.IsActive);
            if (user != null)
            {
                user.IsActive = false;
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out");
        }
        finally
        {
            _currentUser = null;
            CurrentToken = null;
            _client = null;
            AuthStateChanged?.Invoke(this, null);
        }
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
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
