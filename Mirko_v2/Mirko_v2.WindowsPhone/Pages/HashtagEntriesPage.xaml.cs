using Mirko_v2.Controls;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class HashtagEntriesPage : UserControl, IHaveAppBar
    {
        private bool CanShowNewEntriesPopup = false;

        public HashtagEntriesPage()
        {
            this.InitializeComponent();

            var VM = this.DataContext as MainViewModel;
            VM.TaggedNewEntries.CollectionChanged += TaggedNewEntries_CollectionChanged;
        }

        private void TaggedNewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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

        private void ShowNewEntriesPopup()
        {
            this.PopupGrid.Width = Window.Current.Bounds.Width;

            this.NewEntriesPopup.IsOpen = true;
            this.PopupFadeIn.Begin();
        }

        private void HideNewEntriesPopup()
        {
            this.NewEntriesPopup.IsOpen = false;
            this.PopupFadeOut.Begin();
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            HideHeader.Begin();
            AppBar.Hide();

            HideNewEntriesPopup();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            ShowHeader.Begin();
            AppBar.Show();

            if (CanShowNewEntriesPopup)
                ShowNewEntriesPopup();
        }

        #region AppBar
        private CommandBar AppBar = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();

            var up = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Up),
                Label = "w górę",
            };
            up.Click += ScrollUp_Click;

            c.PrimaryCommands.Add(up);

            AppBar = c;
            return c;
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
