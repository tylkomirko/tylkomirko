using Mirko_v2.ViewModel;
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
using Mirko_v2.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class EntryPage : UserControl, IHaveAppBar
    {
        public EntryPage()
        {
            this.InitializeComponent();
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var mainVM = this.DataContext as MainViewModel;
            if (mainVM.CommentToScrollInto != null)
                ListView.ScrollIntoView(mainVM.CommentToScrollInto, ScrollIntoViewAlignment.Leading);
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            AppBar.Hide();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            AppBar.Show();
        }

        private void ListView_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            if(ListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                //AppBar.MakeButtonInvisible();
            }
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
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };

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

            var add = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "nowy",
            };

            c.PrimaryCommands.Add(comment);
            c.PrimaryCommands.Add(delete);
            //c.PrimaryCommands.Add(add);
            c.PrimaryCommands.Add(refresh);
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
    }
}
