using Microsoft.Extensions.DependencyInjection;
using OpenSourceHub.UI.Helpers;
using OpenSourceHub.UI.Services;
using OpenSourceHub.UI.ViewModels;
using OpenSourceHub.UI.Views.Pages;
using System.Windows;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using Page = System.Windows.Controls.Page;

namespace OpenSourceHub.UI.Views;

public partial class MainWindow : System.Windows.Window, OpenSourceHub.UI.IAppBootstrapper
{
    private readonly MainViewModel _vm;
    private readonly IServiceProvider _services;
    private Button? _activeNavBtn;
    private string? _currentTag;

    public MainWindow(MainViewModel vm, IServiceProvider services)
    {
        InitializeComponent();
        _vm = vm;
        _services = services;
        DataContext = vm;

        NotificationService.Instance.SetContainer(NotifContainer);
    }

    public async Task InitializeAsync()
    {
        await _vm.InitializeAsync();

        if (_vm.IsAuthenticated)
        {
            NavigateTo("Dashboard");
            SetActiveNav(DashboardBtn);
        }
        else
        {
            ShowSignIn();
        }
    }

    private void ShowSignIn()
    {
        var signInVm = _services.GetRequiredService<SignInViewModel>();
        signInVm.SignedIn += (_, _) =>
        {
            NavigateTo("Dashboard");
            SetActiveNav(DashboardBtn);
        };
        var page = new SignInPage(signInVm);
        NavigateWithTransition(page);
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var tag = btn.Tag as string;
        if (string.IsNullOrEmpty(tag) || tag == _currentTag) return;

        if (!_vm.IsAuthenticated)
        {
            ShowSignIn();
            return;
        }

        NavigateTo(tag);
        SetActiveNav(btn);
    }

    private void NavigateTo(string page)
    {
        _currentTag = page;
        _vm.CurrentPageTitle = GetPageTitle(page);

        Page? target = page switch
        {
            "Dashboard" => _services.GetRequiredService<DashboardPage>(),
            "Analyze" => _services.GetRequiredService<RepositoryAnalysisPage>(),
            "Trending" => _services.GetRequiredService<TrendingPage>(),
            "Compare" => _services.GetRequiredService<ComparePage>(),
            "Organizations" => _services.GetRequiredService<OrganizationPage>(),
            "Security" => _services.GetRequiredService<SecurityPage>(),
            "AI" => _services.GetRequiredService<AiPage>(),
            "Favorites" => _services.GetRequiredService<FavoritesPage>(),
            "Reports" => _services.GetRequiredService<ReportsPage>(),
            "Logs" => _services.GetRequiredService<LogsPage>(),
            "Settings" => _services.GetRequiredService<SettingsPage>(),
            _ => null
        };

        if (target != null) NavigateWithTransition(target);
    }

    private void NavigateWithTransition(Page page)
    {
        PageContainer.Opacity = 0;
        MainFrame.Navigate(page);

        AnimationHelper.FadeIn(PageContainer, 200);
        AnimationHelper.SlideInFromBottom(PageContainer, 15, 220);
    }

    private static string GetPageTitle(string tag) => tag switch
    {
        "Dashboard" => "Dashboard",
        "Analyze" => "Repository Analysis",
        "Trending" => "Trending",
        "Compare" => "Compare Repositories",
        "Organizations" => "Organizations",
        "Security" => "Security Center",
        "AI" => "AI Insights",
        "Favorites" => "Favorites",
        "Reports" => "Reports",
        "Logs" => "Application Logs",
        "Settings" => "Settings",
        _ => "OpenSourceHub"
    };

    private void SetActiveNav(Button btn)
    {
        if (_activeNavBtn != null)
            _activeNavBtn.Style = (Style)FindResource("NavItemStyle");
        btn.Style = (Style)FindResource("NavItemActiveStyle");
        _activeNavBtn = btn;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) ToggleMaximize();
        else DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => ToggleMaximize();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal : WindowState.Maximized;
        MaxButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
