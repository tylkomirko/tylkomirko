using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class HashtagFlipPage : UserControl
    {
        private double HeaderHeight;

        private NotificationsViewModel VM
        {
            get { return DataContext as NotificationsViewModel; }
        }

        public HashtagFlipPage()
        {
            this.InitializeComponent();

            HeaderHeight = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight + 49;

            FlipView.Margin = new Thickness(0, -HeaderHeight, 0, 0);
        }

        private void ListViewEx_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = sender as ListView;
            var sp = lv.Header as StackPanel;
            var rect = sp.GetDescendant<Rectangle>();
            rect.Height = HeaderHeight;
        }
    }
}
