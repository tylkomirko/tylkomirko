using Mirko.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        private void ListViewEx_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = sender as ListViewEx;
            lv.AppBar = AppBar;
        }
    }
}
