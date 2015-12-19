using System;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.ViewModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Mirko.Pages;
using Mirko.Utils;
using Windows.Graphics.Display;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mirko
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HostPage : Page
    {
        private NavigationService NavService = null;

        public HostPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            NavService = SimpleIoc.Default.GetInstance<NavigationService>();
            NavService.Navigating += NavService_Navigating;

            ApplicationView.GetForCurrentView().VisibleBoundsChanged += HostPage_VisibleBoundsChanged;

            SimpleIoc.Default.GetInstance<SettingsViewModel>().ThemeChanged += HostPage_ThemeChanged;

            Messenger.Default.Register<NotificationMessage<bool>>("MediaElement DoubleTapped", MediaElementDoubleTapped);
        }

#if WINDOWS_UWP
        private void HostPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            e.Handled = true;
            NavService.BackPressed();
        }
#endif

        private void HostPage_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var appView = (sender as ApplicationView).VisibleBounds;
            var screen = Window.Current.Bounds;

            var appBarHeight = screen.Bottom - appView.Bottom;

            MainGrid.Margin = new Thickness(0, appView.Top, 0, appBarHeight);
        }

        private void HostPage_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            if(NavService.CurrentPageKey == "SettingsPage")
            {
                var brushKey = e.Theme == ElementTheme.Dark ? "SettingsBackgroundDark" : "SettingsBackgroundLight";
                this.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
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

            //MainFrame.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            this.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
        }

        private void MediaElementDoubleTapped(NotificationMessage<bool> message)
        {
            bool isFullScreen = message.Content;
            SimpleIoc.Default.GetInstance<MainViewModel>().CanGoBack = !isFullScreen;
            CommandBar appBar = null;

#if WINDOWS_PHONE_APP
            appBar = BottomAppBar as CommandBar;
#else
            var currentPage = NavService.CurrentFrame();
            appBar = currentPage.GetDescendant<CommandBar>();
#endif

            if(isFullScreen)
            {
                StatusBarManager.HideStatusBar();

                if(appBar != null)
                    appBar.MakeInvisible();

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape |
                    DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait |
                    DisplayOrientations.PortraitFlipped;
            }
            else
            {
                StatusBarManager.ShowStatusBar();

                if(appBar != null)
                    appBar.MakeVisible();

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
            }
        }
    }
}
