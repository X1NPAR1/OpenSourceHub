using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class RepositoryAnalysisPage : Page
{
    public RepositoryAnalysisPage(RepositoryAnalysisViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
