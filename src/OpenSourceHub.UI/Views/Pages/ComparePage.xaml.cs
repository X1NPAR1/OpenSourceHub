using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class ComparePage : Page
{
    public ComparePage(CompareViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
