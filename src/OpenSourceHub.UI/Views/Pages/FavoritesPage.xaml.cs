using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class FavoritesPage : Page
{
    private readonly FavoritesViewModel _vm;

    public FavoritesPage(FavoritesViewModel vm)
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
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[FavoritesPage] Load error: {ex}"); }
    }
}
