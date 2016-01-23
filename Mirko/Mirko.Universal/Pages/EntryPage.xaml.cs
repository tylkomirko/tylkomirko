using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class EntryPage : Page
    {
        private MainViewModel VM
        {
            get { return DataContext as MainViewModel; }
        }

        private uint previousEntryID = 0;
        private double PreRefreshOffset = 0;

        public EntryPage()
        {
            this.InitializeComponent();

            this.Unloaded += (s, e) =>
            {
                HeaderCheckBox.IsChecked = false;
                HeaderCheckBox.Visibility = Visibility.Collapsed;
                ListView.SelectionMode = ListViewSelectionMode.None;
                var sv = ListView.GetDescendant<ScrollViewer>();
                if (sv != null)
                    PreRefreshOffset = sv.VerticalOffset;

                var vm = VM.SelectedEntry;
                if (vm != null)
                    previousEntryID = vm.Data.ID;
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
            var height = VM.ListViewHeaderHeight;
            var header = ListView.Header as FrameworkElement;
            var rect = header.GetDescendant<Rectangle>();
            rect.Height = height;
            ListView.Margin = new Thickness(0, -height, 0, 0);

            if (VM.CommentToScrollInto != null)
                ListView.ScrollIntoView(VM.CommentToScrollInto, ScrollIntoViewAlignment.Leading);
            else
            {
                var entryVM = VM.SelectedEntry;
                if (entryVM == null) return;
                if (entryVM.Data.ID != previousEntryID)
                    ListView.GetDescendant<ScrollViewer>().ChangeView(null, 0.0f, null);
            }
        }

        #region AppBar        
        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            var root = VM.SelectedEntry;
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
            var entryVM = VM.SelectedEntry;
            if (entryVM != null)
                entryVM.ShareCommand.Execute(null);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = VM.SelectedEntry;
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

        private void VoteMultiple_Click(object sender, RoutedEventArgs e)
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
                var entryvm = VM.SelectedEntry;
                selectedItems.Insert(0, entryvm);
            }

            return selectedItems;
        }

        private void ListView_ItemLeftEdgeTapped(ListView sender, Controls.ListViewEdgeTappedEventArgs e)
        {
            ListView.SelectionMode = ListViewSelectionMode.Multiple;
            ListView.IsItemLeftEdgeTapEnabled = false;

            HeaderCheckBox.Visibility = Visibility.Visible;
            HeaderEdgeButton.IsHitTestVisible = false;

            AppBar.MakeButtonInvisible("refresh");
            if (SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo != null)
                AppBar.MakeButtonVisible("voteMulti");
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!AnyItemsChecked())
            {
                HeaderCheckBox.Visibility = Visibility.Collapsed;
                HeaderCheckBox.IsChecked = false;
                HeaderEdgeButton.IsHitTestVisible = true;

                ListView.IsItemLeftEdgeTapEnabled = true;
                ListView.SelectionMode = ListViewSelectionMode.None;

                AppBar.MakeButtonVisible("refresh");
                AppBar.MakeButtonInvisible("voteMulti");
            }
        }

        private void EdgeSelectButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HeaderCheckBox.Visibility = Visibility.Visible;
            HeaderCheckBox.IsChecked = true;

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

            HeaderCheckBox.Visibility = Visibility.Collapsed;
            HeaderEdgeButton.IsHitTestVisible = true;
            ListView.SelectionMode = ListViewSelectionMode.None;
            ListView.IsItemLeftEdgeTapEnabled = true;

            AppBar.MakeButtonVisible("refresh");
            AppBar.MakeButtonInvisible("voteMulti");
        }
        #endregion
    }
}
