using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class RepositoryAnalysisPage : Page
{
    private readonly RepositoryAnalysisViewModel _vm;

    public RepositoryAnalysisPage(RepositoryAnalysisViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        try { await _vm.LoadMyRepositoriesAsync(); }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"[AnalysisPage] {ex}"); }
    }
}
