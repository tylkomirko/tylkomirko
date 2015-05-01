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
using Windows.UI.Xaml.Navigation;
using Mirko_v2.Utils;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mirko_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public PivotHeaderPanel PivotHeader;
        public ItemsPresenter ItemsPresenter;

        private Storyboard HideHeader;
        private Storyboard ShowHeader;
        private Storyboard ShowPivotContent;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (PivotHeader == null)
            {
                PivotHeader = MainPivot.GetDescendant<PivotHeaderPanel>();
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
            }

            //if (!App.HasEntryAnimationPlayed)
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

        private void Entry_NavigateTo(object sender, PageNavigationEventArgs e)
        {
            /*
            if (NavigateTo != null)
                NavigateTo(sender, e);
             * */
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
    }
}
