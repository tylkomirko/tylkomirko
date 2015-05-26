using Mirko_v2.Controls;
using Mirko_v2.Utils;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class HashtagEntriesPage : UserControl, IHaveAppBar
    {
        public HashtagEntriesPage()
        {
            this.InitializeComponent();
        }

        private void NewEntriesPopup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            App.MainViewModel.TaggedEntries.AddNewEntries();

            HideNewEntriesPopup();

            var sv = ListView.GetDescendant<ScrollViewer>();
            sv.ChangeView(null, 0.0, null);*/
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            HideHeader.Begin();
            AppBar.Hide();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            ShowHeader.Begin();
            AppBar.Show();
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
