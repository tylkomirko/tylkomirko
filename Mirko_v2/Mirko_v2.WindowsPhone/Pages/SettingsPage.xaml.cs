using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        }

        private async void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var msgBox = new MessageDialog("Zmiana stylu wymaga restartu aplikacji.", "Achtung!");
            await msgBox.ShowAsync();
        }
    }
}
