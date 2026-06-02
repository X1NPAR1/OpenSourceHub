using OpenSourceHub.UI.ViewModels;
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
    }

    protected override async void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        await _vm.LoadAsync();
        ApiKeyBox.Password = _vm.OpenAiApiKey;
    }
}
