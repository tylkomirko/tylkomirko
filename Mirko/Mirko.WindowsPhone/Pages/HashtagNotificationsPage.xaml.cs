using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Linq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class HashtagNotificationsPage : UserControl, IHaveAppBar
    {
        public HashtagNotificationsPage()
        {
            this.InitializeComponent();
        }

        private CommandBar AppBar = null;
        private AppBarButton DeleteSelectedButton = null;

        public CommandBar CreateCommandBar()
        {
            AppBar = new CommandBar();
            var VM = this.DataContext as NotificationsViewModel;
            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;

            DeleteSelectedButton = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Delete),
                IsEnabled = false,
                Label = "usuń",
            };
            DeleteSelectedButton.Click += DeleteSelectedButton_Click;

            var deleteAll = new AppBarButton()
            {
                Label = "usuń wszystkie"
            };
            deleteAll.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = VM,
                Path = new PropertyPath("DeleteCurrentHashtagNotifications"),
            });

            AppBar.PrimaryCommands.Add(DeleteSelectedButton);
            AppBar.SecondaryCommands.Add(deleteAll);

            return AppBar;
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as NotificationsViewModel;
            var selectedItems = ListView.SelectedItems.Cast<NotificationViewModel>().Select(x => x.Data.ID);

            foreach (var item in selectedItems)
                VM.DeleteHashtagNotification.Execute(item);

            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            DeleteSelectedButton.IsEnabled = false;
        }

        private void ListView_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            if(ListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                DeleteSelectedButton.IsEnabled = true;
            }
            else
            {
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
            var VM = this.DataContext as NotificationsViewModel;

            VM.SelectedHashtagNotification = item;
            VM.GoToFlipPage.Execute(null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight + 49; // adjust for header
            ListView.Margin = new Thickness(0, -height, 0, 0);

            (ListView.Header as FrameworkElement).Height = height;
        }
    }
}
