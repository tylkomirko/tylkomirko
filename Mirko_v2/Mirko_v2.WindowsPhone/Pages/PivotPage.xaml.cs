using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Mirko_v2.Utils;
using System.Threading.Tasks;
using Mirko_v2.ViewModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class PivotPage : UserControl, IHaveAppBar
    {
        public ItemsPresenter ItemsPresenter;

        private Storyboard ShowPivotContent;

        private bool HasEntryAnimationPlayed = false;

        public PivotPage()
        {
            this.InitializeComponent();
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsPresenter == null)
            {
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
            }

            //if (!HasEntryAnimationPlayed)
            //    ItemsPresenter.Opacity = 0;

            if (ShowPivotContent == null)
                ShowPivotContent = ItemsPresenter.Resources["FadeIn"] as Storyboard;

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
                App.HasEntryAnimationPlayed = true;
            }
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {

            /*
            var CurrentPage = MainPivot.SelectedIndex;
            if (CurrentPage == 0)
                HideNewEntriesPopup();
            else if (CurrentPage == 1)
                HideHotPopup();
             * */
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {

            /*
            if (CurrentPage == 0 && App.MainViewModel.MirkoNewEntries.Count > 0)
                ShowNewEntriesPopup();
            else if (CurrentPage == 1)
                ShowHotPopup();
             * */
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar() { IsOpen = true };
            var up = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Up),
                Label = "w górę",
            };
            up.Click += ScrollUpButton_Click;

            c.PrimaryCommands.Add(up);
            return c;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as MainViewModel;
            var lv = sender as ListView;
            var items = VM.MirkoEntries;

            var idx = VM.IndexToScrollTo;
            if (idx != -1 && items.Count - 1 >= idx)
            {
                lv.ScrollIntoView(items[idx], ScrollIntoViewAlignment.Leading);
                VM.IndexToScrollTo = -1;
            }
        }
    }
}
