using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mirko_v2
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

            NavService = SimpleIoc.Default.GetInstance<GalaSoft.MvvmLight.Views.INavigationService>() as NavigationService;
            NavService.Navigating += NavService_Navigating;

            Windows.UI.ViewManagement.StatusBar.GetForCurrentView().Hiding += StatusBar_Moving;
            Windows.UI.ViewManagement.StatusBar.GetForCurrentView().Showing += StatusBar_Moving;
            MainGrid.Loaded += (s, e) => StatusBar_Moving(StatusBar.GetForCurrentView(), null);

            SimpleIoc.Default.GetInstance<SettingsViewModel>().ThemeChanged += HostPage_ThemeChanged;
        }

        private void HostPage_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            if(NavService.CurrentPageKey == "SettingsPage")
            {
                var brushKey = e.Theme == ElementTheme.Dark ? "SettingsBackgroundDark" : "SettingsBackgroundLight";
                MainFrame.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            }
        }

        private void StatusBar_Moving(StatusBar sender, object args)
        {
            var statusBar = sender as StatusBar;
            var statusBarDimensions = statusBar.OccludedRect;

            MainGrid.Margin = new Thickness(0, statusBarDimensions.Height, 0, 0);
        }

        private void NavService_Navigating(object source, Utils.StringEventArgs newPage)
        {
            string brushKey;
            if(newPage.String == "SettingsPage")
                brushKey = RequestedTheme == ElementTheme.Dark ? "SettingsBackgroundDark" : "SettingsBackgroundLight";
            else
                brushKey = RequestedTheme == ElementTheme.Dark ? "PageBackgroundDark" : "PageBackgroundLight";

            MainFrame.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
        }
    }
}
