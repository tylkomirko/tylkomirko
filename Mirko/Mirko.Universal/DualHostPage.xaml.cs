using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            Messenger.Default.Register<NotificationMessage<bool>>("MediaElement DoubleTapped", MediaElementDoubleTapped);
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

        private void MediaElementDoubleTapped(NotificationMessage<bool> message)
        {
            bool isFullScreen = message.Content;
            SimpleIoc.Default.GetInstance<MainViewModel>().CanGoBack = !isFullScreen;

            var currentPage = NavService.CurrentFrame();
            CommandBar appBar = currentPage.GetDescendant<CommandBar>();

            if (isFullScreen)
            {
                StatusBarManager.HideStatusBar();

                if (appBar != null)
                    appBar.MakeInvisible();

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape |
                    DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait |
                    DisplayOrientations.PortraitFlipped;
            }
            else
            {
                StatusBarManager.ShowStatusBar();

                if (appBar != null)
                    appBar.MakeVisible();

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
            }
        }
    }
}
