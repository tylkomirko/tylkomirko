using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class HashtagNotificationsPage : Page
    {
        private NotificationsViewModel VM
        {
            get { return DataContext as NotificationsViewModel; }
        }

        public HashtagNotificationsPage()
        {
            this.InitializeComponent();
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListView.SelectedItems.Cast<NotificationViewModel>().Select(x => x.Data.ID);

            foreach (var item in selectedItems)
                VM.DeleteHashtagNotification.Execute(item);

            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            DeleteSelectedButton.IsEnabled = false;
        }

        private void ListView_ItemLeftEdgeTapped(ListView sender, Controls.ListViewEdgeTappedEventArgs e)
        {
            ListView.SelectionMode = ListViewSelectionMode.Multiple;
            ListView.IsItemLeftEdgeTapEnabled = false;

            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            DeleteSelectedButton.IsEnabled = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListView.SelectedItems.Count == 0)
            {
                ListView.SelectionMode = ListViewSelectionMode.None;
                ListView.IsItemLeftEdgeTapEnabled = true;

                AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                DeleteSelectedButton.IsEnabled = false;
            }
        }

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ListView.SelectionMode == ListViewSelectionMode.Multiple)
                return;

            var grid = sender as Grid;
            var item = grid.DataContext as NotificationViewModel;

            VM.SelectedHashtagNotification = item;
            VM.GoToFlipPage.Execute(null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight + 39.2; // adjust for header
            ListView.Margin = new Thickness(0, -height, 0, 0);

            (ListView.Header as FrameworkElement).Height = height;
        }
    }
}
