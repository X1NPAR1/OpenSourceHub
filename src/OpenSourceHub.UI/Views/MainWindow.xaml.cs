using Microsoft.Extensions.DependencyInjection;
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

    public MainWindow(MainViewModel vm, IServiceProvider services)
    {
        InitializeComponent();
        _vm = vm;
        _services = services;
        DataContext = vm;
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
            var signInPage = _services.GetRequiredService<SignInPage>();
            var signInVm = _services.GetRequiredService<SignInViewModel>();
            signInVm.SignedIn += (_, _) =>
            {
                NavigateTo("Dashboard");
                SetActiveNav(DashboardBtn);
            };
            MainFrame.Navigate(signInPage);
        }
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            var tag = btn.Tag as string;
            if (!string.IsNullOrEmpty(tag))
            {
                if (!_vm.IsAuthenticated)
                {
                    var signIn = _services.GetRequiredService<SignInPage>();
                    MainFrame.Navigate(signIn);
                    return;
                }
                NavigateTo(tag);
                SetActiveNav(btn);
            }
        }
    }

    private void NavigateTo(string page)
    {
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

        if (target != null)
            MainFrame.Navigate(target);
    }

    private void SetActiveNav(Button btn)
    {
        if (_activeNavBtn != null)
            _activeNavBtn.Style = (Style)FindResource("NavItemStyle");
        btn.Style = (Style)FindResource("NavItemActiveStyle");
        _activeNavBtn = btn;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => ToggleMaximize();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        MaxButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
