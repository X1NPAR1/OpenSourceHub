using OpenSourceHub.UI.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _vm;
    private bool _syncingKey;

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        // PasswordBox cannot data-bind; route it to the provider-aware CurrentApiKey.
        ApiKeyBox.PasswordChanged += (_, _) =>
        {
            if (_syncingKey) return;
            _vm.CurrentApiKey = ApiKeyBox.Password;
        };
        _vm.PropertyChanged += OnVmPropertyChanged;

        Loaded += OnPageLoaded;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When the provider switches, CurrentApiKey changes — refresh the box.
        if (e.PropertyName is nameof(SettingsViewModel.CurrentApiKey)
            or nameof(SettingsViewModel.SelectedAiProvider))
        {
            SyncKeyBox();
        }
    }

    private void SyncKeyBox()
    {
        if (ApiKeyBox.Password == _vm.CurrentApiKey) return;
        _syncingKey = true;
        ApiKeyBox.Password = _vm.CurrentApiKey;
        _syncingKey = false;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        try
        {
            await _vm.LoadAsync();
            SyncKeyBox();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SettingsPage] Load error: {ex}"); }
    }
}
