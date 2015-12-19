using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class DonationPage : Page
    {
        public DonationPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, args) =>
            {
                string brushKey = null;
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().SelectedTheme == ElementTheme.Dark)
                    brushKey = "SettingsBackgroundDark";
                else
                    brushKey = "SettingsBackgroundLight";

                LayoutRoot.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            };
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=EE3HQ6PYDEJNN"));
        }
    }
}
