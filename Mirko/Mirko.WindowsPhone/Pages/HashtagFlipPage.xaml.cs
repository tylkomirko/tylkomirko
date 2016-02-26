using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Mirko.ViewModel;
using QKit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class HashtagFlipPage : Page
    {
        private double HeaderHeight;
        private double PreRefreshOffset = 0;

        public HashtagFlipPage()
        {
            this.InitializeComponent();

            HeaderHeight = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight + 49;
            FlipView.Margin = new Thickness(0, -HeaderHeight, 0, 0);

            this.Unloaded += (s, e) =>
            {
                var lv = CurrentListView();
                if (lv != null)
                    PreRefreshOffset = lv.GetDescendant<ScrollViewer>().VerticalOffset;
            };

            Messenger.Default.Register<EntryViewModel>(this, "HashtagFlipEntries Updated", ReadMessage);
        }

        private void ReadMessage(EntryViewModel e)
        {
            var lv = CurrentListView();
            if (lv != null)
            {
                var scrollViewer = lv.GetDescendant<ScrollViewer>();
                scrollViewer.ChangeView(null, PreRefreshOffset, null, false);
                PreRefreshOffset = 0;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = sender as ListView;
            var sp = lv.Header as Grid;
            var rect = sp.GetDescendant<Rectangle>();
            rect.Height = HeaderHeight;
        }

        private ListView CurrentListView()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            return item != null ? item.ContentTemplateRoot as ListView : null;
        }

        private EdgeSelectButton CurrentHeaderEdgeButton()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var lv = item.ContentTemplateRoot as ListView;
            var lvHead = lv.Header as Grid;
            return lvHead.GetDescendant<EdgeSelectButton>();
        }

        private CheckBox CurrentHeaderCheckBox()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var lv = item.ContentTemplateRoot as ListView;
            var lvHead = lv.Header as Grid;
            return lvHead.GetDescendant<CheckBox>();
        }

        #region AppBar
        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            var LV = CurrentListView();
            var root = LV.DataContext as EntryViewModel;
            var vm = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            var selectedItems = SelectedItems();

            if (selectedItems.Count == 0) // in other words - item selection mode is not turned on
                selectedItems.Add(root);

            vm.NewEntry.IsEditing = false;
            vm.NewEntry.CommentID = 0;
            vm.NewEntry.EntryID = root.Data.ID;
            vm.GoToNewEntryPage(selectedItems);

            LV.SelectionMode = ListViewSelectionMode.None;
            CurrentHeaderCheckBox().IsChecked = false;
            CurrentHeaderCheckBox().Visibility = Visibility.Collapsed;
            CurrentHeaderEdgeButton().IsHitTestVisible = true;
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = CurrentListView().DataContext as EntryViewModel;
            if (entryVM != null)
                entryVM.ShareCommand.Execute(null);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = CurrentListView().DataContext as EntryViewModel;
            if (entryVM != null)
            {
                PreRefreshOffset = CurrentListView().GetDescendant<ScrollViewer>().VerticalOffset;
                entryVM.RefreshCommand.Execute(null);
            }
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            var sv = CurrentListView().GetDescendant<ScrollViewer>();
            if (sv != null)
                sv.ChangeView(null, 0.0, null, false);
        }

        private void VoteMultipleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SelectedItems();
            var vm = selectedItems.First();
            vm.VoteMultiple.Execute(selectedItems);

            CurrentListView().SelectionMode = ListViewSelectionMode.None;
            CurrentHeaderCheckBox().IsChecked = false;
            CurrentHeaderCheckBox().Visibility = Visibility.Collapsed;
            CurrentHeaderEdgeButton().IsHitTestVisible = true;
        }
        #endregion

        #region Multiselect
        private bool AnyItemsChecked()
        {
            return CurrentListView().SelectedItems.Count > 0 || CurrentHeaderCheckBox().IsChecked.Value;
        }

        private List<EntryBaseViewModel> SelectedItems()
        {
            var selectedItems = CurrentListView().SelectedItems.Cast<EntryBaseViewModel>().ToList();
            if (CurrentHeaderCheckBox().IsChecked.Value)
            {
                var entryvm = CurrentListView().DataContext as EntryViewModel;
                selectedItems.Insert(0, entryvm);
            }

            return selectedItems;
        }

        private void ListView_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            var LV = sender as ListView;
            if (LV.SelectionMode == ListViewSelectionMode.Multiple)
            {
                CurrentHeaderCheckBox().Visibility = Visibility.Visible;
                CurrentHeaderEdgeButton().IsHitTestVisible = false;

                AppBar.MakeButtonInvisible("refresh");
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo != null)
                    AppBar.MakeButtonVisible("voteMulti");
            }
            else if (LV.SelectionMode == ListViewSelectionMode.None)
            {
                if (AnyItemsChecked())
                {
                    LV.SelectionMode = ListViewSelectionMode.Multiple;
                }
                else
                {
                    CurrentHeaderCheckBox().Visibility = Visibility.Collapsed;
                    CurrentHeaderCheckBox().IsChecked = false;
                    CurrentHeaderEdgeButton().IsHitTestVisible = true;

                    AppBar.MakeButtonVisible("refresh");
                    AppBar.MakeButtonInvisible("voteMulti");
                }
            }
        }

        private void EdgeSelectButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentHeaderCheckBox().IsChecked = true;
            CurrentHeaderCheckBox().Visibility = Visibility.Visible;

            CurrentListView().SelectionMode = ListViewSelectionMode.Multiple;
            CurrentHeaderEdgeButton().IsHitTestVisible = false;

            AppBar.MakeButtonInvisible("refresh");
            if (SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo != null)
                AppBar.MakeButtonVisible("voteMulti");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AnyItemsChecked())
                return;

            CurrentHeaderCheckBox().Visibility = Visibility.Collapsed;
            CurrentHeaderEdgeButton().IsHitTestVisible = true;
            CurrentListView().SelectionMode = ListViewSelectionMode.None;

            AppBar.MakeButtonVisible("refresh");
            AppBar.MakeButtonInvisible("voteMulti");
        }
        #endregion

        // I really wanted to use XAML behaviour (like in UWP), but sometimes leads to System.InvalidCastException.
        // Maybe this will help.
        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = FlipView?.SelectedIndex ?? 0;
            var vm = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            vm.HashtagFlipSelectionChanged.Execute(index);
        }
    }
}
