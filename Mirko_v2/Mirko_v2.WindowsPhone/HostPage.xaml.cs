using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
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
        public HostPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            var navService = SimpleIoc.Default.GetInstance<GalaSoft.MvvmLight.Views.INavigationService>() as NavigationService;
            navService.Navigating += NavService_Navigating;
        }

        private void NavService_Navigating(object source, Utils.StringEventArgs newPage)
        {
            if(newPage.String == "SettingsPage")
                MainFrame.Background = Application.Current.Resources["SettingsBackground"] as SolidColorBrush;
            else
                MainFrame.Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush;
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            var statusBarDimensions = statusBar.OccludedRect;

            MainGrid.Margin = new Thickness(0, statusBarDimensions.Height, 0, 0);
        }
    }
}
