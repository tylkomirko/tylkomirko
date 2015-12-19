using GalaSoft.MvvmLight.Messaging;
using Mirko.ViewModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class LoginPage : Page
    {
        private LoginViewModel VM
        {
            get { return DataContext as LoginViewModel; }
        }

        public LoginPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, args) =>
            {
                InputPane.GetForCurrentView().Showing += InputPane_Showing;
                InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
            };
            this.Unloaded += (s, args) =>
            {
                InputPane.GetForCurrentView().Showing -= InputPane_Showing;
                InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
            };

            Messenger.Default.Register<NotificationMessage<string>>(this, ReadMessage);
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            CommandBarTransform.Y = -args.OccludedRect.Height;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            CommandBarTransform.Y = 0;
        }

        private void ReadMessage(NotificationMessage<string> obj)
        {
            if(obj.Notification == "LoginPageFlyout")
                ShowErrorFlyoutWithText(obj.Content);
        }

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
                LoginButton.IsEnabled = true;
            else
                LoginButton.IsEnabled = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
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

            VM.LoginCommand.Execute(null);
        }
    }
}
