using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class BlacklistPage : Page
    {
        private BlacklistViewModel VM
        {
            get { return DataContext as BlacklistViewModel; }
        }

        public BlacklistPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, args) =>
            {
                string brushKey = null;
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().SelectedTheme == ElementTheme.Dark)
                    brushKey = "SettingsBackgroundDark";
                else
                    brushKey = "SettingsBackgroundLight";

                MainGrid.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            };
        }
    }
}
