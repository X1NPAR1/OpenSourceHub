using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class RepositoryPage : Page
{
    private readonly RepositoryManagementViewModel _vm;

    public RepositoryPage(RepositoryManagementViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        try { await _vm.LoadAsync(); }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"[RepositoryPage] {ex}"); }
    }
}
