using GalaSoft.MvvmLight.Messaging;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class LoginPage : UserControl, IHaveAppBar
    {
        private AppBarButton LoginButton = null;

        public LoginPage()
        {
            this.InitializeComponent();

            Messenger.Default.Register<NotificationMessage<string>>(this, ReadMessage);
        }

        private void ReadMessage(NotificationMessage<string> obj)
        {
            if(obj.Notification == "LoginPageFlyout")
                ShowErrorFlyoutWithText(obj.Content);
        }

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
                LoginButton.IsEnabled = true;
            else
                LoginButton.IsEnabled = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
                LoginButton.IsEnabled = true;
            else
                LoginButton.IsEnabled = false;
        }

        private void UsernameBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Focus(FocusState.Programmatic);
        }

        private void ShowErrorFlyoutWithText(string txt)
        {
            if (txt == null) return;

            var flyout = this.Resources["ErrorFlyout"] as Flyout;
            var tb = flyout.Content as TextBlock;
            tb.Text = txt;

            flyout.ShowAt(this);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameBox.Text))
            {
                ShowErrorFlyoutWithText("Podaj nazwę użytkownika.");
                return;
            }
            else if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                ShowErrorFlyoutWithText("Podaj hasło.");
                return;
            }

            var VM = this.DataContext as LoginViewModel;
            VM.LoginCommand.Execute(null);
        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();
            c.IsSticky = true;

            var login = new AppBarButton()
            {
                Name = "LoginButton",
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "zaloguj",
                IsEnabled = false,
            };
            login.Click += LoginButton_Click;

            LoginButton = login;

            c.PrimaryCommands.Add(login);

            return c;
        }
    }
}
