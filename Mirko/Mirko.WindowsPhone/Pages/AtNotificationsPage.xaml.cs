using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class AtNotificationsPage : UserControl, IHaveAppBar
    {
        public AtNotificationsPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var notificationVM = e.ClickedItem as NotificationViewModel;
            if (notificationVM == null) return;

            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            VM.SelectedAtNotification = notificationVM;
            VM.GoToNotification.Execute(null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight;
            ListView.Margin = new Thickness(0, -height, 0, 0);

            var sp = ListView.Header as StackPanel;
            var rect = sp.GetDescendant<Rectangle>();
            rect.Height = height;
        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar() { ClosedDisplayMode = AppBarClosedDisplayMode.Minimal };

            var removeAll = new AppBarButton()
            {
                Label = "usuń wszystkie powiadomienia",
            };
            removeAll.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<NotificationsViewModel>(),
                Path = new PropertyPath("DeleteAllAtNotifications"),
            });

            c.SecondaryCommands.Add(removeAll);

            return c;
        }
    }
}
