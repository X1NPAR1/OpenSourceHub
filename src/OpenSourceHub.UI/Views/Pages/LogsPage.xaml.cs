using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class LogsPage : Page
{
    private readonly LogsViewModel _vm;

    public LogsPage(LogsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
    }

    protected override async void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        await _vm.LoadAsync();
    }
}
