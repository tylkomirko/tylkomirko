using GalaSoft.MvvmLight.Threading;
using Mirko_v2.Utils;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                DayMode.Checked += ThemeRadioButton_Checked;
                NightMode.Checked += ThemeRadioButton_Checked;
            };

            this.Unloaded += async (s, e) =>
            {
                await StatusBarManager.ShowStatusBar();
            };
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
            {
                var msgBox = new MessageDialog("Zmiana stylu wymaga restartu aplikacji.", "Achtung!");
                await msgBox.ShowAsync();
            });
        }
    }
}
