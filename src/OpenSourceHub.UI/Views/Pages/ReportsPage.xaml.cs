using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class ReportsPage : Page
{
    public ReportsPage(ReportsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
