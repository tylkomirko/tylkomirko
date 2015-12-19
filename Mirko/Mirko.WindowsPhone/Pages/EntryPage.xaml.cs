using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Mirko.ViewModel;
using QKit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class EntryPage : Page
    {
        private double PreRefreshOffset = 0;

        public EntryPage()
        {
            this.InitializeComponent();

            this.Unloaded += (s, e) =>
            {
                HeaderCheckBox.IsChecked = false;
                HeaderCheckBox.Visibility = Visibility.Collapsed;
                ListView.SelectionMode = ListViewSelectionMode.None;

                PreRefreshOffset = ListView.GetDescendant<ScrollViewer>().VerticalOffset;
            };

            Messenger.Default.Register<EntryViewModel>(this, "Updated", (e) =>
            {
                var scrollViewer = ListView.GetDescendant<ScrollViewer>();
                scrollViewer.ChangeView(null, PreRefreshOffset, null, false);
                PreRefreshOffset = 0;
            });
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var mainVM = this.DataContext as MainViewModel;

            var height = mainVM.ListViewHeaderHeight;
            var header = ListView.Header as FrameworkElement;
            var rect = header.GetDescendant<Rectangle>();
            rect.Height = height;
            /* It would make much more sense to set ListView.Margin, but that causes a bug.
             * When user navigates for the first time, margin seems to be not set.
             * It must be because of QKit */
            this.Margin = new Thickness(0, -height, 0, 0);

            if (mainVM.CommentToScrollInto != null)
                ListView.ScrollIntoView(mainVM.CommentToScrollInto, ScrollIntoViewAlignment.Leading);
        }

        #region AppBar
        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            var root = this.ListView.DataContext as EntryViewModel;
            var vm = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            var selectedItems = SelectedItems();

            if (selectedItems.Count == 0) // in other words - item selection mode is not turned on
                selectedItems.Add(root);

            vm.NewEntry.IsEditing = false;
            vm.NewEntry.CommentID = 0;
            vm.NewEntry.EntryID = root.Data.ID;
            vm.GoToNewEntryPage(selectedItems);

            ListView.SelectionMode = ListViewSelectionMode.None;
            HeaderCheckBox.IsChecked = false;
            HeaderCheckBox.Visibility = Visibility.Collapsed;
            HeaderEdgeButton.IsHitTestVisible = true;
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = this.ListView.DataContext as EntryViewModel;
            if (entryVM != null)
                entryVM.ShareCommand.Execute(null);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = this.ListView.DataContext as EntryViewModel;
            if (entryVM != null)
            {
                PreRefreshOffset = ListView.GetDescendant<ScrollViewer>().VerticalOffset;
                entryVM.RefreshCommand.Execute(null);
            }
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            var sv = this.ListView.GetDescendant<ScrollViewer>();
            if (sv != null)
                sv.ChangeView(null, 0.0, null, false);
        }

        private void VoteMultipleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SelectedItems();
            var vm = selectedItems.First();
            vm.VoteMultiple.Execute(selectedItems);

            ListView.SelectionMode = ListViewSelectionMode.None;
            HeaderCheckBox.IsChecked = false;
            HeaderCheckBox.Visibility = Visibility.Collapsed;
            HeaderEdgeButton.IsHitTestVisible = true;
        }
        #endregion

        #region Multiselect
        private bool AnyItemsChecked()
        {
            return ListView.SelectedItems.Count > 0 || HeaderCheckBox.IsChecked.Value;
        }

        private List<EntryBaseViewModel> SelectedItems()
        {
            var selectedItems = ListView.SelectedItems.Cast<EntryBaseViewModel>().ToList();
            if (HeaderCheckBox.IsChecked.Value)
            {
                var entryvm = this.ListView.DataContext as EntryViewModel;
                selectedItems.Insert(0, entryvm);
            }

            return selectedItems;
        }

        private void ListView_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                HeaderEdgeButton.IsHitTestVisible = false;

                AppBar.MakeButtonInvisible("refresh");
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo != null)
                    AppBar.MakeButtonVisible("voteMulti");
            }
            else if (ListView.SelectionMode == ListViewSelectionMode.None)
            {
                if (AnyItemsChecked())
                {
                    ListView.SelectionMode = ListViewSelectionMode.Multiple;
                }
                else
                {
                    HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    HeaderCheckBox.IsChecked = false;
                    HeaderEdgeButton.IsHitTestVisible = true;

                    AppBar.MakeButtonVisible("refresh");
                    AppBar.MakeButtonInvisible("voteMulti");
                }
            }
        }

        private void EdgeSelectButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderCheckBox.IsChecked = true;
            HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Visible;

            ListView.SelectionMode = ListViewSelectionMode.Multiple;
            HeaderEdgeButton.IsHitTestVisible = false;

            AppBar.MakeButtonInvisible("refresh");
            if (SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo != null)
                AppBar.MakeButtonVisible("voteMulti");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AnyItemsChecked())
                return;

            HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            HeaderEdgeButton.IsHitTestVisible = true;
            ListView.SelectionMode = ListViewSelectionMode.None;

            AppBar.MakeButtonVisible("refresh");
            AppBar.MakeButtonInvisible("voteMulti");
        }
        #endregion
    }
}
