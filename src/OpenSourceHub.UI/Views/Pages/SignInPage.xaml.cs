using OpenSourceHub.UI.ViewModels;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Views.Pages;

public partial class SignInPage : Page
{
    private readonly SignInViewModel _vm;

    public SignInPage(SignInViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.SignedIn += (_, _) =>
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        };
    }

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TokenBox.Focus();
    }

    private void TokenBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        _vm.Token = TokenBox.Password;
    }
}
