using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class OrganizationPage : Page
{
    public OrganizationPage(OrganizationViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
