using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class AppHeader : UserControl
    {
        public AppHeader()
        {
            this.InitializeComponent();
            this.Loaded += AppHeader_Loaded;
        }

        private void AppHeader_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();
            switch (navService.CurrentPageKey)
            {
                case "HashtagNotificationsPage":
                    PaintHash();
                    break;

                case "ConversationsPage":
                    PaintPM();
                    break;
            }
        }

        public void PaintHash()
        {
            this.HashTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }

        public void PaintAt()
        {
            this.AtTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }

        public void PaintPM()
        {
            this.PMTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }

        private void At_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        /*

        private void Hashtag_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (App.NotificationsViewModel.HashtagsCollection.Count == 0)
                App.NotificationsViewModel.UpdateHashtagDictionary();

            if (App.NotificationsViewModel.HashtagNotificationsCount == 0)
            {
                NavigateTo(this, new PageNavigationEventArgs(typeof(HashtagNotificationsListPage)));
            }
            else if (App.NotificationsViewModel.HashtagNotificationsCount == 1)
            {
                WykopAPI.JSON.Notification n = null;
                string hashtag = null;

                foreach (var tmp in App.NotificationsViewModel.HashtagsDictionary)
                {
                    var collection = tmp.Value;
                    if (collection.Count == 1)
                    {
                        n = collection[0];
                        hashtag = tmp.Key;
                    }
                }

                var json = EntryNavigationParameterExtensions.fromNotification(n, hashtag);
                NavigateTo(this, new PageNavigationEventArgs(typeof(FullscreenEntry), json));
            }
            else if(App.NotificationsViewModel.HashtagNotificationsCount > 1)
            {
                // check if all notifications relate to the same hashtag

                int nonZeroesFound = 0;
                string hashtag = null;
                foreach (var item in App.NotificationsViewModel.HashtagsCollection)
                {
                    if (item.Count != 0)
                    {
                        nonZeroesFound++;
                        hashtag = item.Name;

                        if (nonZeroesFound >= 2)
                            break;
                    }
                }

                if (nonZeroesFound <= 1) // all notifications belong to one tag
                {
                    NavigateTo(this, new PageNavigationEventArgs(typeof(HashtagNotificationsPage), hashtag));
                }
                else
                {
                    NavigateTo(this, new PageNavigationEventArgs(typeof(HashtagNotificationsListPage)));
                }

            }
        }

        private void At_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var param = new EntryNavigationParameter();

            if (App.NotificationsViewModel.AtNotificationsCount == 1)
            {
                WykopAPI.JSON.Notification notification = null;
                try
                {
                    notification = App.NotificationsViewModel.AtNotificationsStream.First(x => x.@new == true);
                }
                catch (Exception) { }

                if (notification == null)
                {
                    NavigateTo(this, new PageNavigationEventArgs(typeof(AtNotificationsList)));
                    return;
                }

                if (notification.type == "entry_comment_directed")
                {
                    param.body = notification.body;
                    param.id = notification.entry.id;
                    param.notification_id = notification.id;
                    param.is_notification = true;
                    param.is_comment = true;
                    param.comment_id = notification.comment.id;

                    NavigateTo(this, new PageNavigationEventArgs(typeof(FullscreenEntry), param.toString()));
                }
                else if (notification.type == "entry_directed")
                {
                    param.body = notification.body;
                    param.id = notification.entry.id;
                    param.notification_id = notification.id;
                    param.is_notification = true;

                    NavigateTo(this, new PageNavigationEventArgs(typeof(FullscreenEntry), param.toString()));
                }
                else
                {
                    NavigateTo(this, new PageNavigationEventArgs(typeof(AtNotificationsList)));
                }
            }
            else
            {
                // more than one notification
                // check if all notifications relate to the same entry
                var newNotifications = App.NotificationsViewModel.AtNotificationsStream.Where(x => x.@new);
                var entriesIDs = newNotifications.Select(x => x.entry.id).Distinct();
                if (entriesIDs.Count() == 1)
                {
                    // navigate to that entry
                    var notification = newNotifications.First();
                    param.body = notification.body;
                    param.id = notification.entry.id;
                    param.notification_id = notification.id;
                    param.is_notification = true;

                    NavigateTo(this, new PageNavigationEventArgs(typeof(FullscreenEntry), param.toString()));
                }
                else
                {
                    NavigateTo(this, new PageNavigationEventArgs(typeof(AtNotificationsList)));
                }
            }
        }

        private void PM_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (App.NotificationsViewModel.PMNotificationsCount == 1)
            {
                if (App.NotificationsViewModel.PMNotifications.Count > 0)
                {
                    var conversation = App.NotificationsViewModel.PMNotifications.First();
                    var param = new PMNavigationParameter()
                    {
                        UserName = conversation.author,
                        Sex = conversation.author_sex,
                        Group = conversation.author_group,
                    };

                    NavigateTo(this, new PageNavigationEventArgs(typeof(PMPage), param.toString()));
                }
                else
                {
                    App.NotificationsViewModel.PMNotificationsCount = 0;
                    NavigateTo(this, new PageNavigationEventArgs(typeof(PMList)));
                }
            }
            else
            {
                NavigateTo(this, new PageNavigationEventArgs(typeof(PMList)));
            }

        }

        private void Logo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigateTo(this, new PageNavigationEventArgs(typeof(PivotPage)));
        }

        public void PlayAnimation()
        {
            LogoAnimation.Begin();
            NotificationsAnimation.Begin();
        }

        public void PaintHash()
        {
            this.HashTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }

        public void PaintAt()
        {
            this.AtTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }

        public void PaintPM()
        {
            this.PMTB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 89, 23));
        }
         * */
    }
}
