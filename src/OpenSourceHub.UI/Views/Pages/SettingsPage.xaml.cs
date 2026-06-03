using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _vm;

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        ApiKeyBox.PasswordChanged += (_, _) => _vm.OpenAiApiKey = ApiKeyBox.Password;
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        try
        {
            await _vm.LoadAsync();
            ApiKeyBox.Password = _vm.OpenAiApiKey;
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SettingsPage] Load error: {ex}"); }
    }
}
