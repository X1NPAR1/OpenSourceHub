using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class TrendingPage : Page
{
    private readonly TrendingViewModel _vm;

    public TrendingPage(TrendingViewModel vm)
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
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[TrendingPage] Load error: {ex}"); }
    }
}
