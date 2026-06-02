using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class AiPage : Page
{
    public AiPage(AiViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
