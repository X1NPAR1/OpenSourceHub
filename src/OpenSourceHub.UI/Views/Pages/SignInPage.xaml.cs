using OpenSourceHub.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSourceHub.UI.Views.Pages;

public partial class SignInPage : Page
{
    private readonly SignInViewModel _vm;

    public SignInPage(SignInViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SignInViewModel.IsTokenVisible))
                SyncTokenVisibility();
        };
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        TokenBox.Focus();
    }

    private void TokenBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.Token = TokenBox.Password;
        if (TokenTextBox.Text != TokenBox.Password)
            TokenTextBox.Text = TokenBox.Password;
    }

    private void TokenTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.Token = TokenTextBox.Text;
        if (TokenBox.Password != TokenTextBox.Text)
        {
            TokenBox.PasswordChanged -= TokenBox_PasswordChanged;
            TokenBox.Password = TokenTextBox.Text;
            TokenBox.PasswordChanged += TokenBox_PasswordChanged;
        }
    }

    private void SyncTokenVisibility()
    {
        if (_vm.IsTokenVisible)
        {
            TokenTextBox.Text = TokenBox.Password;
            TokenBox.Visibility = Visibility.Collapsed;
            TokenTextBox.Visibility = Visibility.Visible;
            TokenTextBox.Focus();
            TokenTextBox.SelectionStart = TokenTextBox.Text.Length;
        }
        else
        {
            TokenBox.Password = TokenTextBox.Text;
            TokenTextBox.Visibility = Visibility.Collapsed;
            TokenBox.Visibility = Visibility.Visible;
            TokenBox.Focus();
        }
    }

    private void TokenField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _vm.AuthenticateCommand.CanExecute(null))
        {
            e.Handled = true;
            _vm.AuthenticateCommand.Execute(null);
        }
    }
}
