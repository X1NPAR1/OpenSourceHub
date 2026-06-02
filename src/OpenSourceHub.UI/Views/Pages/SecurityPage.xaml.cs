using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class SecurityPage : Page
{
    public SecurityPage(SecurityViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
