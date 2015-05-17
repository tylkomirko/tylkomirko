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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class PivotPage : UserControl
    {
        public PivotHeaderPanel PivotHeader;
        public ItemsPresenter ItemsPresenter;

        private Storyboard HideHeader;
        private Storyboard ShowHeader;
        private Storyboard ShowPivotContent;

        private bool HasEntryAnimationPlayed = false;

        public PivotPage()
        {
            this.InitializeComponent();
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (PivotHeader == null)
            {
                PivotHeader = MainPivot.GetDescendant<PivotHeaderPanel>();
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
            }

            //if (!HasEntryAnimationPlayed)
            //    ItemsPresenter.Opacity = 0;

            // create hide animations
            if (HideHeader == null)
                HideHeader = PivotHeader.Resources["HideHeader"] as Storyboard;

            // create show animations
            if (ShowHeader == null)
                ShowHeader = PivotHeader.Resources["ShowHeader"] as Storyboard;

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
                ShowHeader.SpeedRatio = .5;
                ShowHeader.Begin();

                App.HasEntryAnimationPlayed = true;
            }
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            HideHeader.Begin();

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
            ShowHeader.Begin();

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
    }
}
