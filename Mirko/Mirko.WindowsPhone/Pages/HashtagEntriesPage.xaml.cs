using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Controls;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class HashtagEntriesPage : Page
    {
        private bool CanShowNewEntriesPopup = false;

        public HashtagEntriesPage()
        {
            this.InitializeComponent();

            var VM = this.DataContext as MainViewModel;
            var height = VM.ListViewHeaderHeight + 49; // adjust for header
            ListView.Margin = new Thickness(10, -height, 0, 0);
            var rect = (ListView.Header as FrameworkElement).GetDescendant<Rectangle>();
            rect.Height = height;

            VM.TaggedNewEntries.CollectionChanged -= TaggedNewEntries_CollectionChanged;
            VM.TaggedNewEntries.CollectionChanged += TaggedNewEntries_CollectionChanged;

            Messenger.Default.Register<NotificationMessage>(this, async (m) =>
            {
                if (m.Notification == "HashtagEntriesPage reload")
                {
                    if (ListView.ItemsSource != null)
                        await ListView.LoadMoreItemsAsync(); // agrhrgrrrhhr... Satya....
                }
            });
        }

        private void TaggedNewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var col = sender as ObservableCollectionEx<EntryViewModel>;
            if (col.Count > 0)
            {
                CanShowNewEntriesPopup = true;
                PopupFadeIn.Begin();
            }
            else
            {
                CanShowNewEntriesPopup = false;
                PopupFadeOut.Begin();
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            var popupGrid = NewEntriesPopup.Child as Grid;
            var horizontal = Window.Current.Bounds.Right - popupGrid.Width - 22;
            NewEntriesPopup.HorizontalOffset = horizontal;
            NewEntriesPopup.IsOpen = true;
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            HideHeader.Begin();
            AppBar.Hide();

            PopupFadeOut.Begin();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            ShowHeader.Begin();
            AppBar.Show();

            if (CanShowNewEntriesPopup)
                PopupFadeIn.Begin();
        }

        #region AppBar
        private void Observe_Loaded(object sender, RoutedEventArgs e)
        {
            var cacheVM = SimpleIoc.Default.GetInstance<CacheViewModel>();
            var mainVM = this.DataContext as MainViewModel;
            if (mainVM.SelectedHashtag != null && !string.IsNullOrEmpty(mainVM.SelectedHashtag.Hashtag) &&
                cacheVM.ObservedHashtags.Contains(mainVM.SelectedHashtag.Hashtag))
            {
                ObserveButton.IsChecked = true;
                ObserveButton.Label = "nie obserwuj";
            }
            else
            {
                ObserveButton.IsChecked = false;
                ObserveButton.Label = "obserwuj";
            }
        }

        private void Observe_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarToggleButton;
            var notificationsVM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            var mainVM = this.DataContext as MainViewModel;

            if(button.IsChecked.Value)
            {
                notificationsVM.ObserveHashtag.Execute(mainVM.SelectedHashtag.Hashtag);
                ObserveButton.Label = "nie obserwuj";
            }
            else
            {
                notificationsVM.UnobserveHashtag.Execute(mainVM.SelectedHashtag.Hashtag);
                ObserveButton.Label = "obserwuj";
            }
        }

        private void ScrollUp_Click(object sender, RoutedEventArgs e)
        {
            var sv = this.ListView.GetDescendant<ScrollViewer>();
            if (sv != null)
                sv.ChangeView(null, 0.0, null);
        }
        #endregion
    }


}
