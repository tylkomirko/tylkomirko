using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Controls;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class PivotPage : Page
    {
        private AppHeader AppHeader;

        public ItemsPresenter ItemsPresenter;
        private Storyboard ShowPivotContent;
        private bool HasEntryAnimationPlayed = false;
        private bool CanShowNewEntriesPopup = false;

        private double PreviousPivotOffset;
        private const double PivotOffsetThreshold = 10;

        private MainViewModel VM { get { return DataContext as MainViewModel; } }
        private SettingsViewModel SettingsVM { get { return SimpleIoc.Default.GetInstance<SettingsViewModel>(); } }

        public PivotPage()
        {
            this.InitializeComponent();

            var VM = this.DataContext as MainViewModel;
            VM.MirkoNewEntries.CollectionChanged -= MirkoNewEntries_CollectionChanged;
            VM.MirkoNewEntries.CollectionChanged += MirkoNewEntries_CollectionChanged;

            var navService = SimpleIoc.Default.GetInstance<NavigationService>();
            AppHeader = navService.GetAppHeader();

            Messenger.Default.Register<NotificationMessage<bool>>("MediaElement DoubleTapped", MediaElementDoubleTapped);
        }

        private void MediaElementDoubleTapped(NotificationMessage<bool> msg)
        {
            if (msg.Content)
            {
                // entered fullscreen
                if (MainPivot.SelectedIndex == 0)
                {
                    CanShowNewEntriesPopup = false;
                    NewMirkoEntriesPopupFadeOut.Begin();
                }
                else if(MainPivot.SelectedIndex == 1)
                {
                    TimeSpanIndicatorFadeOut.Begin();
                }
            }
            else
            {
                // left fullscreen
                if (MainPivot.SelectedIndex == 0)
                {
                    CanShowNewEntriesPopup = true;
                    if (VM.MirkoNewEntries.Count > 0)
                        NewMirkoEntriesPopupFadeIn.Begin();
                }
                else if (MainPivot.SelectedIndex == 1)
                {
                    TimeSpanIndicatorFadeIn.Begin();
                }
            }                
        }

        private void MirkoNewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(MainPivot.SelectedIndex != 0) return;

            var col = sender as ObservableCollectionEx<EntryViewModel>;
            if (col.Count > 0)
            {
                var sv = MirkoListView.GetDescendant<ScrollViewer>();

                if(AppBar.IsOpen || sv.VerticalOffset == 0)
                    NewMirkoEntriesPopupFadeIn.Begin();

                CanShowNewEntriesPopup = true;
            }
            else
            {
                CanShowNewEntriesPopup = false;
                NewMirkoEntriesPopupFadeOut.Begin();
            }
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainPivot.Margin.Top == 0)
            {
                var appHeaderHeight = AppHeader.ActualHeight;
                var statusBarHeight = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().OccludedRect.Height;
                var margin = appHeaderHeight + 2.5 * statusBarHeight;

                MainPivot.Margin = new Thickness(10, -margin, 0, 0);
                SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight = AppHeader.ActualHeight + statusBarHeight;
            }

            var scroll = MainPivot.GetDescendant<ScrollViewer>();
            scroll.ViewChanged -= ScrollViewer_ViewChanged;
            scroll.ViewChanged += ScrollViewer_ViewChanged;

            var VM = this.DataContext as MainViewModel;
            if (VM.MirkoNewEntries.Count > 0 && MainPivot.SelectedIndex == 0)
                CanShowNewEntriesPopup = true;
          
            if (scroll.VerticalOffset == 0 && CanShowNewEntriesPopup)
                NewMirkoEntriesPopupFadeIn.Begin();

            if (ItemsPresenter == null)
            {
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
                ShowPivotContent = ItemsPresenter.Resources["FadeIn"] as Storyboard;
            }

            if (!HasEntryAnimationPlayed)
                ItemsPresenter.Opacity = 0;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            var newOffset = sv.HorizontalOffset;
            if ((Math.Abs(PreviousPivotOffset - newOffset)) >= PivotOffsetThreshold)
                AppHeader.ShowStreams();

            PreviousPivotOffset = newOffset;
        }

        private void PivotPageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HasEntryAnimationPlayed)
            {
                ShowPivotContent.Begin();
                HasEntryAnimationPlayed = true;
            }

            var popupGrid = NewMirkoEntriesPopup.Child as Grid;
            var horizontal = Window.Current.Bounds.Right - popupGrid.Width - 22;
            NewMirkoEntriesPopup.HorizontalOffset = horizontal;
            TimeSpanIndicatorPopup.HorizontalOffset = horizontal;
            NewMirkoEntriesPopup.IsOpen = true;
            TimeSpanIndicatorPopup.IsOpen = true;
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0)
                NewMirkoEntriesPopupFadeOut.Begin();
            else if (currentPage == 1)
                TimeSpanIndicatorFadeOut.Begin();
            else if (currentPage == 3)
                HideMyEntriesIndicatorPopup();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0 && CanShowNewEntriesPopup)
                NewMirkoEntriesPopupFadeIn.Begin();
            else if (currentPage == 1)
                TimeSpanIndicatorFadeIn.Begin();
            else if (currentPage == 3)
                ShowMyEntriesIndicatorPopup();
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Mirko");

                if(CanShowNewEntriesPopup)
                    NewMirkoEntriesPopupFadeIn.Begin();

                AppBar.MakeButtonVisible("refresh");
                if (SettingsVM.UserInfo != null)
                    AppBar.MakeButtonVisible("add");
                TimeSpanIndicatorFadeOut.Begin();
                HideMyEntriesIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            }
            else if (currentPage == 1)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Hot");

                NewMirkoEntriesPopupFadeOut.Begin();
                AppBar.MakeButtonInvisible("refresh");
                AppBar.MakeButtonInvisible("add");
                TimeSpanIndicatorFadeIn.Begin();
                HideMyEntriesIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            } 
            else if(currentPage == 2)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Fav");

                NewMirkoEntriesPopupFadeOut.Begin();
                AppBar.MakeButtonInvisible("refresh");
                AppBar.MakeButtonInvisible("add");
                TimeSpanIndicatorFadeOut.Begin();
                HideMyEntriesIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            }
            else if (currentPage == 3)
            {
                App.TelemetryClient.TrackPageView("PivotPage-My");

                NewMirkoEntriesPopupFadeOut.Begin();
                AppBar.MakeButtonInvisible("refresh");
                AppBar.MakeButtonInvisible("add");
                TimeSpanIndicatorFadeOut.Begin();
                ShowMyEntriesIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.Start();
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as MainViewModel;
            var lv = sender as ListViewEx;

            lv.AppBar = AppBar;

            ObservableCollectionEx<EntryViewModel> items = null;
            string tag = (string)lv.Tag;
            if (tag == "LV0")
                items = VM.MirkoEntries;
            else if (tag == "LV1")
                items = VM.HotEntries;
            else if (tag == "LV2")
                items = VM.FavEntries;
            else if (tag == "LV3")
                items = VM.MyEntries;

            var idx = VM.IndexToScrollTo;
            if (idx != -1 && items != null && items.Count - 1 >= idx)
            {
                lv.ScrollIntoView(items[idx], ScrollIntoViewAlignment.Leading);
                VM.IndexToScrollTo = -1;
            }
        }

        private void SelectionPopup_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void SelectionPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        #region Hot time span popups
        private void TimeSpanIndicatorGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowTimeSpanSelectionPopup();
        }

        private void TimeSpanSelectionListView_Loaded(object sender, RoutedEventArgs e)
        {
            var selectedIndex = TimeSpanSelectionListView.SelectedIndex;
            var panel = TimeSpanSelectionListView.ItemsPanelRoot;

            for (int i = 0; i < panel.Children.Count; i++)
            {
                var lvItem = panel.Children[i] as ListViewItem;
                var tb = lvItem.GetDescendant<TextBlock>();

                if (i == selectedIndex)
                    tb.Foreground = App.Current.Resources["HashtagForeground"] as SolidColorBrush;
                else
                    tb.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            }
        }

        private async void TimeSpanSelectionListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            HideTimeSpanSelectionPopup();

            var selectedItem = e.ClickedItem as string;
            int selectedIndex = 3;

            var items = TimeSpanSelectionListView.Items.Cast<string>();
            for (int i = 0; i < items.Count(); i++)
                if (items.ElementAt(i) == selectedItem)
                    selectedIndex = i;

            var converter = Application.Current.Resources["HotTimeSpanIndexConverter"] as IValueConverter;
            var newTimeSpan = (int)converter.ConvertBack(selectedIndex, typeof(int), null, null);

            if (VM.HotTimeSpan != newTimeSpan)
            {
                VM.HotTimeSpan = newTimeSpan;
                VM.HotEntries.ClearAll();
                if (HotListView.ItemsSource != null) // forgive me for this dirty hack. it's Satya's fault.
                    await HotListView.LoadMoreItemsAsync();
            }
        }

        private void ShowTimeSpanSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.TimeSpanSelectionListView.Width = Window.Current.Bounds.Width;
            this.TimeSpanSelectionListView.Height = 5000;//Window.Current.Bounds.Height;

            this.TimeSpanSelectionPopup.IsOpen = true;
            //this.TimeSpanSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = false;
        }

        private void HideTimeSpanSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.TimeSpanSelectionPopup.IsOpen = false;
            //this.TimeSpanSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = true;
        }
        #endregion

        #region My entries type popups
        private void ShowMyEntriesIndicatorPopup()
        {
            this.MyEntriesIndicatorGrid.Width = Window.Current.Bounds.Width;

            this.MyEntriesIndicatorPopup.IsOpen = true;
            this.MyEntriesIndicatorFadeIn.Begin();
        }

        private void HideMyEntriesIndicatorPopup()
        {
            this.MyEntriesIndicatorPopup.IsOpen = false;
            this.MyEntriesIndicatorFadeOut.Begin();
        }

        private void MyEntriesIndicatorGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowMyEntriesSelectionPopup();
        }

        private void MyEntriesSelectionListView_Loaded(object sender, RoutedEventArgs e)
        {
            var selectedIndex = MyEntriesSelectionListView.SelectedIndex;
            var panel = MyEntriesSelectionListView.ItemsPanelRoot;

            for (int i = 0; i < panel.Children.Count; i++)
            {
                var lvItem = panel.Children[i] as ListViewItem;
                var tb = lvItem.GetDescendant<TextBlock>();

                if (i == selectedIndex)
                    tb.Foreground = App.Current.Resources["HashtagForeground"] as SolidColorBrush;
                else
                    tb.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            }
        }

        private async void MyEntriesSelectionListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            HideMyEntriesSelectionPopup();

            var selectedItem = e.ClickedItem as string;
            int selectedIndex = 0;

            var items = MyEntriesSelectionListView.Items.Cast<string>();
            for (int i = 0; i < items.Count(); i++)
                if (items.ElementAt(i) == selectedItem)
                    selectedIndex = i;

            var converter = Application.Current.Resources["MyEntriesTypeIndexConverter"] as IValueConverter;
            var newEntriesType = (MyEntriesTypeEnum)converter.ConvertBack(selectedIndex, typeof(MyEntriesTypeEnum), null, null);

            if (VM.MyEntriesType != newEntriesType)
            {
                VM.MyEntriesType = newEntriesType;
                VM.MyEntries.ClearAll();
                if (MyListView.ItemsSource != null) // forgive me for this dirty hack. it's Satya's fault.
                    await MyListView.LoadMoreItemsAsync();
            }
        }

        private void ShowMyEntriesSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.MyEntriesSelectionListView.Width = Window.Current.Bounds.Width;
            this.MyEntriesSelectionListView.Height = 5000;//Window.Current.Bounds.Height;

            this.MyEntriesSelectionPopup.IsOpen = true;
            //this.MyEntriesSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = false;
        }

        private void HideMyEntriesSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.MyEntriesSelectionPopup.IsOpen = false;
            //this.MyEntriesSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = true;
        }
        #endregion

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (TimeSpanSelectionPopup.IsOpen)
            {
                e.Handled = true;
                HideTimeSpanSelectionPopup();
            }
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer sv = null;
            var index = MainPivot.SelectedIndex;

            if (index == 0)
                sv = MirkoListView.GetDescendant<ScrollViewer>();
            else if (index == 1)
                sv = HotListView.GetDescendant<ScrollViewer>();

            if(sv != null)
                sv.ChangeView(null, 0.0, null);
        }
    }
}
