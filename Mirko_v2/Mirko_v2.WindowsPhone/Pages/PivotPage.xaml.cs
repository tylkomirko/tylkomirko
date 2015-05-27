using Mirko_v2.Controls;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class PivotPage : UserControl, IHaveAppBar
    {
        public ItemsPresenter ItemsPresenter;
        private Storyboard ShowPivotContent;
        private bool HasEntryAnimationPlayed = false;
        private bool CanShowNewEntriesPopup = false;

        public PivotPage()
        {
            this.InitializeComponent();

            var VM = this.DataContext as MainViewModel;
            VM.MirkoNewEntries.CollectionChanged += MirkoNewEntries_CollectionChanged;
        }

        private void MirkoNewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(MainPivot.SelectedIndex != 0) return;

            var col = sender as ObservableCollectionEx<EntryViewModel>;
            if (col.Count > 0)
            {
                CanShowNewEntriesPopup = true;
                ShowNewEntriesPopup();
            }
            else
            {
                CanShowNewEntriesPopup = false;
                HideNewEntriesPopup();
            }
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsPresenter == null)
            {
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
                ShowPivotContent = ItemsPresenter.Resources["FadeIn"] as Storyboard;
            }

            if (!HasEntryAnimationPlayed)
                ItemsPresenter.Opacity = 0;

            /*
            if (pivot.SelectedIndex == 0)
                App.MainViewModel.ApiAddNewEntries();
            else if (pivot.SelectedIndex == 1 && PivotHeader.Opacity != 0)
                ShowHotPopup();
             * */
        }

        private void PivotPageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HasEntryAnimationPlayed)
            {
                ShowPivotContent.Begin();
                HasEntryAnimationPlayed = true;
            }
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            AppBar.Hide();

            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0)
                HideNewEntriesPopup();
            //else if (CurrentPage == 1)
            //    HideHotPopup();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            AppBar.Show();

            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0 && CanShowNewEntriesPopup)
                ShowNewEntriesPopup();
            //else if (currentPage == 1)
            //    HideHotPopup();
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as MainViewModel;
            var lv = sender as ListView;

            ObservableCollectionEx<EntryViewModel> items;
            if ((string)lv.Tag == "LV0")
                items = VM.MirkoEntries;
            else
                return;

            var idx = VM.IndexToScrollTo;
            if (idx != -1 && items.Count - 1 >= idx)
            {
                lv.ScrollIntoView(items[idx], ScrollIntoViewAlignment.Leading);
                VM.IndexToScrollTo = -1;
            }
        }

        #region Popups
        private void ShowNewEntriesPopup()
        {
            this.PopupGrid.Width = Window.Current.Bounds.Width;

            this.NewMirkoEntriesPopup.IsOpen = true;
            this.PopupFadeIn.Begin();
        }

        private void HideNewEntriesPopup()
        {
            this.NewMirkoEntriesPopup.IsOpen = false;

            this.PopupFadeOut.Begin();
        }
        #endregion

        #region AppBar
        private CommandBar AppBar = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar() { IsOpen = true };
            var up = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Up),
                Label = "w górę",
            };
            up.Click += ScrollUpButton_Click;

            var add = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "nowy",
            };

            c.PrimaryCommands.Add(add);
            c.PrimaryCommands.Add(up);
            AppBar = c;

            return c;
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
        #endregion
    }
}
