using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Localization;
using OpenSourceHub.UI.Services;
using System.Windows;

namespace OpenSourceHub.UI.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IGitHubAuthService _auth;
    private readonly ISettingsService _settings;
    public NavigationService Navigation { get; }
    public LocalizationManager Loc { get; } = LocalizationManager.Instance;

    [ObservableProperty]
    private UserProfile? _currentUser;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _selectedPage = "Dashboard";

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.Dark;

    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";

    public MainViewModel(IGitHubAuthService auth, ISettingsService settings, NavigationService navigation)
    {
        _auth = auth;
        _settings = settings;
        Navigation = navigation;

        _auth.AuthStateChanged += OnAuthStateChanged;
    }

    public async Task InitializeAsync()
    {
        var s = await _settings.GetSettingsAsync();
        CurrentTheme = s.Theme;
        Loc.CurrentLanguage = s.Language;

        var user = await _auth.GetCurrentUserAsync();
        CurrentUser = user;
        IsAuthenticated = user != null;
    }

    private void OnAuthStateChanged(object? sender, UserProfile? user)
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => OnAuthStateChanged(sender, user));
            return;
        }
        CurrentUser = user;
        IsAuthenticated = user != null;
    }

    partial void OnSelectedPageChanged(string value) { }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        await _auth.SignOutAsync();
        CurrentUser = null;
        IsAuthenticated = false;
        SelectedPage = "SignIn";
        Notifications.Info("Signed out successfully");
    }
}
