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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
