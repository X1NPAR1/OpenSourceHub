using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface IGitHubAuthService
{
    Task<string> GetAuthorizationUrlAsync();
    Task<UserProfile?> AuthenticateWithTokenAsync(string token, CancellationToken ct = default);
    Task<UserProfile?> GetCurrentUserAsync(CancellationToken ct = default);
    Task SignOutAsync();
    bool IsAuthenticated { get; }
    string? CurrentToken { get; }
    event EventHandler<UserProfile?> AuthStateChanged;
}
