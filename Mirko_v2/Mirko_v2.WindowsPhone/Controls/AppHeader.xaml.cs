using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class AppHeader : UserControl
    {
        public AppHeader()
        {
            this.InitializeComponent();
            DrawLogo();

            this.Loaded += AppHeader_Loaded;
        }

        private void AppHeader_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();
            switch (navService.CurrentPageKey)
            {
                case "HashtagSelectionPage":
                    PaintHash();
                    break;

                case "ConversationsPage":
                    PaintPM();
                    break;
            }
        }

        private void DrawLogo()
        {
            // check current date and show appropriate logo
            string insidePathData = "";
            string outlinePathData = "M157.9 210H83.4c-18.9-2-39.1-2.8-55.8-12.9c-15.8-9.7-23.8-28.2-24.8-46.2C0.2 113.1-3.7 73.7 7.6 37    C18.5 7.4 52.8 1.4 80.4 0l76.5 0c20.2 2.2 42.4 2.6 59.2 15.5c20.8 15.7 21.8 43.6 23.5 67.3v51.5c-1.5 22.3-4.8 48.2-25.1 61.6    C198.1 207.6 177.2 208 157.9 210z M166.2 181.6c13.4-0.9 29.5-2 37.9-14.2c7.8-12.6 8.2-28.1 9-42.5c0.2-23.3 1.6-46.9-3.6-69.8    c-2.7-17.6-20.7-26.2-36.9-26.3C134.7 27.1 96.5 26 58.7 30c-16.3 0.8-27.9 15-29.6 30.6c-3.2 22-2.3 44.3-2 66.4    c0.6 15.1 1.3 31.9 11.4 44.1c10.2 10.1 25.6 9.8 38.9 10.8C107.1 182.7 136.7 182.9 166.2 181.6z";

            var fill = Application.Current.Resources["LogoFill"] as SolidColorBrush;
            var outlinePath = new Path() { Height = 40, Width = 46, Stretch = Stretch.Uniform, Fill = fill };
            var insidePath = new Path() { Height = 28, Width = 32, Stretch = Stretch.Uniform, Fill = fill };

            var currentDate = DateTime.Now.Date;
            if (currentDate == new DateTime(2015, 2, 12) || currentDate == new DateTime(2016, 2, 4)) // donut day!
            {
                insidePathData = "M 119.8,56.4 C 85.7,56.4 58,78.2 58,105 58,105.3 58,105.7 58,106 62.2,110.2 66.5,113.8 68.2,114.1 69.6,114.3 72.2,113.7 74.7,113.1 78.9,112.1 83.3,111 86.3,112.8 89.1,114.5 91.6,118.6 94.2,122.9 96.6,126.9 99.6,131.9 101.6,132.2 103.4,132.5 106.9,129.6 109.8,127.3 113.4,124.4 116.8,121.6 119.9,121.6 L 119.9,121.6 C 123,121.6 126.4,124.4 130,127.3 132.8,129.6 136.4,132.5 138.2,132.2 140.3,131.9 143.2,126.9 145.6,122.9 148.2,118.5 150.6,114.4 153.4,112.8 156.4,111 160.8,112.1 165,113.1 167.5,113.7 170,114.3 171.5,114.1 173.2,113.8 177.5,110.2 181.7,106 181.7,105.7 181.7,105.3 181.7,105 181.7,78.2 154,56.4 119.8,56.4 Z M 138.4,86.7 C 134,82.4 129.7,78.1 125.3,73.7 123.5,74.1 121.8,74.5 120.1,74.9 124.4,79.2 128.9,83.5 133.2,87.9 129.6,88.8 126,89.6 122.4,90.5 118,86.2 113.7,81.9 109.3,77.5 107.5,77.9 105.8,78.3 104.1,78.7 108.1,82.9 112.5,87 116.5,91.1 116.7,91.2 116.8,91.5 116.8,91.7 116.8,91.9 116.8,92.9 116.8,93.2 116.8,94.9 115.6,96.3 114,96.3 113.9,96.3 113.8,96.3 113.7,96.3 113.3,97.5 112.3,98.3 111.2,98.6 L 111.2,101.8 C 111.2,102.2 110.9,102.6 110.5,102.6 110.1,102.6 109.8,102.3 109.8,101.8 L 109.8,98.6 C 108.2,98.2 107,96.7 107,94.8 L 107,94 C 105,93.8 102.9,93.2 102.9,91.8 102.9,91.6 103,91.4 103.1,91.3 98.8,87.1 94.6,82.9 90.4,78.7 104.7,75.3 119,71.9 133.3,68.5 138.7,73.7 143.9,78.9 149.1,84.1 145.6,85.1 142,85.9 138.4,86.7 Z";
                var insidePathData2 = "M 172.1 120.1c-2.2 0.4-5-0.3-8.1-1c-3-0.7-7-1.7-8.5-0.8c-1.7 1-4 4.9-6 8.3c-3.3 5.5-6.6 11.2-10.9 11.8  c-0.2 0-0.5 0-0.7 0c-3.3 0-6.9-2.9-10.7-6c-2.6-2.1-5.8-4.7-7.4-4.7v0c-1.6 0-4.8 2.6-7.4 4.7c-3.8 3.1-7.3 6-10.7 6  c-0.2 0-0.5 0-0.7 0c-4.2-0.6-7.6-6.3-10.9-11.8c-2-3.4-4.3-7.2-6-8.3c-1.5-0.9-5.6 0.1-8.5 0.8c-3 0.7-5.8 1.4-8.1 1  c-2.1-0.4-5.3-2.6-8.3-5.3c5.8 22.1 30.7 38.7 60.6 38.7c29.9 0 54.8-16.6 60.6-38.7C177.4 117.5 174.2 119.8 172.1 120.1z";

                insidePath = new Path() { Width = 25, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, -4, 0, 0) };
                var insidePath2 = new Path() { Width = 25, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, 13, 0, 0) };
                BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
                BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });
                BindingOperations.SetBinding(insidePath2, Path.DataProperty, new Binding() { Source = insidePathData2 });

                Logo.Children.Add(outlinePath);
                Logo.Children.Add(insidePath);
                Logo.Children.Add(insidePath2);
            }
            else if (currentDate.Month == 2 && currentDate.Day == 14) // valentine's day
            {
                outlinePathData = "M 153.4,27 C 182.4,27 206,50.5 206,79.5 206.1,146.3 116.6,182.1 116.6,182.1 116.6,182.1 27.1,146.5 27,79.8 27,50.8 50.5,27.2 79.5,27.1 79.5,27.1 79.5,27.1 79.6,27.1 94,27.1 107,32.9 116.5,42.2 126,32.8 139,27 153.4,27 153.4,27 153.4,27 153.4,27 M 153.4,0 L 153.4,0 C 140.3,0 127.7,3.2 116.4,9.1 105.2,3.2 92.6,0.1 79.6,0.1 L 79.5,0.1 C 35.6,0.1 0,35.9 0,79.8 0,115.8 19.2,149.5 55.4,177.4 80.5,196.7 105.6,206.8 106.7,207.2 L 116.7,211.2 126.7,207.2 C 127.8,206.8 152.9,196.6 177.9,177.2 214,149.3 233.1,115.5 233,79.5 233,35.7 197.3,0 153.4,0 L 153.4,0 Z";
                insidePathData = "M 110.7,144.9 C 103.3,148.5 96,152.3 88.4,155.7 77.4,133.6 66.5,111.4 55.7,89.1 85.2,74.4 114.9,60 144.5,45.5 155.6,67.6 166.4,89.9 177.3,112.1 170,116 162.5,119.6 155,123.1 145.8,104.7 136.9,86.2 127.8,67.8 124.1,69.5 120.5,71.2 117,73.1 126,91.6 135.2,109.9 144.1,128.4 136.7,132.2 129.3,135.8 121.8,139.4 112.7,121 103.7,102.5 94.6,84.2 91,85.8 87.4,87.5 83.8,89.2 92.4,108 102.1,126.2 110.7,144.9 Z";

                insidePath = new Path() { Width = 20, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, -1, 0, 0) };
                BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
                BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });

                Logo.Children.Add(outlinePath);
                Logo.Children.Add(insidePath);
            }
            else // regular logo
            {
                insidePathData = "M 111.8 161.7c-9.4 4.6-18.6 9.4-28.1 13.6c-14-27.9-27.7-56-41.3-84.1C79.7 72.6 117.2 54.4 154.6 36   c14 27.9 27.6 56.1 41.4 84.1c-9.3 5-18.7 9.5-28.2 14c-11.6-23.2-22.9-46.6-34.4-69.8c-4.6 2.1-9.2 4.3-13.7 6.7   c11.4 23.3 23 46.5 34.2 69.9c-9.3 4.8-18.7 9.3-28.2 13.9c-11.5-23.2-22.8-46.6-34.4-69.8c-4.6 2-9.1 4.2-13.6 6.4   C88.6 115 100.9 138 111.8 161.7z";
                BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
                BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });

                Logo.Children.Add(outlinePath);
                Logo.Children.Add(insidePath);
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
