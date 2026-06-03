using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Interfaces;
using System.Diagnostics;

namespace OpenSourceHub.UI.ViewModels;

public partial class SignInViewModel : BaseViewModel
{
    private readonly IGitHubAuthService _auth;
    public event EventHandler? SignedIn;

    [ObservableProperty] private string _token = string.Empty;
    [ObservableProperty] private string? _tokenError;
    [ObservableProperty] private bool _isTokenVisible;

    public SignInViewModel(IGitHubAuthService auth) => _auth = auth;

    [RelayCommand]
    private async Task AuthenticateAsync()
    {
        TokenError = null;

        var trimmed = Token.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            TokenError = "Please enter your GitHub Personal Access Token.";
            return;
        }

        if (trimmed.Length < 10)
        {
            TokenError = "Token appears too short. GitHub tokens are typically 40+ characters.";
            return;
        }

        SetLoading(true, "Connecting to GitHub...");
        try
        {
            var user = await _auth.AuthenticateWithTokenAsync(trimmed);
            if (user == null)
            {
                TokenError = "Authentication failed. Please verify your token has the required scopes: repo, read:org, read:user.";
                Notifications.Error("Authentication failed — check your token.");
                return;
            }

            Notifications.Success($"Welcome, {user.Name ?? user.Login}! 🎉");
            Token = string.Empty;
            IsTokenVisible = false;
            SignedIn?.Invoke(this, EventArgs.Empty);
        }
        catch (InvalidOperationException ex)
        {
            TokenError = ex.Message;
            Notifications.Error("Sign in failed.");
        }
        catch (Exception ex)
        {
            TokenError = $"Unexpected error: {ex.Message}";
            SetError("Authentication failed.");
        }
        finally
        {
            SetLoading(false);
        }
    }

    [RelayCommand]
    private void ToggleTokenVisibility() => IsTokenVisible = !IsTokenVisible;

    [RelayCommand]
    private async Task OpenTokenPageAsync()
    {
        var url = await _auth.GetAuthorizationUrlAsync();
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
