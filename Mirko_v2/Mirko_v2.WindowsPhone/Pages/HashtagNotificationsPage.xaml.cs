using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class HashtagNotificationsPage : UserControl, IHaveAppBar
    {
        public HashtagNotificationsPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as NotificationViewModel;
            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();

            VM.SelectedHashtagNotification = item;
            VM.GoToFlipPage.Execute(null);
        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();
            var VM = this.DataContext as NotificationsViewModel;
            c.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;

            var deleteAll = new AppBarButton()
            {
                Label = "usuń wszystkie"
            };
            deleteAll.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = VM,
                Path = new PropertyPath("DeleteCurrentHashtagNotifications"),
            });

            c.SecondaryCommands.Add(deleteAll);

            return c;
        }
    }
}
