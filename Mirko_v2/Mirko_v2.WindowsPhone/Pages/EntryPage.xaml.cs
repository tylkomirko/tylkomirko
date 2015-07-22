using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using QKit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class EntryPage : UserControl, IHaveAppBar
    {
        public EntryPage()
        {
            this.InitializeComponent();

            this.Unloaded += (s, e) =>
            {
                HeaderCheckBox.IsChecked = false;
                HeaderCheckBox.Visibility = Visibility.Collapsed;
                ListView.SelectionMode = ListViewSelectionMode.None;
            };
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var mainVM = this.DataContext as MainViewModel;

            var height = mainVM.ListViewHeaderHeight;
            ListView.Margin = new Thickness(0, -height, 0, 10);
            var header = ListView.Header as FrameworkElement;
            var rect = header.GetDescendant<Rectangle>();
            rect.Height = height;

            if (mainVM.CommentToScrollInto != null)
                ListView.ScrollIntoView(mainVM.CommentToScrollInto, ScrollIntoViewAlignment.Leading);
        }

        #region AppBar
        private CommandBar AppBar = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();

            // buttons only visible in comment selection mode
            var comment = new AppBarButton()
            {
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/reply.png") },
                Label = "odpowiedz",
                Tag = "reply",
            };
            comment.Click += CommentButton_Click;

            var delete = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "usuń",
                Tag = "delete",
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };

            // regular buttons
            var refresh = new AppBarButton()
            {
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/refresh.png") },
                Label = "odśwież",
                Tag = "refresh"
            };
            refresh.Click += RefreshButton_Click;

            var up = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Up),
                Label = "w górę",
                Tag = "up"
            };
            up.Click += ScrollUpButton_Click;

            var voteMultiple = new AppBarButton()
            {
                Label = "daj plusa",
                Tag = "voteMulti",
                Icon = new SymbolIcon(Symbol.Add),
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };
            voteMultiple.Click += (s, e) =>
            {
                var selectedItems = SelectedItems();
                var vm = selectedItems.First();
                vm.VoteMultiple.Execute(selectedItems);

                ListView.SelectionMode = ListViewSelectionMode.None;
                HeaderCheckBox.IsChecked = false;
                HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                HeaderEdgeButton.IsHitTestVisible = true;
            };

            var replyMultiple = new AppBarButton()
            {
                Label = "odpowiedz",
                Tag = "replyMulti",
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/reply.png") },
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };
            replyMultiple.Click += (s, e) =>
            {
                var selectedItems = SelectedItems();
                var root = this.ListView.DataContext as EntryViewModel;
                var vm = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
                vm.NewEntry.IsEditing = false;
                vm.NewEntry.CommentID = 0;
                vm.NewEntry.EntryID = root.Data.ID;
                vm.GoToNewEntryPage(selectedItems);

                ListView.SelectionMode = ListViewSelectionMode.None;
                HeaderCheckBox.IsChecked = false;
                HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                HeaderEdgeButton.IsHitTestVisible = true;
            };

            c.PrimaryCommands.Add(comment);
            c.PrimaryCommands.Add(delete);
            c.PrimaryCommands.Add(refresh);
            c.PrimaryCommands.Add(replyMultiple);
            c.PrimaryCommands.Add(voteMultiple);
            c.PrimaryCommands.Add(up);

            var share = new AppBarButton()
            {
                Label = "udostępnij"
            };
            share.Click += ShareButton_Click;

            c.SecondaryCommands.Add(share);

            AppBar = c;

            return c;
        }

        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            var entryVM = this.ListView.DataContext as EntryViewModel;
            if (entryVM != null)
                entryVM.ReplyCommand.Execute(null);
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
            if(entryVM != null)
                entryVM.RefreshCommand.Execute(null);
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            var sv = this.ListView.GetDescendant<ScrollViewer>();
            if (sv != null)
                sv.ChangeView(null, 0.0, null);
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
                AppBar.MakeButtonInvisible("reply");
                AppBar.MakeButtonVisible("replyMulti");
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
                    AppBar.MakeButtonVisible("reply");
                    AppBar.MakeButtonInvisible("replyMulti");
                    AppBar.MakeButtonInvisible("voteMulti");
                }
            }
        }

        private void EdgeSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var edgeButton = sender as EdgeSelectButton;
            var stackPanel = edgeButton.GetAntecedent<StackPanel>();
            var checkBox = stackPanel.GetDescendant<CheckBox>();

            HeaderCheckBox.IsChecked = true;
            HeaderCheckBox.Visibility = Windows.UI.Xaml.Visibility.Visible;

            ListView.SelectionMode = ListViewSelectionMode.Multiple;
            HeaderEdgeButton.IsHitTestVisible = false;

            AppBar.MakeButtonInvisible("refresh");
            AppBar.MakeButtonInvisible("reply");
            AppBar.MakeButtonVisible("replyMulti");
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
            AppBar.MakeButtonVisible("reply");
            AppBar.MakeButtonInvisible("replyMulti");
            AppBar.MakeButtonInvisible("voteMulti");
        }
        #endregion
    }
}
