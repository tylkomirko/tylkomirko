using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mirko.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DualHostPage : Page
    {
        private NavigationService NavService = null;

        public DualHostPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            ApplicationView.GetForCurrentView().VisibleBoundsChanged += HostPage_VisibleBoundsChanged;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += DualHostPage_BackRequested;

            NavService = SimpleIoc.Default.GetInstance<NavigationService>();
            NavService.Navigating += NavService_Navigating;
            SimpleIoc.Default.GetInstance<SettingsViewModel>().ThemeChanged += HostPage_ThemeChanged;
        }

        private void DualHostPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            NavService.BackPressed();
        }

        private void HostPage_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var appView = (sender as ApplicationView).VisibleBounds;
            var screen = Window.Current.Bounds;

            var appBarHeight = screen.Bottom - appView.Bottom;

            if(App.IsMobile)
                MainGrid.Margin = new Thickness(0, appView.Top, 0, appBarHeight);
        }

        private void HostPage_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            if (NavService.CurrentPageKey == "SettingsPage")
            {
                var brushKey = e.Theme == ElementTheme.Dark ? "SettingsBackgroundDark" : "SettingsBackgroundLight";
                MainFrame.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            }
        }

        private void NavService_Navigating(object source, Utils.StringEventArgs newPage)
        {
            string brushKey;
            if (newPage.String == "SettingsPage" || newPage.String == "DonationPage" || newPage.String == "BlacklistPage")
                brushKey = RequestedTheme == ElementTheme.Dark ? "SettingsBackgroundDark" : "SettingsBackgroundLight";
            else if (newPage.String == "NewEntryPage" || newPage.String == "AttachmentPage")
                brushKey = RequestedTheme == ElementTheme.Dark ? "NewEntryBackgroundDark" : "NewEntryBackgroundLight";
            else
                brushKey = RequestedTheme == ElementTheme.Dark ? "PageBackgroundDark" : "PageBackgroundLight";

            MainFrame.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
        }
    }
}
