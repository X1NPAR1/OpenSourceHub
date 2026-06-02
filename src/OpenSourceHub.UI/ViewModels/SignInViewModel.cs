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

    public SignInViewModel(IGitHubAuthService auth) => _auth = auth;

    [RelayCommand]
    private async Task AuthenticateAsync()
    {
        TokenError = null;
        if (string.IsNullOrWhiteSpace(Token))
        {
            TokenError = "Please enter a GitHub token.";
            return;
        }

        SetLoading(true, "Authenticating...");
        try
        {
            var user = await _auth.AuthenticateWithTokenAsync(Token.Trim());
            if (user == null)
            {
                TokenError = "Authentication failed. Please check your token.";
                Notifications.Error("Invalid GitHub token.");
                return;
            }

            Notifications.Success($"Welcome, {user.Name}!");
            Token = string.Empty;
            SignedIn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            TokenError = $"Error: {ex.Message}";
            SetError("Authentication failed.");
        }
        finally
        {
            SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task OpenTokenPageAsync()
    {
        var url = await _auth.GetAuthorizationUrlAsync();
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
