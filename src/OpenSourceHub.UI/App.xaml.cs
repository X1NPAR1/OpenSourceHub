using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure;
using OpenSourceHub.Reporting.Services;
using OpenSourceHub.UI.Helpers;
using OpenSourceHub.UI.Services;
using OpenSourceHub.UI.ViewModels;
using OpenSourceHub.UI.Views;
using OpenSourceHub.UI.Views.Pages;
using System.Windows;

namespace OpenSourceHub.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseDefaultServiceProvider(opts =>
                {
                    opts.ValidateScopes = false;
                    opts.ValidateOnBuild = false;
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await _host.StartAsync();
            await DependencyInjection.InitializeDatabaseAsync(_host.Services);

            var settings = await _host.Services.GetRequiredService<ISettingsService>().GetSettingsAsync();
            ThemeManager.Apply(settings.Theme);

            var bootstrapper = _host.Services.GetRequiredService<IAppBootstrapper>();
            var window = (Window)bootstrapper;
            base.MainWindow = window;
            window.Show();
            await bootstrapper.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"OpenSourceHub başlatılamadı.\n\n{ex.GetType().Name}: {ex.Message}\n\nDetay:\n{ex.InnerException?.Message}",
                "Başlatma Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure();

        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<NavigationService>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<RepositoryAnalysisViewModel>();
        services.AddTransient<TrendingViewModel>();
        services.AddTransient<OrganizationViewModel>();
        services.AddTransient<CompareViewModel>();
        services.AddTransient<SecurityViewModel>();
        services.AddTransient<AiViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<FavoritesViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<LogsViewModel>();
        services.AddTransient<SignInViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<IAppBootstrapper>(sp => (IAppBootstrapper)sp.GetRequiredService<MainWindow>());

        services.AddTransient<DashboardPage>();
        services.AddTransient<RepositoryAnalysisPage>();
        services.AddTransient<TrendingPage>();
        services.AddTransient<OrganizationPage>();
        services.AddTransient<ComparePage>();
        services.AddTransient<SecurityPage>();
        services.AddTransient<AiPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<FavoritesPage>();
        services.AddTransient<ReportsPage>();
        services.AddTransient<LogsPage>();
        services.AddTransient<SignInPage>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
